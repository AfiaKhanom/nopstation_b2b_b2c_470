using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures;

public class B2BB2CFeaturesPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin, INopStationPlugin
{
    #region Fields

    private readonly ILogger _logger;
    private readonly IWebHelper _webHelper;
    private readonly IAddressService _addressService;
    private readonly ILanguageService _languageService;
    private readonly IPermissionService _permissionService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ILocalizationService _localizationService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ICustomerService _customerService;

    public bool HideInWidgetList => false;

    public int Order => throw new NotImplementedException();

    private static string DEFAULT_ERP_SALES_ORG_CODE => "101";

    #endregion

    #region Ctor

    public B2BB2CFeaturesPlugin(ILogger logger,
        IWebHelper webHelper,
        IAddressService addressService,
        ILanguageService languageService,
        IPermissionService permissionService,
        IErpSalesOrgService erpSalesOrgService,
        IScheduleTaskService scheduleTaskService,
        IEmailAccountService emailAccountService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        ICustomerService customerService)
    {
        _logger = logger;
        _webHelper = webHelper;
        _addressService = addressService;
        _languageService = languageService;
        _permissionService = permissionService;
        _erpSalesOrgService = erpSalesOrgService;
        _scheduleTaskService = scheduleTaskService;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _customerService = customerService;
    }

    #endregion

    #region Utilities

    private Language GetDefaultEnglishLanguage()
    {
        return _languageService.GetAllLanguages().FirstOrDefault(x => x.UniqueSeoCode.Equals("en", StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task InstalLocalResourseStringFromXmlFileAsync()
    {
        var language = GetDefaultEnglishLanguage();

        if (language == null)
        {
            _logger.Error("Can't Add Resource string. Couldn't Find The Requered Language!");
            return;
        }

        try
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);
            using var sr = new StreamReader(path, Encoding.UTF8);
            await _localizationService.ImportResourcesFromXmlAsync(language, sr);
        }
        catch (Exception ex)
        {
            _logger.Error("B2B Features Plugin: Can't Add Resource string!", ex);
        }
    }

    public async Task UnInstalLocalResourseStringFromXmlFileAsync()
    {
        try
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);

            var doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    var resource = node.Attributes["Name"]?.InnerText;
                    if (!string.IsNullOrEmpty(resource))
                    {
                        await _localizationService.DeleteLocaleResourceAsync(resource);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("B2B Features Plugin: Can't Remove Resource string!", ex);
        }
    }

    #endregion

