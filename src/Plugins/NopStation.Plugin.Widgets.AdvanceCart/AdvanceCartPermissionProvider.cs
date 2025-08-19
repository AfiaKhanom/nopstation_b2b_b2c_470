using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AdvanceCart;

public class AdvanceCartPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageAdvanceCart = new PermissionRecord { Name = "NopStation Advance Cart. Manage Advance Cart", SystemName = "ManageNopStationAdvanceCart", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageAdvanceCart
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
                    ManageAdvanceCart
                }
            )
        };
    }
}
