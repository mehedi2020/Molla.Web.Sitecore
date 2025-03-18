using Molla.Foundation.OrderCloud.Common.Services;
using Molla.Foundation.OrderCloud.Models;
using System.Web.Mvc;

namespace Molla.Foundation.OrderCloud.Common.Controllers
{
    public class OrderCloudAPIController : Controller
    {
        protected IOrderCloudRepository OrderCloudService
        {
            get;
        }

        public OrderCloudAPIController(IOrderCloudRepository repository)
        {
            this.OrderCloudService = repository;
        }
        // GET: OrderCloudAPI
        public ActionResult Categories()
        {
            CategoryResponse categoryResponse = this.OrderCloudService.GetCategoriesAsyncForHome(OrderCloudSetting.CatalogName).Result;
            return View("~/Views/Category/Categories.cshtml", categoryResponse);
        }
    }
}