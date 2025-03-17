using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Sitecore.Diagnostics;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public static class HttpClientSingleton
    {
        private static readonly HttpClient _httpClient;

        static HttpClientSingleton()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a default timeout
        }

        public static HttpClient Instance => _httpClient;
    }

    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly Policy _retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpService"/> class.
        /// </summary>
        /// <param name="defaultHeaders">Default headers to include in all requests.</param>
        /// <param name="token">Authorization token (optional).</param>
        public HttpService(Dictionary<string, string> defaultHeaders = null, string token = null)
        {
            _httpClient = HttpClientSingleton.Instance; // Use the singleton HttpClient

            // Set default headers if provided
            if (defaultHeaders != null)
            {
                foreach (var header in defaultHeaders)
                {
                    // Check if the header already exists
                    if (_httpClient.DefaultRequestHeaders.Contains(header.Key))
                    {
                        // Remove the existing header
                        _httpClient.DefaultRequestHeaders.Remove(header.Key);
                    }
                    // Add the new header
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            // Set authorization token if provided
            if (!string.IsNullOrEmpty(token))
            {
                // Check if the Authorization header already exists
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    // Remove the existing Authorization header
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                // Add the new Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Configure retry policy using Polly v6
            _retryPolicy = Policy
                .Handle<HttpRequestException>() // Retry on HTTP request exceptions
                .Or<TaskCanceledException>()   // Retry on timeouts
                .WaitAndRetryAsync(
                    retryCount: 3, // Retry 3 times
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetryAsync: async (exception, timespan, retryCount, context) =>
                    {
                        Log.Warn($"Retry {retryCount} due to {exception.Message}", this);
                        await Task.CompletedTask; // Ensure async compatibility
                    });
        }

        /// <summary>
        /// Sends a GET request to the specified URL.
        /// </summary>
        /// <typeparam name="T">The type of the response.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="queryParams">Optional query parameters to include in the request.</param>
        /// <returns>The deserialized response object.</returns>
        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> queryParams = null)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                // Append query parameters to the URL if provided
                if (queryParams != null)
                {
                    var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    url = $"{url}?{queryString}";
                }

                Log.Info($"Sending GET request to {url}", this);
                var response = _httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                Log.Info($"Received response: {content}", this);
                return JsonConvert.DeserializeObject<T>(content);
            });
        }

        /// <summary>
        /// Sends a POST request to the specified URL.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The request body data.</param>
        /// <returns>The deserialized response object.</returns>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Log.Info($"Sending POST request to {url} with data: {json}", this);
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                Log.Info($"Received response: {responseContent}", this);
                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            });
        }

        /// <summary>
        /// Sends a PUT request to the specified URL.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The request body data.</param>
        /// <returns>The deserialized response object.</returns>
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Log.Info($"Sending PUT request to {url} with data: {json}", this);
                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                Log.Info($"Received response: {responseContent}", this);
                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            });
        }

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        public async Task DeleteAsync(string url)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                Log.Info($"Sending DELETE request to {url}", this);
                var response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                Log.Info($"DELETE request successful", this);
            });
        }

        /// <summary>
        /// Adds or updates a default header for all requests.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header value.</param>
        public void SetDefaultHeader(string key, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(key))
            {
                _httpClient.DefaultRequestHeaders.Remove(key);
            }
            _httpClient.DefaultRequestHeaders.Add(key, value);
        }
    }
}