    #region Method

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(PublicWidgetZones.HeaderLinksBefore))
            return typeof(PublicHeaderViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
            return typeof(B2BRootHeadViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.OrderSummaryContentDeals))
            return typeof(OrderSummaryContentDealsViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ZoneAfterTirePriceCard))
            return typeof(ErpSpecialPriceViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ZoneAfterSpecialPriceCard))
            return typeof(ErpPriceGroupViewComponent);
        else if (widgetZone.Equals(AdminWidgetZones.OrderDetailsBlock))
            return typeof(ErpOrderItemInOrderDetailsAdminViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ErpAdminWidgetZonesOrderDetailsBlock))
            return typeof(ErpOrderInOrderDetailsAdminViewComponent);
        else if (widgetZone.Equals(AdminWidgetZones.CustomerDetailsBlock))
            return typeof(NopCustomerErpAccountInfoComponent);
        else
            return null;
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var pluginAssembly = Assembly.GetExecutingAssembly();

        if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(pluginAssembly).Result)
        {
            return Task.FromResult<IList<string>>(new List<string> { string.Empty });
        }

        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HeaderLinksBefore,
            PublicWidgetZones.HeadHtmlTag,
            PublicWidgetZones.OrderSummaryContentDeals,
            B2BB2CFeaturesDefaults.ZoneAfterTirePriceCard,
            B2BB2CFeaturesDefaults.ZoneAfterSpecialPriceCard,
            B2BB2CFeaturesDefaults.ErpAdminWidgetZonesOrderDetailsBlock,
            AdminWidgetZones.OrderDetailsBlock,
            AdminWidgetZones.CustomerDetailsBlock
        });
    }

    public override async Task InstallAsync()
    {
        await InstalLocalResourseStringFromXmlFileAsync();

        await _permissionService.InstallPermissionsAsync(new B2BB2CPermissionProvider());
        await _permissionService.InstallPermissionsAsync(new ErpPermissionProvider());

        if (await _scheduleTaskService.GetTaskByTypeAsync(B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTask) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(new()
            {
                Enabled = false,
                StopOnError = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTaskName,
                Type = B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTask,
                Seconds = B2BB2CFeaturesDefaults.DefaultTaskTimeOutPeriod
            });
        }

        #region Message Templates

        var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        if (emailAccount is not null)
        {
            await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
            {
                Name = B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification,
                Subject = "%Store.Name%. Order place at ERP failed",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your order is not placed at ERP. Plese contact with Account Rep. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            });

            await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
            {
                Name = B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin,
                Subject = "%Store.Name%. ERP Customer Registration Application Created",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.AdminName%,{Environment.NewLine}<br />{Environment.NewLine}An application is created to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Please review the application. Application Id: %Application.Id%. And Registration Number: %Application.RegistrationNumber%{Environment.NewLine}<br />{Environment.NewLine}Thanks</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            });

            await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
            {
                Name = B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer,
                Subject = "%Store.Name%. ERP Customer Registration Application Created",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}An application is created to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Admin will review your application. You will get another mail if it gets approved.{Environment.NewLine}<br />{Environment.NewLine}Thank you</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            });

            await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
            {
                Name = B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification,
                Subject = "%Store.Name%. ERP Customer Registration Application Approved",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your application is Approved to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Admin will create an ERP account for your user according to your given information.{Environment.NewLine}<br />{Environment.NewLine}Thank you</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            });
        }

        #endregion

        await this.InstallPluginAsync();

        #region Default Sales Org and addres creation

        var address = new Address
        {
            FirstName = "John",
            LastName = "Wick",
            Email = "John@mail.com",
            County = "England",
            City = "London",
            ZipPostalCode = "1000",
            PhoneNumber = "88017xxxxxxxx",
            CreatedOnUtc = DateTime.UtcNow
        };
        await _addressService.InsertAddressAsync(address);

        var defaultSalesOrg = await _erpSalesOrgService.GetSalesOrgByCodeAsync(DEFAULT_ERP_SALES_ORG_CODE);

        if (defaultSalesOrg is null)
        {
            defaultSalesOrg = new ErpSalesOrg
            {
                Name = "Default Sales Org",
                Code = DEFAULT_ERP_SALES_ORG_CODE,
                Email = "default@mail.com",
                IntegrationClientId = "1",
                AuthenticationKey = "authkey1212",
                IsActive = true,
                AddressId = address.Id,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _erpSalesOrgService.InsertErpSalesOrgAsync(defaultSalesOrg);
        }

        #endregion

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await InstalLocalResourseStringFromXmlFileAsync();
        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/B2BB2CFeatures/Configure";
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var childNode = new SiteMapNode()
        {
            SystemName = "NopStation.B2BB2CFeatures",
            Title = "B2B-B2C Features",
            IconClass = "nav-icon fas fa-cube",
            Visible = true,
            ChildNodes = new List<SiteMapNode>() {
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.Configuration",
                    Title = "Configuration",
                    ControllerName = "B2BB2CFeatures",
                    ActionName = "Configure",
                    IconClass = "nav-icon fas fa-cogs",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpAccounts",
                    Title = "ERP Accounts",
                    ControllerName = "ErpAccount",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-users",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpRegistrationApplication",
                    Title = "Registration Applications",
                    ControllerName = "ErpRegistrationApplication",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-users",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpSalesOrgs",
                    Title = "ERP Sales Orgs",
                    ControllerName = "ErpSalesOrg",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-building",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpShipToAddress",
                    Title = "ERP Account Branch",
                    ControllerName = "ErpShipToAddress",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-circle",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpNopUsers",
                    Title = "ERP Nop Users",
                    ControllerName = "ErpNopUser",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-user",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpGroupPriceCode",
                    Title = "ERP Group Price Code",
                    ControllerName = "ErpGroupPriceCode",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-dollar-sign",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpInvoices",
                    Title = "ERP Invoices",
                    ControllerName = "ErpInvoice",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-file-invoice-dollar",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.SalesRepresentatives",
                    Title = "Sales representatives",
                    ControllerName = "SalesRepresentative",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-user-tie",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpAllProducts",
                    Title = "All ERP Products",
                    ControllerName = "ErpProductPricing",
                    ActionName = "AllProductList",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpOrder",
                    Title = "All ERP Orders",
                    ControllerName = "ErpOrder",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                }
            }
        };
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
        pluginNode?.ChildNodes.Add(childNode);

        childNode = new SiteMapNode()
        {
            SystemName = "NopStation.B2BB2CFeatures.ErpLogs",
            Title = "ERP Logs",
            ControllerName = "ErpLogs",
            ActionName = "List",
            IconClass = "nav-icon fas fa-list",
            Visible = true,
            ChildNodes = new List<SiteMapNode>() { }
        };
        pluginNode?.ChildNodes.Add(childNode);

        childNode = new SiteMapNode()
        {
            SystemName = "NopStation.B2BB2CFeatures.ErpActivityLogs",
            Title = "ERP Activity Logs",
            IconClass = "nav-icon fas fa-cube",
            Visible = true,
            ChildNodes = new List<SiteMapNode>() {
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpActivityLogsList",
                    Title = "ERP Activity Logs List",
                    ControllerName = "ErpActivityLogs",
                    ActionName = "ERPActivityLogs",
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                },
                new ()
                {
                    SystemName = "NopStation.B2BB2CFeatures.ErpActivityLogsTypes",
                    Title = "ERP Activity Logs Types",
                    ControllerName = "ErpActivityLogs",
                    ActionName = "ErpActivityTypes",
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { }
                }
            }
        };
        pluginNode?.ChildNodes.Add(childNode);

    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await UnInstalLocalResourseStringFromXmlFileAsync();

        await _permissionService.UninstallPermissionsAsync(new B2BB2CPermissionProvider());
        await _permissionService.UninstallPermissionsAsync(new ErpPermissionProvider());

        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
        var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);
        using var sr = new StreamReader(path, Encoding.UTF8);
        var result = new HashSet<(string name, string value)>();

        using (var xmlReader = XmlReader.Create(sr))
            while (xmlReader.ReadToFollowing("Language"))
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                using var languageReader = xmlReader.ReadSubtree();
                while (languageReader.ReadToFollowing("LocaleResource"))
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.GetAttribute("Name") is string name)
                    {
                        using var lrReader = languageReader.ReadSubtree();
                        if (lrReader.ReadToFollowing("Value") && lrReader.NodeType == XmlNodeType.Element)
                            result.Add((name.ToLowerInvariant(), lrReader.ReadString()));
                    }

                break;
            }

        return result.Select(item => new KeyValuePair<string, string>(item.name, item.value)).ToList();
    }

    #endregion
}