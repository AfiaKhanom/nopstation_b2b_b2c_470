using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Theme.Stafast
{
    public class StafastPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageStafast = new PermissionRecord { Name = "Stafast theme. Manage NopStation Stafast theme", SystemName = "ManageNopStationStafast", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageStafast
            };
        }

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageStafast
                    }
                )
            };
        }
    }
}
