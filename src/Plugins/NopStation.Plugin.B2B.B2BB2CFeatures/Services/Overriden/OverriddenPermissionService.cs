using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverriddenPermissionService : PermissionService
{
    #region Fields

    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Ctor

    public OverriddenPermissionService(ICustomerService customerService,
        ILocalizationService localizationService,
        IRepository<PermissionRecord> permissionRecordRepository,
        IRepository<PermissionRecordCustomerRoleMapping> permissionRecordCustomerRoleMappingRepository,
        IStaticCacheManager staticCacheManager,
        IWorkContext workContext,
        IErpNopUserAccountMapService erpNopUserAccountMapService, 
        IServiceProvider serviceProvider) : base(customerService,
            localizationService,
            permissionRecordRepository,
            permissionRecordCustomerRoleMappingRepository,
            staticCacheManager,
            workContext)
    {
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _serviceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public override async Task<bool> AuthorizeAsync(string permissionRecordSystemName, Customer customer)
    {
        if (string.IsNullOrEmpty(permissionRecordSystemName))
            return false;

        var erpCustomerFunctionalityService = _serviceProvider.GetService<IErpCustomerFunctionalityService>();
        var erpNopCustomer = await erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (erpNopCustomer != null)
        {
            var erpNopUserAccountMap = await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(erpNopCustomer.ErpAccountId, erpNopCustomer.Id);

            if (erpNopUserAccountMap != null)
            {
                var roles = string.IsNullOrWhiteSpace(erpNopUserAccountMap.CustomerRolesIds) ? new string[0] : erpNopUserAccountMap.CustomerRolesIds.Split(',');
                var customerRolesIds = roles.Select(c => int.Parse(c.ToString())).ToArray();

                var nopRoles = await _customerService.GetCustomerRolesAsync(customer);
                var nopRoleIds = nopRoles.Select(x => (int)x.Id).ToArray();

                var combinedRoles = nopRoleIds.Concat(customerRolesIds).ToArray();

                foreach (var role in combinedRoles)
                {
                    if (await AuthorizeAsync(permissionRecordSystemName, role))
                        return true;
                }
            }
        }
        else
        {
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            foreach (var role in customerRoles)
                if (await AuthorizeAsync(permissionRecordSystemName, role.Id))
                    return true;
        }

        return false;
    }

    #endregion
}