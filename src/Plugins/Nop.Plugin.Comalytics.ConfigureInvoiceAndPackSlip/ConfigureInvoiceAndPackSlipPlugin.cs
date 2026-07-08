using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Template;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip
{
    /// <summary>
    /// Configure Invoice and Pack Slip plugin
    /// </summary>
    public class ConfigureInvoiceAndPackSlipPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ITemplateService _templateService;
        private readonly IWebHelper _webHelper;

        public ConfigureInvoiceAndPackSlipPlugin(
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            ITemplateService templateService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _templateService = templateService;
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ConfigureInvoiceAndPackSlipAdmin/Configure";
        }

        public override async Task InstallAsync()
        {
            // Install settings
            await _settingService.SaveSettingAsync(new ConfigureInvoiceAndPackSlipSettings());

            // Install permissions
            await _permissionService.InstallPermissionsAsync(new ConfigureInvoiceAndPackSlipPermissionProvider());

            // Install localization resources
            await InstallLocalizationResourcesAsync();

            // Install default templates
            await InstallDefaultTemplatesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            // Delete settings
            await _settingService.DeleteSettingAsync<ConfigureInvoiceAndPackSlipSettings>();

            // Uninstall permissions
            await _permissionService.UninstallPermissionsAsync(new ConfigureInvoiceAndPackSlipPermissionProvider());

            // Uninstall localization resources
            await UninstallLocalizationResourcesAsync();

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "ConfigureInvoiceAndPackSlip",
                Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.Title"),
                IconClass = "far fa-dot-circle",
                Visible = true
            };

            if (await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
            {
                menuItem.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "ConfigureInvoiceAndPackSlip.InvoiceTemplates",
                    Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.InvoiceTemplates"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/InvoiceTemplate/List",
                    IconClass = "far fa-circle",
                    Visible = true
                });
            }

            if (await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManagePackingSlipTemplates))
            {
                menuItem.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "ConfigureInvoiceAndPackSlip.PackingSlipTemplates",
                    Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.PackingSlipTemplates"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PackingSlipTemplate/List",
                    IconClass = "far fa-circle",
                    Visible = true
                });
            }

            if (await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManagePdfJobs))
            {
                menuItem.ChildNodes.Add(new SiteMapNode()
                {
                    SystemName = "ConfigureInvoiceAndPackSlip.PdfJobs",
                    Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.PdfJobs"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PdfJob/List",
                    IconClass = "far fa-circle",
                    Visible = true
                });
            }

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "ConfigureInvoiceAndPackSlip.Configure",
                Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.Configure"),
                Url = GetConfigurationPageUrl(),
                IconClass = "far fa-circle",
                Visible = true
            });

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }

        private async Task InstallLocalizationResourcesAsync()
        {
            var resources = new Dictionary<string, string>
            {
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.Title"] = "Invoice & Packing Slips",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.InvoiceTemplates"] = "Invoice Templates",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.PackingSlipTemplates"] = "Packing Slip Templates",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.PdfJobs"] = "PDF Jobs",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Menu.Configure"] = "Configure",
                
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.Title"] = "Configure Invoice and Packing Slip",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PdfRenderer"] = "PDF Renderer",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PdfRenderer.Hint"] = "Select the PDF renderer (QuestPdf or Chromium)",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PageOrientation"] = "Page Orientation",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PageSize"] = "Page Size",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.Margins"] = "Margins (mm)",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.FontSettings"] = "Font Settings",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.SaveSuccess"] = "Configuration saved successfully",
                
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.List"] = "Invoice Templates",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.AddNew"] = "Add New Template",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Edit"] = "Edit Template",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.BackToList"] = "back to template list",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Name"] = "Name",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Store"] = "Store",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Language"] = "Language",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.IsDefault"] = "Is Default",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.RenderMode"] = "Render Mode",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.TemplateHtml"] = "Template HTML",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Created"] = "Template created successfully",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Updated"] = "Template updated successfully",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Deleted"] = "Template deleted successfully",
                
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PackingSlipTemplates.List"] = "Packing Slip Templates",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PackingSlipTemplates.AddNew"] = "Add New Template",
                
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PdfJobs.List"] = "PDF Jobs",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PdfJobs.Status"] = "Status",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PdfJobs.Created"] = "Created",
                ["Plugins.Comalytics.ConfigureInvoiceAndPackSlip.PdfJobs.Completed"] = "Completed"
            };

            foreach (var resource in resources)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value);
            }
        }

        private async Task UninstallLocalizationResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip");
        }

        private async Task InstallDefaultTemplatesAsync()
        {
            // Install a basic default invoice template
            var defaultInvoiceTemplate = new InvoiceTemplate
            {
                Name = "Default Invoice Template",
                StoreId = 0,
                LanguageId = 0,
                IsDefault = true,
                RenderMode = RenderMode.QuestPdf,
                TemplateHtml = GetDefaultInvoiceTemplateHtml(),
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _templateService.InsertInvoiceTemplateAsync(defaultInvoiceTemplate);

            // Install a basic default packing slip template
            var defaultPackingSlipTemplate = new PackingSlipTemplate
            {
                Name = "Default Packing Slip Template",
                StoreId = 0,
                LanguageId = 0,
                IsDefault = true,
                RenderMode = RenderMode.QuestPdf,
                TemplateHtml = GetDefaultPackingSlipTemplateHtml(),
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _templateService.InsertPackingSlipTemplateAsync(defaultPackingSlipTemplate);
        }

        private string GetDefaultInvoiceTemplateHtml()
        {
            return @"
<!-- Default Invoice Template -->
<!-- This template uses tokens that will be replaced with actual data -->
<div style='font-family: Arial, sans-serif;'>
    <h1>Invoice</h1>
    <p><strong>Order Number:</strong> {Order.Number}</p>
    <p><strong>Order Date:</strong> {Order.Date}</p>
    <p><strong>Customer:</strong> {Customer.Name}</p>
    <p><strong>Email:</strong> {Customer.Email}</p>
    
    <h3>Billing Address:</h3>
    <p>{Billing.Address}</p>
    
    <h3>Shipping Address:</h3>
    <p>{Shipping.Address}</p>
    
    <h3>Items:</h3>
    <table border='1' cellpadding='5' cellspacing='0' style='width: 100%;'>
        <tr>
            <th>SKU</th>
            <th>Product</th>
            <th>Qty</th>
            <th>Unit Price</th>
            <th>Total</th>
        </tr>
        {Order.Items}
    </table>
    
    <div style='margin-top: 20px; text-align: right;'>
        <p><strong>Subtotal:</strong> {Order.SubTotal}</p>
        <p><strong>Shipping:</strong> {Order.ShippingTotal}</p>
        <p><strong>Tax:</strong> {Order.Tax}</p>
        <p><strong>Total:</strong> {Order.Total}</p>
    </div>
    
    <div style='margin-top: 30px;'>
        <p><strong>Payment Method:</strong> {Payment.Method}</p>
    </div>
</div>
";
        }

        private string GetDefaultPackingSlipTemplateHtml()
        {
            return @"
<!-- Default Packing Slip Template -->
<div style='font-family: Arial, sans-serif;'>
    <h1>Packing Slip</h1>
    <p><strong>Order Number:</strong> {Order.Number}</p>
    <p><strong>Order Date:</strong> {Order.Date}</p>
    <p><strong>Customer:</strong> {Customer.Name}</p>
    
    <h3>Shipping Address:</h3>
    <p>{Shipping.Address}</p>
    
    <p><strong>Tracking Number:</strong> {Shipment.TrackingNumber}</p>
    
    <h3>Items:</h3>
    <table border='1' cellpadding='5' cellspacing='0' style='width: 100%;'>
        <tr>
            <th>SKU</th>
            <th>Product</th>
            <th>Qty</th>
        </tr>
        {Shipment.Items}
    </table>
</div>
";
        }
    }
}
