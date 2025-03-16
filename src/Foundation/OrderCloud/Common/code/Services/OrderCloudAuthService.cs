using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Molla.Foundation.OrderCloud.Models.Models;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public class OrderCloudAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _accessToken;
        private DateTime _tokenExpiry;

        public OrderCloudAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _accessToken; // Return existing token if still valid
            }

            return await FetchNewAccessTokenAsync();
        }

        private async Task<string> FetchNewAccessTokenAsync()
        {
            var clientId = _configuration["OrderCloud:ClientId"];
            var username = _configuration["OrderCloud:Username"];
            var password = _configuration["OrderCloud:Password"];
            var tokenUrl = _configuration["OrderCloud:TokenUrl"];
            var scope = _configuration["OrderCloud:Scope"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tokenUrl))
            {
                throw new InvalidOperationException("Missing OrderCloud authentication configuration.");
            }

            var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
            { "scope", scope }
        };

            var requestContent = new FormUrlEncodedContent(tokenRequest);

            try
            {
                var response = await _httpClient.PostAsync(tokenUrl, requestContent);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<OrderCloudAuthResponse>(content);

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 min before expiry

                Console.WriteLine("Access token obtained successfully.");
                return _accessToken;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching access token: {ex.Message}");
                throw;
            }
        }

        public async Task SetAuthorizationHeaderAsync()
        {
            string token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}