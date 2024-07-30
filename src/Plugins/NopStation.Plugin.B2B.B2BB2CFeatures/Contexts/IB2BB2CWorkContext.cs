using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Contexts
{

    public interface IB2BB2CWorkContext : IWorkContext
    {
        Task<ERPCustomer> GetCurrentERPCustomerAsync(int erpAccountId = 0);
        Task SetCurrentERPCustomerAsync(Customer customer = null, int erpAccountId = 0);
    }
}