﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout

            // Configure retry policy
            _retryPolicy = Polly.Policy
                .Handle<HttpRequestException>() // Retry on HTTP request exceptions
                .Or<TaskCanceledException>()   // Retry on timeouts
                .WaitAndRetryAsync(
                    retryCount: 3, // Retry 3 times
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Warn($"Retry {retryCount} due to {exception.Message}", this);
                    });
        }

        // Generic GET method with retry
        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> queryParams = null)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                if (queryParams != null)
                {
                    var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    url = $"{url}?{queryString}";
                }

                Log.Info($"Sending GET request to {url}", this);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Log.Info($"Received response: {content}", this);
                return JsonConvert.DeserializeObject<T>(content);
            });
        }

        // Generic POST method with retry
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

        // Generic PUT method with retry
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

        // Generic DELETE method with retry
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

        // Set custom headers
        public void SetHeader(string key, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(key))
            {
                _httpClient.DefaultRequestHeaders.Remove(key);
            }
            _httpClient.DefaultRequestHeaders.Add(key, value);
        }
    }
}