using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AjaxCart;

public class AjaxCartPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageAjaxCart = new PermissionRecord { Name = "NopStation Ajax Cart. Manage Ajax Cart", SystemName = "ManageNopStationAjaxCart", Category = "NopStation" };

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageAjaxCart
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
                    ManageAjaxCart
                }
            )
        };
    }
}
