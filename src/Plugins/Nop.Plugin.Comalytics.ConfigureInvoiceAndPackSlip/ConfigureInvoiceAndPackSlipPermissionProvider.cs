using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip
{
    /// <summary>
    /// Permission provider for the plugin
    /// </summary>
    public class ConfigureInvoiceAndPackSlipPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageInvoiceTemplates = new() 
        { 
            Name = "Admin area. Manage Invoice Templates", 
            SystemName = "ManageInvoiceTemplates", 
            Category = "Configuration" 
        };

        public static readonly PermissionRecord ManagePackingSlipTemplates = new() 
        { 
            Name = "Admin area. Manage Packing Slip Templates", 
            SystemName = "ManagePackingSlipTemplates", 
            Category = "Configuration" 
        };

        public static readonly PermissionRecord ManagePdfJobs = new() 
        { 
            Name = "Admin area. Manage PDF Jobs", 
            SystemName = "ManagePdfJobs", 
            Category = "Configuration" 
        };

        public static readonly PermissionRecord GeneratePdfs = new() 
        { 
            Name = "Admin area. Generate PDFs", 
            SystemName = "GeneratePdfs", 
            Category = "Orders" 
        };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageInvoiceTemplates,
                ManagePackingSlipTemplates,
                ManagePdfJobs,
                GeneratePdfs
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
                        ManageInvoiceTemplates,
                        ManagePackingSlipTemplates,
                        ManagePdfJobs,
                        GeneratePdfs
                    }
                )
            };
        }
    }
}
