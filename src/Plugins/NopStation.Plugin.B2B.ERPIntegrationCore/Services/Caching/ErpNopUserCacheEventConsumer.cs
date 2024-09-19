using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class ErpNopUserCacheEventConsumer : CacheEventConsumer<ErpNopUser>
{
    protected override async Task ClearCacheAsync(ErpNopUser entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey, entity.NopCustomerId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerAndErpAccountCacheKey, entity.NopCustomerId, entity.ErpAccountId);
    }
}
