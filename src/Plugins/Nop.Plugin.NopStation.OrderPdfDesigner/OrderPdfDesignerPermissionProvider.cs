using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.NopStation.OrderPdfDesigner;

/// <summary>
/// Represents the Order PDF Designer permission provider
/// </summary>
public class OrderPdfDesignerPermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageOrderPdfDesigner = new PermissionRecord 
    { 
        Name = "NopStation Order PDF Designer. Manage configuration", 
        SystemName = "ManageNopStationOrderPdfDesigner", 
        Category = "NopStation" 
    };

    /// <summary>
    /// Get permissions
    /// </summary>
    /// <returns>Permissions</returns>
    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageOrderPdfDesigner
        };
    }

    /// <summary>
    /// Get default permissions
    /// </summary>
    /// <returns>Default permissions</returns>
    public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new HashSet<(string, PermissionRecord[])>
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                new[]
                {
                    ManageOrderPdfDesigner
                }
            )
        };
    }
}
