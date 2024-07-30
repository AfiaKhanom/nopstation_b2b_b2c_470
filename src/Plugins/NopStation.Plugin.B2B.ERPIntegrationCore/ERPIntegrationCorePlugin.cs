using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.ERPIntegrationCore
{
    public class ERPIntegrationCorePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        public ERPIntegrationCorePlugin(ICustomerService customerService, 
            IWebHelper webHelper, 
            ILocalizationService localizationService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _customerService = customerService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ERPIntegrationCore/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync();

            var b2BCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRole);
            if (b2BCustomerRole == null)
            {
                //new role (save it)
                b2BCustomerRole = new CustomerRole
                {
                    Name = ERPIntegrationCoreDefaults.B2BCustomerRole,
                    Active = true,
                    SystemName = ERPIntegrationCoreDefaults.B2BCustomerRole
                };
                await _customerService.InsertCustomerRoleAsync(b2BCustomerRole);
            }
            var b2CCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRole);
            if (b2CCustomerRole == null)
            {
                //new role (save it)
                b2CCustomerRole = new CustomerRole
                {
                    Name = ERPIntegrationCoreDefaults.B2CCustomerRole,
                    Active = true,
                    SystemName = ERPIntegrationCoreDefaults.B2CCustomerRole
                };
                await _customerService.InsertCustomerRoleAsync(b2CCustomerRole);
            }
            var b2BSalesRepRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
            if (b2BSalesRepRole == null)
            {
                //new role (save it)
                b2BSalesRepRole = new CustomerRole
                {
                    Name = ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName,
                    Active = true,
                    SystemName = ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName
                };
                await _customerService.InsertCustomerRoleAsync(b2BSalesRepRole);
            }
            var quickOrderUserRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName);
            if (quickOrderUserRole == null)
            {
                //new role (save it)
                quickOrderUserRole = new CustomerRole
                {
                    Name = ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName,
                    Active = true,
                    SystemName = ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName
                };
                await _customerService.InsertCustomerRoleAsync(quickOrderUserRole);
            }

            var b2BB2CAdminRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName);
            if (b2BB2CAdminRole == null)
            {
                //new role (save it)
                b2BB2CAdminRole = new CustomerRole
                {
                    Name = ERPIntegrationCoreDefaults.B2BB2CAdminRole,
                    Active = true,
                    SystemName = ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName
                };
                await _customerService.InsertCustomerRoleAsync(b2BB2CAdminRole);
            }

            await _erpActivityLogsService.InsertOrUpdateErpActivityTypesAsync(ErpActivityLogTypes());

            await base.InstallAsync();
             
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync();

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var childNode = new SiteMapNode()
            {
                SystemName = "NopStation.ERPIntegrationCore.Configuration",
                Title = "Core Configuration",
                IconClass = "nav-icon fas fa-cogs",
                Visible = true,
                ActionName = "Configure",
                ControllerName = "ERPIntegrationCore" 
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
            {
                pluginNode.Title = "B2B/B2C Plugins";
                pluginNode.ChildNodes.Add(childNode);
            }
        }

        public async override Task UpdateAsync(string currentVersion, string targetVersion)
        {
            //adding local strings
            var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

            await _erpActivityLogsService.InsertOrUpdateErpActivityTypesAsync(ErpActivityLogTypes());
            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select", "Select"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin", "ERP Integration Plugin"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin.Hint", "Select an ERP Integration Plugin from these System Names."),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Title", "ERP Integration Core Settings"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.BlockTitle.Settings", "Settings"),
                new KeyValuePair<string, string>("Plugins.Misc.NopStation.ERPIntegrationCore.Configuration.Updated", "ERPIntegrationCore settings has been updated successfully."),
            };

            return list;
        }

        public List<ActivityLogType> ErpActivityLogTypes()
        {
            return new List<ActivityLogType>
            {
                new()
                {
                    SystemKeyword = "Erp_EditSettings",
                    Enabled = true,
                    Name = "Edit setting(s)"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpActivityLog",
                    Enabled = true,
                    Name = "Delete an Erp Activity Log"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteAllErpActivityLogs",
                    Enabled = true,
                    Name = "Delete all Erp Activity Log"
                },
                new()
                {
                    SystemKeyword = "Erp_UpdateErpActivityLogsTypes",
                    Enabled = true,
                    Name = "Update Erp Activity Log Types"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewB2CCustomer",
                    Enabled = true,
                    Name = "Add a new B2C Customer"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewB2BCustomer",
                    Enabled = true,
                    Name = "Add a new B2B Customer"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpNopUser",
                    Enabled = true,
                    Name = "Add a new Erp Nop User"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpNopUser",
                    Enabled = true,
                    Name = "Edit an Erp Nop User"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpNopUser",
                    Enabled = true,
                    Name = "Delete an Erp Nop User"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpAccount",
                    Enabled = true,
                    Name = "Add a new Erp Account"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpAccount",
                    Enabled = true,
                    Name = "Edit an Erp Account"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpAccount",
                    Enabled = true,
                    Name = "Delete an Erp Account"
                },
                new()
                {
                    SystemKeyword = "Erp_ErpOrderPlacement",
                    Enabled = true,
                    Name = "Place Order on ERP"
                },
                new()
                {
                    SystemKeyword = "Erp_B2COrderPlacement",
                    Enabled = true,
                    Name = "Place a B2C Order"
                },
                new()
                {
                    SystemKeyword = "Erp_B2BOrderPlacement",
                    Enabled = true,
                    Name = "Place a B2B Order"
                },
                new()
                {
                    SystemKeyword = "Erp_B2BQuoteOrderPlacement",
                    Enabled = true,
                    Name = "Place a B2B Quote Order"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpShipToAddress",
                    Enabled = true,
                    Name = "Add a new Erp Ship To Address"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpShipToAddress",
                    Enabled = true,
                    Name = "Edit an Erp Ship To Address"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpShipToAddress",
                    Enabled = true,
                    Name = "Delete an Erp Ship To Address"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpGroupPriceCode",
                    Enabled = true,
                    Name = "Add a new Erp Group Price Code"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpGroupPriceCode",
                    Enabled = true,
                    Name = "Edit an Erp Group Price Code"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpGroupPriceCode",
                    Enabled = true,
                    Name = "Delete an Erp Group Price Code"
                },
                new()
                {
                    SystemKeyword = "Erp_CustomerImpersonationStart",
                    Enabled = true,
                    Name = "Erp Customer Impersonation Start"
                },
                new()
                {
                    SystemKeyword = "Erp_CustomerImpersonationEnd",
                    Enabled = true,
                    Name = "Erp Customer Impersonation End"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpNopUserAccountMap",
                    Enabled = true,
                    Name = "Add a new Erp Nop User Account Map"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpNopUserAccountMap",
                    Enabled = true,
                    Name = "Delete an Erp Nop User Account Map"
                },
                new()
                {
                    SystemKeyword = "Erp_ReprocessErpOrder",
                    Enabled = true,
                    Name = "Reprocess an Erp Order"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewSpecialPrice",
                    Enabled = true,
                    Name = "Add a new Erp Special Price"
                },
                new()
                {
                    SystemKeyword = "Erp_EditSpecialPrice",
                    Enabled = true,
                    Name = "Edit an Erp Special Price"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteSpecialPrice",
                    Enabled = true,
                    Name = "Delete an Erp Special Price"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpGroupPrice",
                    Enabled = true,
                    Name = "Add a new Erp Group Price"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpGroupPrice",
                    Enabled = true,
                    Name = "Edit an Erp Group Price"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpGroupPrice",
                    Enabled = true,
                    Name = "Delete an Erp Group Price"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpSalesOrg",
                    Enabled = true,
                    Name = "Add a new Erp Sales Org"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpSalesOrg",
                    Enabled = true,
                    Name = "Edit an Erp Sales Org"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpSalesOrg",
                    Enabled = true,
                    Name = "Delete an Erp Sales Org"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpSalesOrgWarehouse",
                    Enabled = true,
                    Name = "Add a new Erp Sales Org Warehouse"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpSalesOrgWarehouse",
                    Enabled = true,
                    Name = "Edit an Erp Sales Org Warehouse"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpSalesOrgWarehouse",
                    Enabled = true,
                    Name = "Delete an Erp Sales Org Warehouse"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpSalesRep",
                    Enabled = true,
                    Name = "Add a new Erp Sales Representative"
                },
                new()
                {
                    SystemKeyword = "Erp_EditErpSalesRep",
                    Enabled = true,
                    Name = "Edit an Erp Sales Representative"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpSalesRep",
                    Enabled = true,
                    Name = "Delete an Erp Sales Representative"
                },
                new()
                {
                    SystemKeyword = "Erp_ReOrder",
                    Enabled = true,
                    Name = "ReOrder an Order"
                },
                new()
                {
                    SystemKeyword = "Erp_ErpCustomerPublicStoreLogin",
                    Enabled = true,
                    Name = "Erp Customer login into Public Store"
                },
                new()
                {
                    SystemKeyword = "Erp_ErpCustomerPublicStoreLogOut",
                    Enabled = true,
                    Name = "Erp Customer log out from public store"
                },
                new()
                {
                    SystemKeyword = "Erp_QuoteToOrderConvert",
                    Enabled = true,
                    Name = "Convert Quote to Order"
                },
                new()
                {
                    SystemKeyword = "Erp_QuickOrderToOrderConvert",
                    Enabled = true,
                    Name = "Convert Quick order to Order"
                },
                new()
                {
                    SystemKeyword = "Erp_CreateQuickOrder",
                    Enabled = true,
                    Name = "Create a Quick Order"
                },
                new()
                {
                    SystemKeyword = "Erp_UpdateQuickOrder",
                    Enabled = true,
                    Name = "Update a Quick Order"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteQuickOrder",
                    Enabled = true,
                    Name = "Delete a Quick Order"
                },
                new()
                {
                    SystemKeyword = "Erp_InvoiceDownload",
                    Enabled = true,
                    Name = "Download Invoice"
                },
                new()
                {
                    SystemKeyword = "Erp_PODDownload",
                    Enabled = true,
                    Name = "Download POD"
                },
                new()
                {
                    SystemKeyword = "Erp_ErpNopUserAccountSwitch",
                    Enabled = true,
                    Name = "Switch Erp Account for Erp Nop User"
                },
                new()
                {
                    SystemKeyword = "Erp_AddNewErpAccountForErpSalesRepMap",
                    Enabled = true,
                    Name = "Add new Erp Account for Erp Sales Rep"
                },
                new()
                {
                    SystemKeyword = "Erp_DeleteErpAccountForErpSalesRepMap",
                    Enabled = true,
                    Name = "Delete Erp Account for Erp Sales Rep"
                },
                new()
                {
                    SystemKeyword = "Erp_EditSyncTask",
                    Enabled = true,
                    Name = "Edit sync task"
                },
            };
        }
    }
}