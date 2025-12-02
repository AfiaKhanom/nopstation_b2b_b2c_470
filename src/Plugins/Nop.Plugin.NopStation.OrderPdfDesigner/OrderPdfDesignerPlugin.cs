using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace Nop.Plugin.NopStation.OrderPdfDesigner;

/// <summary>
/// Represents the Order PDF Designer plugin
/// </summary>
public class OrderPdfDesignerPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IMiscPlugin
{
    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    public OrderPdfDesignerPlugin(
        IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/OrderPdfDesignerAdmin/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        // Install default settings
        var settings = new OrderPdfDesignerSettings
        {
            ServerSectionHeader = "<h1>Order Invoice</h1>",
            HeaderDetails = "<p><strong>Order #:</strong> %Order.OrderNumber%<br/><strong>Date:</strong> %Order.OrderDate%</p>",
            AddressSection = @"<div style='width: 48%; display: inline-block; vertical-align: top;'>
<h3>Billing Address</h3>
<p>%Order.BillingFirstName% %Order.BillingLastName%<br/>
%Order.BillingAddress1%<br/>
%Order.BillingCity%, %Order.BillingStateProvince% %Order.BillingZipPostalCode%<br/>
%Order.BillingCountry%</p>
</div>
<div style='width: 48%; display: inline-block; vertical-align: top;'>
<h3>Shipping Address</h3>
<p>%Order.ShippingFirstName% %Order.ShippingLastName%<br/>
%Order.ShippingAddress1%<br/>
%Order.ShippingCity%, %Order.ShippingStateProvince% %Order.ShippingZipPostalCode%<br/>
%Order.ShippingCountry%</p>
</div>",
            ProductSection = "<h3>Order Items</h3><p>Products will be listed here.</p>",
            NoteSection = "",
            OrderSummarySection = @"<div style='text-align: right; margin-top: 20px;'>
<p><strong>Subtotal:</strong> %Order.OrderSubTotal%</p>
<p><strong>Shipping:</strong> %Order.OrderShipping%</p>
<p><strong>Tax:</strong> %Order.OrderTax%</p>
<p><strong>Total:</strong> %Order.OrderTotal%</p>
</div>",
            Footer = "<hr/><p style='text-align: center;'>%Store.Name% - %Store.URL%</p>",
            FooterDescription = "<p style='text-align: center; font-size: 11px;'>Thank you for your business!</p>",
            PaperSize = "A4",
            MarginTop = 10,
            MarginBottom = 10,
            MarginLeft = 10,
            MarginRight = 10,
            EnableImageInsertion = false,
            ActiveTemplateVersion = 1
        };

        await _settingService.SaveSettingAsync(settings);

        // Install localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.NopStation.OrderPdfDesigner.Menu.OrderPdfDesigner"] = "Order PDF Designer",
            ["Plugins.NopStation.OrderPdfDesigner.Menu.Configuration"] = "Configuration",

            ["Plugins.NopStation.OrderPdfDesigner.Fields.ServerSectionHeader"] = "Server Section Header",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.ServerSectionHeader.Hint"] = "Enter the HTML for the server section header",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.HeaderDetails"] = "Header Details",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.HeaderDetails.Hint"] = "Enter the HTML for the header details",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.AddressSection"] = "Address Section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.AddressSection.Hint"] = "Enter the HTML for the address section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.ProductSection"] = "Product Section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.ProductSection.Hint"] = "Enter the HTML for the product section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.NoteSection"] = "Note Section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.NoteSection.Hint"] = "Enter the HTML for the note section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.OrderSummarySection"] = "Order Summary Section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.OrderSummarySection.Hint"] = "Enter the HTML for the order summary section",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.Footer"] = "Footer",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.Footer.Hint"] = "Enter the HTML for the footer",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.FooterDescription"] = "Footer Description",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.FooterDescription.Hint"] = "Enter the HTML for the footer description",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.PaperSize"] = "Paper Size",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.PaperSize.Hint"] = "Select the paper size for PDF generation",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginTop"] = "Top Margin (mm)",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginTop.Hint"] = "Enter the top margin in millimeters",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginBottom"] = "Bottom Margin (mm)",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginBottom.Hint"] = "Enter the bottom margin in millimeters",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginLeft"] = "Left Margin (mm)",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginLeft.Hint"] = "Enter the left margin in millimeters",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginRight"] = "Right Margin (mm)",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.MarginRight.Hint"] = "Enter the right margin in millimeters",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.EnableImageInsertion"] = "Enable Image Insertion",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.EnableImageInsertion.Hint"] = "Enable image insertion support",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.OrderId"] = "Order ID",
            ["Plugins.NopStation.OrderPdfDesigner.Fields.OrderId.Hint"] = "Enter an order ID to preview",

            ["Plugins.NopStation.OrderPdfDesigner.OrderNotFound"] = "Order not found",
            ["Plugins.NopStation.OrderPdfDesigner.AvailableTokens"] = "Available Tokens",
            ["Plugins.NopStation.OrderPdfDesigner.PreviewOrder"] = "Preview Order",
            ["Plugins.NopStation.OrderPdfDesigner.GeneratePdf"] = "Generate PDF",

            // Token descriptions
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderNumber"] = "Order Number",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderId"] = "Order ID",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.CustomerFullName"] = "Customer Full Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.CustomerEmail"] = "Customer Email",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderDate"] = "Order Date",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderTotal"] = "Order Total",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderSubTotal"] = "Order Sub-Total",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderShipping"] = "Shipping Cost",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderTax"] = "Tax Amount",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderDiscount"] = "Discount Amount",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.PaymentMethod"] = "Payment Method",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Order.ShippingMethod"] = "Shipping Method",

            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.FirstName"] = "Billing First Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.LastName"] = "Billing Last Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Address1"] = "Billing Address 1",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Address2"] = "Billing Address 2",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.City"] = "Billing City",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.StateProvince"] = "Billing State/Province",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.ZipPostalCode"] = "Billing Zip/Postal Code",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Country"] = "Billing Country",

            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.FirstName"] = "Shipping First Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.LastName"] = "Shipping Last Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Address1"] = "Shipping Address 1",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Address2"] = "Shipping Address 2",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.City"] = "Shipping City",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.StateProvince"] = "Shipping State/Province",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.ZipPostalCode"] = "Shipping Zip/Postal Code",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Country"] = "Shipping Country",

            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Store.Name"] = "Store Name",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Store.URL"] = "Store URL",
            ["Plugins.NopStation.OrderPdfDesigner.Tokens.Store.Email"] = "Store Email"
        });

        await this.InstallPluginAsync(new OrderPdfDesignerPermissionProvider());
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<OrderPdfDesignerSettings>();

        await _localizationService.DeleteLocaleResourcesAsync("Plugins.NopStation.OrderPdfDesigner");

        await this.UninstallPluginAsync(new OrderPdfDesignerPermissionProvider());
        await base.UninstallAsync();
    }

    /// <summary>
    /// Manage site map
    /// </summary>
    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(OrderPdfDesignerPermissionProvider.ManageOrderPdfDesigner))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Menu.OrderPdfDesigner"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Menu.Configuration"),
                Url = "~/Admin/OrderPdfDesignerAdmin/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "OrderPdfDesigner.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }

    /// <summary>
    /// Gets plugin resources
    /// </summary>
    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Plugins.NopStation.OrderPdfDesigner.Menu.OrderPdfDesigner", "Order PDF Designer"),
            new KeyValuePair<string, string>("Plugins.NopStation.OrderPdfDesigner.Menu.Configuration", "Configuration")
        };
    }
}
