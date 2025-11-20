using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ErpAccountCacheEventConsumer : CacheEventConsumer<ErpAccount>
{
    protected override async Task ClearCacheAsync(ErpAccount entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByIdCacheKey, entity.Id, true);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByIdWithActiveCacheKey, entity.Id);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByErpShipToAddressCacheKey, entity.BillingAddressId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByAccountNumberCacheKey, entity.AccountNumber);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountBySalesOrgIdCacheKey, entity.ErpSalesOrgId);

        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByIdPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByIdWithActivePrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByErpShipToAddressPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByAccountNumberPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByCustomerIdPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountBySalesOrgIdPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountAllActivePrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountPagedPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountListPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountPagedByIdsPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountOfActiveNopUsersPrefixCacheKey);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId, entity.Id);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix, entity.Id);
    }
}