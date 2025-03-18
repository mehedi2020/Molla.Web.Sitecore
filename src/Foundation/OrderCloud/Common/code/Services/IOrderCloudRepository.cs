using Molla.Foundation.OrderCloud.Models;
using Molla.Foundation.OrderCloud.Models.Models;
using System.Threading.Tasks;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public interface IOrderCloudRepository
    {
       Task<SupplierListResponse> GetSuppliersAsync();
       Task<CategoryResponse> GetCategoriesAsyncForHome(string catalogName); 
    }
}