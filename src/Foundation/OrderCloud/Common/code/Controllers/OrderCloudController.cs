using Molla.Foundation.OrderCloud.Common.Services;
using Molla.Foundation.OrderCloud.Models.Models;
using Sitecore.XA.Foundation.RenderingVariants.Controllers.Base;
using System.Collections.Generic;

namespace Molla.Foundation.OrderCloud.Common.Controllers
{

    public class OrderCloudController : PaginableController
    {
       
        protected IOrderCloudRepository OrderCloudService
        {
            get;
        }

        public OrderCloudController(IOrderCloudRepository repository)
        {
            this.OrderCloudService = repository;
        }

        protected override object GetModel()
        {
              SupplierListResponse supplierListResponse =  this.OrderCloudService.GetSuppliersAsync().Result;
             return supplierListResponse;
        }
    } 
}