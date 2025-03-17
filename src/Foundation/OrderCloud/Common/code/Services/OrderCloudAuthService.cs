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
using Microsoft.Extensions.Configuration;
using System.Text;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Diagnostics;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public class OrderCloudAuthService
    {
        private static readonly string _tokenUrl = OrderCloudAPIS._tokenUrl; // OrderCloud OAuth token endpoint
        private static readonly string _clientId = "F9B6874F-59DA-42D3-8B55-8181E7837787"; // Replace with your client ID
        private static readonly string _clientSecret = "YOUR_CLIENT_SECRET"; // Replace with your client secret
        private static readonly string _clientUserName = "admin01"; // Replace with your client secret
        private static readonly string _clientPassword = "Caasco_12345"; // Replace with your client secret
        private static readonly string _scope = "CatalogAdmin BuyerReader MeAdmin InventoryAdmin PasswordReset OrderAdmin PriceScheduleAdmin ProductAdmin ProductAssignmentAdmin ShipmentAdmin SupplierAdmin SupplierReader"; // Replace with the required scope (e.g., "OrderCloud.Supplier")

        public static string GetApiTokenAsync()
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientUserName) ||
                string.IsNullOrEmpty(_clientPassword) || string.IsNullOrEmpty(_scope) || string.IsNullOrEmpty(_tokenUrl))
            {
                throw new ArgumentException("All parameters must be provided and non-empty.");
            }

            // Use a static HttpClient instance (best practice)
            using (var httpClient = new HttpClient())
            {
                // Set headers
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Accept header

                // Prepare the request body (client credentials)
                var requestBody = $"grant_type=password&client_id={_clientId}&username={_clientUserName}&password={_clientPassword}&scope={_scope}";
                HttpContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

                try
                {
                    // Send the POST request
                    var response =   httpClient.PostAsync(_tokenUrl, requestContent).Result;

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Parse the response content
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var tokenResponse = JsonConvert.DeserializeObject<OrderCloudAuthResponse>(responseContent);

                    // Return the access token
                    return tokenResponse.AccessToken;
                }
                catch (HttpRequestException ex)
                {
                    Log.Info("Failed to retrieve the API token. Please check your credentials and try again.", "");
                }
                catch (JsonException ex)
                {
                    Log.Info("Failed to parse the API token response.", "");
                }
                catch (Exception ex)
                {
                    Log.Info("An unexpected error occurred while retrieving the API token.", "");
                }

                return null;
            }
        }


    }
}