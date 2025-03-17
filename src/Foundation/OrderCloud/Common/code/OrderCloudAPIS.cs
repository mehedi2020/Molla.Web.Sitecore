namespace Molla.Foundation.OrderCloud.Common
{
    public sealed class OrderCloudAPIS
    {
        private const string BaseUrl = "https://sandboxapi.ordercloud.io/v1";

        public static string GetCatalogEndpoint() => $"{BaseUrl}/catalogs";
        public static string GetProductEndpoint() => $"{BaseUrl}/products";
        public static string GetSuppliersEndpoint() => $"{BaseUrl}/suppliers";

        public static readonly string _tokenUrl = "https://sandboxapi.ordercloud.io/oauth/token"; // OrderCloud OAuth token endpoint 

    }


}