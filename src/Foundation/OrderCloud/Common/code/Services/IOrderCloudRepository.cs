using Molla.Foundation.OrderCloud.Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Molla.Foundation.OrderCloud.Common.Services
{
    public interface IOrderCloudRepository
    {
       Task<SupplierListResponse> GetSuppliersAsync();
    }
}