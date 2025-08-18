using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Extensions;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class B2BB2CFeaturesController : NopStationAdminController
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICommonHelper _commonHelper;
    private readonly ISettingService _settingService;
    private readonly ICountryService _countryService;
    private readonly IPermissionService _permissionService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpActivityLogsService _erpActivityLogsService;

    #endregion

    #region Ctor

    public B2BB2CFeaturesController(IWorkContext workContext,
        IStoreContext storeContext,
        ICommonHelper commonHelper,
        ISettingService settingService,
        ICountryService countryService,
        IPermissionService permissionService,
        IStaticCacheManager staticCacheManager,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ISpecificationAttributeService specificationAttributeService,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IErpLogsService erpLogsService,
        IErpSalesOrgService erpSalesOrgService,
        IErpActivityLogsService erpActivityLogsService)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _commonHelper = commonHelper;
        _settingService = settingService;
        _countryService = countryService;
        _permissionService = permissionService;
        _staticCacheManager = staticCacheManager;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _specificationAttributeService = specificationAttributeService;
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _erpLogsService = erpLogsService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpActivityLogsService = erpActivityLogsService;
    }

    #endregion

    #region Utilities

    public async Task PrepareAvailableCountriesAsync(ConfigurationModel model)
    {
        //prepare available countries
        var availableCountries = await _countryService.GetAllCountriesAsync(showHidden: true);
        model.DefaultCountryId = model.DefaultCountryId > 0 ? model.DefaultCountryId : availableCountries[0]?.Id ?? 0;

        if (!availableCountries.Any())
        {
            model.AvailableCountries.Add(new SelectListItem { Value = "0", Text = "No Country available", Selected = true });
            return;
        }

        foreach (var country in availableCountries)
        {
            model.AvailableCountries.Add(new SelectListItem { Value = country.Id.ToString(), Text = country.Name });
        }
    }

    public async Task PrepareAvailableStockDisplayFormatsAsync(ConfigurationModel model)
    {
        //prepare available stock display formats
        var availableStockDisplayFormats = await StockDisplayFormat.DoNotShowAnyStockAtAll.ToSelectListAsync(false);
        foreach (var stockDisplayFormat in availableStockDisplayFormats)
        {
            model.AvailableStockDisplayFormats.Add(stockDisplayFormat);
        }

        await _commonHelper.PrepareDefaultItemAsync(model.AvailableStockDisplayFormats, true);
    }

    public async Task PrepareAvailableSalesOrganizationsAsync(ConfigurationModel model)
    {
        var saleOrganizations = await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false);
        foreach (var salesOrg in saleOrganizations)
        {
            var item = new SelectListItem
            {
                Text = salesOrg.Name,
                Value = salesOrg.Id.ToString(),
            };
            model.AvailableSalesOrganizations.Add(item);
        }

        await _commonHelper.PrepareDefaultItemAsync(model.AvailableSalesOrganizations, true);
    }

    public async Task PrepareAvailableSpecificationAttributesAsync(ConfigurationModel model)
    {
        model.AvailableSpecificationAttributes = await _staticCacheManager.GetAsync(ERPIntegrationCoreDefaults.ErpProductSpecificationAttributeList, async () =>
        {
            var specificAttributes = await _specificationAttributeService.GetSpecificationAttributesAsync();
            var selectList = specificAttributes.Select(attribute => new SelectListItem(attribute.Name, attribute.Id.ToString()))
                .ToList();
            //insert this default item at first
            selectList.Insert(0, new SelectListItem { Text = "Not Selected Yet", Value = "0" });

            return selectList;
        });
    }

    public async Task PrepareAvailableProductAvailabilityRanges_DefaultValue(ConfigurationModel model)
    {
        //prepare available product availability ranges
        await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(model.AvailableProductAvailabilityRanges_DefaultValue,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"));

        await _commonHelper.PrepareDefaultItemAsync(model.AvailableProductAvailabilityRanges_DefaultValue, true);
    }
    private async Task PrepareOverrideForStore(ConfigurationModel model, int storeScope, B2BB2CFeaturesSettings settings)
    {
        model.ActiveStoreScopeConfiguration = storeScope;
        if (storeScope > 0)
        {
            model.Override_AllowBackInStockSubscriptions = await _settingService.SettingExistsAsync(settings, x => x.AllowBackInStockSubscriptions_DefaultValue, storeScope);
            model.Override_LowStockActivityId = await _settingService.SettingExistsAsync(settings, x => x.LowStockActivityId_DefaultValue, storeScope);
            model.Override_BackorderModeId = await _settingService.SettingExistsAsync(settings, x => x.BackorderModeId_DefaultValue, storeScope);
            model.Override_TrackInventoryMethodId = await _settingService.SettingExistsAsync(settings, x => x.TrackInventoryMethodId, storeScope);
            model.Override_ProductAvailabilityRangeId = await _settingService.SettingExistsAsync(settings, x => x.ProductAvailabilityRangeId_DefaultValue, storeScope);
            model.Override_AvailableForPreOrder = await _settingService.SettingExistsAsync(settings, x => x.AvailableForPreOrder_DefaultValue, storeScope);
            model.Override_DisplayStockAvailability = await _settingService.SettingExistsAsync(settings, x => x.DisplayStockAvailability_DefaultValue, storeScope);
            model.Override_DisplayStockQuantity = await _settingService.SettingExistsAsync(settings, x => x.DisplayStockQuantity_DefaultValue, storeScope);
            model.Override_AllowOverspend = await _settingService.SettingExistsAsync(settings, x => x.AllowOverspend, storeScope);
            model.EnableLiveCreditChecks_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableLiveCreditChecks, storeScope);
            model.OrderMaximumQuantity_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OrderMaximumQuantity, storeScope);
            model.DefaultCountryId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultCountryId, storeScope);
            model.IsB2CUserRegisterAllowed_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsB2CUserRegisterAllowed, storeScope);
            model.EnableAccountPayment_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableAccountPayment, storeScope);
            model.UsePrefilterFacet_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UsePrefilterFacet, storeScope);
            model.UseDefaultAccountForB2CUser_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseDefaultAccountForB2CUser, storeScope);
            model.DefaultB2CErpAccountId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultB2CErpAccountId, storeScope);
            model.UseERPIntegration_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseERPIntegration, storeScope);
            model.UseMultiSalesOrg_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseMultiSalesOrg, storeScope);
            model.UsePercentageOfAllocatedStock_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UsePercentageOfAllocatedStock, storeScope);
            model.PercentageOfStockAllowed_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PercentageOfStockAllowed, storeScope);
            model.IsErpAccountCustomerRegisterAllowed_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsErpAccountCustomerRegisterAllowed, storeScope);
            model.DownloadInvoicesPath_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DownloadInvoicesPath, storeScope);
            model.FtpUserName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpUserName, storeScope);
            model.FtpPassword_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpPassword, storeScope);
            model.EnableAccountStatementDownload_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableAccountStatementDownload, storeScope);
            model.IsCustomerReferenceRequiredDuringPayment_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsCustomerReferenceRequiredDuringPayment, storeScope);
            model.MaintainUniqueCustomerReference_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MaintainUniqueCustomerReference, storeScope);
            model.PreventSpecialCharactersInCustomerReference_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PreventSpecialCharactersInCustomerReference, storeScope);
            model.SpecialCharactersToPreventInCustomerReference_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SpecialCharactersToPreventInCustomerReference, storeScope);
            model.PaymentPopupMessageDelayTimeInSec_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PaymentPopupMessageDelayTimeInSec, storeScope);
            model.GoogleMapsApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.GoogleMapsApiKey, storeScope);
            model.Latitude_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Latitude, storeScope);
            model.Longitude_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Longitude, storeScope);
            model.IsActive_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsActive, storeScope);
            model.EnableWarehouse_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableWarehouse, storeScope);
            model.PlaceB2BOrder_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PlaceB2BOrder, storeScope);
            model.PlaceB2COrder_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PlaceB2COrder, storeScope);
            model.UseNopProductPrice_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseNopProductPrice, storeScope);
            model.UseProductGroupPrice_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseProductGroupPrice, storeScope);
            model.UseProductSpecialPrice_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseProductSpecialPrice, storeScope);
            model.UseProductCombinedPrice_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseProductCombinedPrice, storeScope);
            model.AllowBackOrderingForAll_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowBackOrderingForAll, storeScope);
            model.IsB2BUserRegisterAllowed_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsB2BUserRegisterAllowed, storeScope);
            model.IsShowLoginForPrice_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsShowLoginForPrice, storeScope);
            model.IsShowYearlySavings_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsShowYearlySavings, storeScope);
            model.IsShowAllTimeSavings_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsShowAllTimeSavings, storeScope);
            model.LastDateTimeOfTCUpdate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.LastDateTimeOfTCUpdate, storeScope);
            model.UpdatedOnUtc_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UpdatedOnUtc, storeScope);
            model.UpdatedById_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UpdatedById, storeScope);
            model.MaxERPIntegrationOrderPlaceReties_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MaxErpIntegrationOrderPlaceRetries, storeScope);
            model.EnableLogOnErpCall_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableLogOnErpCall, storeScope);
            model.EnableLiveStockChecks_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableLiveStockChecks, storeScope);
            model.EnableLivePriceChecks_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableLivePriceChecks, storeScope);
            model.StockDisplayFormat_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.StockDisplayFormat, storeScope);
            model.EnableQuoteFunctionality_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableQuoteFunctionality, storeScope);
            model.PreFilterFacetSpecificationAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PreFilterFacetSpecificationAttributeId, storeScope);
            model.UnitOfMeasureSpecificationAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UnitOfMeasureSpecificationAttributeId, storeScope);
            model.DisplayAddToQuickListFavouriteButton_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DisplayAddToQuickListFavouriteButton, storeScope);
            model.AllowAddressEditOnCheckoutForAll_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowAddressEditOnCheckoutForAll, storeScope);
            model.DeliveryDays_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DeliveryDays, storeScope);
            model.CutoffTime_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CutoffTime, storeScope);
            model.ERPToDetermineDate_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ERPToDetermineDate, storeScope);
            model.OverSpendWarningText_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OverSpendWarningText, storeScope);
            model.DefaultB2COrganizationId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultB2COrganizationId, storeScope);
        }
    }
    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var model = new ConfigurationModel();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
        if (settings != null)
            model = settings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeScope;
        await PrepareOverrideForStore(model, storeScope, settings);
        await PrepareAvailableCountriesAsync(model);
        await PrepareAvailableStockDisplayFormatsAsync(model);
        await PrepareAvailableSalesOrganizationsAsync(model);
        await PrepareAvailableSpecificationAttributesAsync(model);
        await PrepareAvailableProductAvailabilityRanges_DefaultValue(model);

        return View(model);
    }

    

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
        if (ModelState.IsValid)
        {
            model.UpdatedById = (await _workContext.GetCurrentCustomerAsync()).Id;
            model.UpdatedOnUtc = DateTime.UtcNow;
            model.PreFilterFacetSpecificationAttributeId = !model.UsePrefilterFacet ? 0 : model.PreFilterFacetSpecificationAttributeId;

            //load settings for a chosen store scope
            var settings = model.ToSettings(b2BB2CFeaturesSettings);
            await _settingService.SaveSettingAsync(settings, storeScope);
            if (!settings.UseDefaultAccountForB2CUser)
            {
                settings.DefaultB2CErpAccountId = 0;
            }
            if (!settings.IsCustomerReferenceRequiredDuringPayment || !settings.PreventSpecialCharactersInCustomerReference)
            {
                if (!settings.IsCustomerReferenceRequiredDuringPayment)
                {
                    settings.MaintainUniqueCustomerReference = false;
                    settings.PreventSpecialCharactersInCustomerReference = false;
                }
                settings.SpecialCharactersToPreventInCustomerReference = string.Empty;
            }
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsActive, model.IsActive_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableWarehouse, model.EnableWarehouse_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PlaceB2BOrder, model.PlaceB2BOrder_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PlaceB2COrder, model.PlaceB2COrder_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseNopProductPrice, model.UseNopProductPrice_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseProductGroupPrice, model.UseProductGroupPrice_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseProductSpecialPrice, model.UseProductSpecialPrice_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseProductCombinedPrice, model.UseProductCombinedPrice_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowBackOrderingForAll, model.AllowBackOrderingForAll_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsB2BUserRegisterAllowed, model.IsB2BUserRegisterAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsShowLoginForPrice, model.IsShowLoginForPrice_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsShowYearlySavings, model.IsShowYearlySavings_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsShowAllTimeSavings, model.IsShowAllTimeSavings_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LastDateTimeOfTCUpdate, model.LastDateTimeOfTCUpdate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UpdatedOnUtc, model.UpdatedOnUtc_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UpdatedById, model.UpdatedById_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MaxErpIntegrationOrderPlaceRetries, model.MaxERPIntegrationOrderPlaceReties_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableLogOnErpCall, model.EnableLogOnErpCall_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableLiveStockChecks, model.EnableLiveStockChecks_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableLivePriceChecks, model.EnableLivePriceChecks_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.StockDisplayFormatId, model.StockDisplayFormat_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableQuoteFunctionality, model.EnableQuoteFunctionality_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DisplayAddToQuickListFavouriteButton, model.DisplayAddToQuickListFavouriteButton_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowAddressEditOnCheckoutForAll, model.AllowAddressEditOnCheckoutForAll_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DeliveryDays, model.DeliveryDays_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CutoffTime, model.CutoffTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ERPToDetermineDate, model.ERPToDetermineDate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OverSpendWarningText, model.OverSpendWarningText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultB2COrganizationId, model.DefaultB2COrganizationId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PreFilterFacetSpecificationAttributeId, model.PreFilterFacetSpecificationAttributeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UnitOfMeasureSpecificationAttributeId, model.UnitOfMeasureSpecificationAttributeId_OverrideForStore, storeScope, false);

            //new settings
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowOverspend, model.Override_AllowOverspend, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackInventoryMethodId, model.Override_TrackInventoryMethodId, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LowStockActivityId_DefaultValue, model.Override_LowStockActivityId, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.BackorderModeId_DefaultValue, model.Override_BackorderModeId, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowBackInStockSubscriptions_DefaultValue, model.Override_AllowBackInStockSubscriptions, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProductAvailabilityRangeId_DefaultValue, model.Override_ProductAvailabilityRangeId, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AvailableForPreOrder_DefaultValue, model.Override_AvailableForPreOrder, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DisplayStockAvailability_DefaultValue, model.Override_DisplayStockAvailability, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DisplayStockQuantity_DefaultValue, model.Override_DisplayStockQuantity, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableLiveCreditChecks, model.EnableLiveCreditChecks_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OrderMaximumQuantity, model.OrderMaximumQuantity_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultCountryId, model.DefaultCountryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsB2CUserRegisterAllowed, model.IsB2CUserRegisterAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableAccountPayment, model.EnableAccountPayment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UsePrefilterFacet, model.UsePrefilterFacet_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseDefaultAccountForB2CUser, model.UseDefaultAccountForB2CUser_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultB2CErpAccountId, model.DefaultB2CErpAccountId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseERPIntegration, model.UseERPIntegration_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseMultiSalesOrg, model.UseMultiSalesOrg_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UsePercentageOfAllocatedStock, model.UsePercentageOfAllocatedStock_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PercentageOfStockAllowed, model.PercentageOfStockAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsErpAccountCustomerRegisterAllowed, model.IsErpAccountCustomerRegisterAllowed_OverrideForStore, storeScope, false);

            //Ftp settings
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DownloadInvoicesPath, model.DownloadInvoicesPath_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FtpUserName, model.FtpUserName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FtpPassword, model.FtpPassword_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableAccountStatementDownload, model.EnableAccountStatementDownload_OverrideForStore, storeScope, false);

            //Checkout Settings
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsCustomerReferenceRequiredDuringPayment, model.IsCustomerReferenceRequiredDuringPayment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MaintainUniqueCustomerReference, model.MaintainUniqueCustomerReference_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PreventSpecialCharactersInCustomerReference, model.PreventSpecialCharactersInCustomerReference_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SpecialCharactersToPreventInCustomerReference, model.SpecialCharactersToPreventInCustomerReference_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PaymentPopupMessageDelayTimeInSec, model.PaymentPopupMessageDelayTimeInSec_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.GoogleMapsApiKey, model.GoogleMapsApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Latitude, model.Latitude_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Longitude, model.Longitude_OverrideForStore, storeScope, false);
            //now clear settings cache
            await _settingService.ClearCacheAsync();

            // clear price cache
            await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix);

            var successMsg = await _localizationService.GetResourceAsync("B2BB2CFeatures.Configuration.Updated");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(successMsg, ErpSyncLevel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditSettings", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditConfigurations"));
        }
        await PrepareOverrideForStore(model, storeScope, b2BB2CFeaturesSettings);
        await PrepareAvailableCountriesAsync(model);
        await PrepareAvailableStockDisplayFormatsAsync(model);
        await PrepareAvailableSalesOrganizationsAsync(model);
        await PrepareAvailableSpecificationAttributesAsync(model);
        await PrepareAvailableProductAvailabilityRanges_DefaultValue(model);

        return View(model);
    }

    #endregion
}
