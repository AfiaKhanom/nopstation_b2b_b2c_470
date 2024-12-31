using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class CustomerCacheEventConsumer : CacheEventConsumer<Customer>
{
    protected override async Task ClearCacheAsync(Customer entity, EntityEventType entityEventType)
    {
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByCustomerPrefixCacheKey, entity.Id);
    }
}
