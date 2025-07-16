using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Caching;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ErpNopUserCacheEventConsumer : CacheEventConsumer<ErpNopUser>
{
    private readonly ICustomerService _customerService;

    public ErpNopUserCacheEventConsumer(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    protected override async Task ClearCacheAsync(ErpNopUser entity, EntityEventType entityEventType)
    {
        var customer = await _customerService.GetCustomerByIdAsync(entity.NopCustomerId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByCustomerCacheKey, customer.Id,
            string.Join(",", await _customerService.GetCustomerRoleIdsAsync(customer)));
        await RemoveAsync(NopEntityCacheDefaults<ErpNopUser>.ByIdCacheKey, entity.Id);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerAndErpAccountCacheKey, entity.NopCustomerId, 0);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerAndErpAccountCacheKey, entity.NopCustomerId, entity.ErpAccountId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey, entity.NopCustomerId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerAndErpAccountCacheKey, entity.NopCustomerId, entity.ErpAccountId);
    }
}
