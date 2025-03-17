using Molla.Foundation.OrderCloud.Models.Models;
using Sitecore.XA.Foundation.IoC;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using Sitecore.XA.Foundation.RenderingVariants.Lists.Pagination;
using Sitecore.XA.Foundation.RenderingVariants.Models;
using Sitecore.XA.Foundation.RenderingVariants.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Molla.Foundation.OrderCloud.Common.Services
{
   
    public class OrderCloudRepository :  ListRepository, IOrderCloudRepository, IModelRepository, IControllerRepository, IAbstractRepository<IRenderingModelBase>
    {
        private readonly HttpService _httpService;
        public OrderCloudRepository(HttpService httpService)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            string authorization = OrderCloudAuthService.GetApiTokenAsync(); 
            keyValuePairs.Add("ContentType", "application/json");
            _httpService = new HttpService(keyValuePairs, authorization);
        }
        // Fetch suppliers from OrderCloud API
        public async Task<SupplierListResponse> GetSuppliersAsync()
        {
            string url = OrderCloudAPIS.GetSuppliersEndpoint();  // Set the endpoint for suppliers
            return await _httpService.GetAsync<SupplierListResponse>(url);
        } 
       
    }

}