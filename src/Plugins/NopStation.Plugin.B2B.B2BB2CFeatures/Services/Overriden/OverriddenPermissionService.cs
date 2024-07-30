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

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden
{
    public class OverriddenPermissionService : PermissionService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly IRepository<PermissionRecordCustomerRoleMapping> _permissionRecordCustomerRoleMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContext _workContext;
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
        IErpNopUserAccountMapService erpNopUserAccountMapService, IServiceProvider serviceProvider) :
            base(customerService,
            localizationService,
            permissionRecordRepository,
            permissionRecordCustomerRoleMappingRepository,
            staticCacheManager,
            workContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _permissionRecordRepository = permissionRecordRepository;
            _permissionRecordCustomerRoleMappingRepository = permissionRecordCustomerRoleMappingRepository;
            _staticCacheManager = staticCacheManager;
            _workContext = workContext;
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

            // If erpNop Customer 
            if (erpNopCustomer != null)
            {
                var erpNopUserAccountMap = await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(erpNopCustomer.ErpAccountId, erpNopCustomer.Id);

                if (erpNopUserAccountMap != null)
                {
                    // Taking erp roles ids 
                    var roles = erpNopUserAccountMap.CustomerRolesIds.Split(',');
                    int[] customerRolesIds = roles.Select(c => int.Parse(c.ToString())).ToArray();

                    // taking nop roles ids 
                    var nopRoles = await _customerService.GetCustomerRolesAsync(customer);
                    int[] nopRoleIds = nopRoles.Select(x => (int)x.Id).ToArray();

                    // concate roles and check
                    int[] combinedRoles = nopRoleIds.Concat(customerRolesIds).ToArray();

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
}