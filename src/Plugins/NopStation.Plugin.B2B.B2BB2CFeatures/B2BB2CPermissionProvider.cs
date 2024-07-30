using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.B2B.B2BB2CFeatures
{
    public class B2BB2CPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageSalesRepresantatives = new PermissionRecord { Name = "B2B/B2c. Manage Sales Represantatives", SystemName = "B2BB2CManageSalesRepresantatives", Category = "B2BB2C" };
        public static readonly PermissionRecord EnableQuickOrder = new PermissionRecord { Name = "B2B/B2c. Enable Quick Order", SystemName = "EnableQuickOrder", Category = "B2BB2C" };
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageSalesRepresantatives
                    }
                ),
                (
                    ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName,
                    new[]
                    {
                        ManageSalesRepresantatives
                    }
                ),
                (
                    ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName,
                    new[]
                    {
                         EnableQuickOrder
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageSalesRepresantatives,
                EnableQuickOrder
            };
        }
    }
}