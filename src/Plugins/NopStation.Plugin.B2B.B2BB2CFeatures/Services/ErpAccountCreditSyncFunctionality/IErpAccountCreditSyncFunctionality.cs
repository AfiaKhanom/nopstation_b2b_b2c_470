using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;

public interface IErpAccountCreditSyncFunctionality
{
    Task LiveErpAccountCreditCheckAsync(ErpAccount erpAccount, Customer customer);
}