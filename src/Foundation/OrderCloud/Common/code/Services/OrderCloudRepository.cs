using Molla.Foundation.OrderCloud.Models;
using Molla.Foundation.OrderCloud.Models.Models;
using Sitecore.XA.Foundation.IoC;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using Sitecore.XA.Foundation.RenderingVariants.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Molla.Foundation.OrderCloud.Common.Services
{

    public class OrderCloudRepository : ListRepository, IOrderCloudRepository, IModelRepository, IControllerRepository, IAbstractRepository<IRenderingModelBase>
    {
        private readonly HttpService _httpService;
        public OrderCloudRepository(HttpService httpService)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            string authorization = OrderCloudAuthService.GetApiTokenAsync(); 
            keyValuePairs.Add("ContentType", "application/json");
            _httpService = new HttpService(keyValuePairs, authorization);
        }

      

        public async Task<CategoryResponse> GetCategoriesAsyncForHome(string catalogName)
        {
            string url = OrderCloudAPIS.GetCategoriesByCatalogNameEndpoint(catalogName) + "?filters=xp.DisplayHomePage=true";  // Set the endpoint for Categories by CatalogName
            return await _httpService.GetAsync<CategoryResponse>(url);
        }

        // Fetch suppliers from OrderCloud API
        public async Task<SupplierListResponse> GetSuppliersAsync()
        {
            string url = OrderCloudAPIS.GetSuppliersEndpoint();  // Set the endpoint for suppliers
            return await _httpService.GetAsync<SupplierListResponse>(url);
        } 
       
    }

}