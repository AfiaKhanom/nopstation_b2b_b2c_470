using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Theme.Stafast.Areas.Admin.Models;
using NopStation.Plugin.Theme.Stafast.Infrastructure.Cache;

namespace NopStation.Plugin.Theme.Stafast.Areas.Admin.Controllers
{
    public class StafastController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;
        private readonly IStoreService _storeService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public StafastController(IPermissionService permissionService,
         ISettingService settingService,
         IStoreContext storeContext,
         ILocalizationService localizationService,
         INotificationService notificationService,
         ILogger logger,
         IStoreService storeService,
         INopFileProvider nopFileProvider,
         IStaticCacheManager staticCacheManager,
         ILanguageService languageService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _logger = logger;
            _storeService = storeService;
            _nopFileProvider = nopFileProvider;
            _staticCacheManager = staticCacheManager;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        protected async Task UpdateCssFilesAsync(int storeId)
        {
            if (storeId == 0)
                foreach (var store in await _storeService.GetAllStoresAsync())
                    await UpdateCssFileAsync(store.Id);
            else
                await UpdateCssFileAsync(storeId);
        }

        protected async Task UpdateCssFileAsync(int storeId)
        {
            var cssText = await GetThemeColorCssTextAsync(storeId);

            var path = $"~/Themes/Stafast/Content/css/styles.default-{storeId}.css";
            var fileSystemPath = _nopFileProvider.MapPath(path);

            try
            {
                if (!_nopFileProvider.FileExists(fileSystemPath))
                    _nopFileProvider.CreateFile(fileSystemPath);

                await _nopFileProvider.WriteAllTextAsync(fileSystemPath, cssText, Encoding.UTF8);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"Could not save or create {fileSystemPath} ", e);
            }
        }

        protected async Task<string> GetThemeColorCssTextAsync(int storeId)
        {
            var StafastSettings = await _settingService.LoadSettingAsync<StafastSettings>(storeId);

            var cssText = StafastSettings.CustomCss + Environment.NewLine +
                ":root { " + Environment.NewLine +
                "   --primary-color: " + StafastSettings.PrimaryThemeColor + ";" + Environment.NewLine +
                "   --secondary-color: " + StafastSettings.SecondaryThemeColor + ";" + Environment.NewLine +
                "}" +
                Environment.NewLine;

            return cssText;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StafastPermissionProvider.ManageStafast))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var StafastSettings = await _settingService.LoadSettingAsync<StafastSettings>(storeId);
            var model = StafastSettings.ToSettingsModel<ConfigurationModel>();

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.FooterContactUsText = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterContactUsText, languageId, 0, false, false);

                locale.FooterDescriptionBoxOneTitle = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxOneTitle, languageId, 0, false, false);
                locale.FooterDescriptionBoxOneText = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxOneText, languageId, 0, false, false);
                locale.FooterDescriptionBoxOneUrl = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxOneUrl, languageId, 0, false, false);

                locale.FooterDescriptionBoxTwoTitle = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxTwoTitle, languageId, 0, false, false);
                locale.FooterDescriptionBoxTwoText = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxTwoText, languageId, 0, false, false);
                locale.FooterDescriptionBoxTwoUrl = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxTwoUrl, languageId, 0, false, false);

                locale.FooterDescriptionBoxThreeTitle = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxThreeTitle, languageId, 0, false, false);
                locale.FooterDescriptionBoxThreeText = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxThreeText, languageId, 0, false, false);
                locale.FooterDescriptionBoxThreeUrl = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxThreeUrl, languageId, 0, false, false);

                locale.FooterDescriptionBoxFourTitle = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxFourTitle, languageId, 0, false, false);
                locale.FooterDescriptionBoxFourText = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxFourText, languageId, 0, false, false);
                locale.FooterDescriptionBoxFourUrl = await _localizationService
                    .GetLocalizedSettingAsync(StafastSettings, x => x.FooterDescriptionBoxFourUrl, languageId, 0, false, false);
            });

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId > 0)
            {
                #region Color
                model.PrimaryThemeColor_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.PrimaryThemeColor, storeId);
                model.SecondaryThemeColor_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.SecondaryThemeColor, storeId);

                #endregion

                #region General 

                model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.PhoneNumber, storeId);
                model.LinkedInLink_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.LinkedInLink, storeId);
                model.PinterestLink_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.PinterestLink, storeId);

                model.FooterContactUsText_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterContactUsText, storeId);
                model.HideDesignedByNopStationAtFooter_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.HideDesignedByNopStationAtFooter, storeId);
                model.ShowLogoAtFooter_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.ShowLogoAtFooter, storeId);
                model.FooterLogoPictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterLogoPictureId, storeId);
                model.ShowSupportedCardsAtFooter_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.ShowSupportedCardsAtFooter, storeId);
                model.FooterSupportedCardsPictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterSupportedCardsPictureId, storeId);

                model.EnableLoginBoxAtHeader_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableLoginBoxAtHeader, storeId);
                model.EnableImageLazyLoad_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableImageLazyLoad, storeId);
                model.LazyLoadPictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.LazyLoadPictureId, storeId);
                model.NewsLetterSideImageId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.NewsLetterSideImageId, storeId);
                model.EnableStickyHeader_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableStickyHeader, storeId);
                model.HideProductBoxButtonsOnMobile_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.HideProductBoxButtonsOnMobile, storeId);
                model.NuberOfItemsToShowInProductFilters_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.NuberOfItemsToShowInProductFilters, storeId);
                model.CustomCss_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CustomCss, storeId);
                model.EnableCategoryCustomGridLayout_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCategoryCustomGridLayout, storeId);

                #endregion

                #region Carousels    

                model.EnableCarouselsForDefaultEntityLists_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCarouselsForDefaultEntityLists, storeId);
                model.EnableCarouselAutoPlay_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCarouselAutoPlay, storeId);
                model.CarouselAutoPlayTimeout_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselAutoPlayTimeout, storeId);
                model.CarouselAutoPlayHoverPause_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselAutoPlayHoverPause, storeId);
                model.EnableCarouselLoop_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCarouselLoop, storeId);
                model.EnableCarouselNavigation_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCarouselNavigation, storeId);
                model.EnableCarouselPagination_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableCarouselPagination, storeId);
                model.CarouselPaginationClickable_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselPaginationClickable, storeId);
                model.CarouselPaginationTypeId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselPaginationTypeId, storeId);
                model.CarouselPaginationDynamicBullets_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselPaginationDynamicBullets, storeId);
                model.CarouselPaginationDynamicMainBullets_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.CarouselPaginationDynamicMainBullets, storeId);

                #endregion

                #region Home page        

                model.HideHomePageBestSellers_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.HideHomePageBestSellers, storeId);
                model.HideHomePageFeaturedCategories_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.HideHomePageFeaturedCategories, storeId);
                model.HideHomePageProducts_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.HideHomePageProducts, storeId);

                #endregion

                #region Footer description

                model.EnableFooterDescriptionBoxOne_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableFooterDescriptionBoxOne, storeId);
                model.FooterDescriptionBoxOneTitle_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxOneTitle, storeId);
                model.FooterDescriptionBoxOneText_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxOneText, storeId);
                model.FooterDescriptionBoxOneUrl_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxOneUrl, storeId);
                model.FooterDescriptionBoxOnePictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxOnePictureId, storeId);

                model.EnableFooterDescriptionBoxTwo_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableFooterDescriptionBoxTwo, storeId);
                model.FooterDescriptionBoxTwoTitle_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxTwoTitle, storeId);
                model.FooterDescriptionBoxTwoText_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxTwoText, storeId);
                model.FooterDescriptionBoxTwoUrl_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxTwoUrl, storeId);
                model.FooterDescriptionBoxTwoPictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxTwoPictureId, storeId);

                model.EnableFooterDescriptionBoxThree_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableFooterDescriptionBoxThree, storeId);
                model.FooterDescriptionBoxThreeTitle_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxThreeTitle, storeId);
                model.FooterDescriptionBoxThreeText_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxThreeText, storeId);
                model.FooterDescriptionBoxThreeUrl_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxThreeUrl, storeId);
                model.FooterDescriptionBoxThreePictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxThreePictureId, storeId);

                model.EnableFooterDescriptionBoxFour_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.EnableFooterDescriptionBoxFour, storeId);
                model.FooterDescriptionBoxFourTitle_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxFourTitle, storeId);
                model.FooterDescriptionBoxFourText_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxFourText, storeId);
                model.FooterDescriptionBoxFourUrl_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxFourUrl, storeId);
                model.FooterDescriptionBoxFourPictureId_OverrideForStore = await _settingService.SettingExistsAsync(StafastSettings, x => x.FooterDescriptionBoxFourPictureId, storeId);

                #endregion
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StafastPermissionProvider.ManageStafast))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var StafastSettings = await _settingService.LoadSettingAsync<StafastSettings>(storeScope);
            StafastSettings = model.ToSettings(StafastSettings);

            #region Color
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.PrimaryThemeColor, model.PrimaryThemeColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.SecondaryThemeColor, model.SecondaryThemeColor_OverrideForStore, storeScope, false);

            #endregion

            #region General

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.PhoneNumber, model.PhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.LinkedInLink, model.LinkedInLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.PinterestLink, model.PinterestLink_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterContactUsText, model.FooterContactUsText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.HideDesignedByNopStationAtFooter, model.HideDesignedByNopStationAtFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.ShowLogoAtFooter, model.ShowLogoAtFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterLogoPictureId, model.FooterLogoPictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.ShowSupportedCardsAtFooter, model.ShowSupportedCardsAtFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterSupportedCardsPictureId, model.FooterSupportedCardsPictureId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableLoginBoxAtHeader, model.EnableLoginBoxAtHeader_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableImageLazyLoad, model.EnableImageLazyLoad_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.LazyLoadPictureId, model.LazyLoadPictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.LazyLoadPictureId, model.LazyLoadPictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.NewsLetterSideImageId, model.NewsLetterSideImageId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableStickyHeader, model.EnableStickyHeader_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.HideProductBoxButtonsOnMobile, model.HideProductBoxButtonsOnMobile_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.NuberOfItemsToShowInProductFilters, model.NuberOfItemsToShowInProductFilters_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CustomCss, model.CustomCss_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCategoryCustomGridLayout, model.EnableCategoryCustomGridLayout_OverrideForStore, storeScope, false);

            #endregion

            #region Carousels

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCarouselsForDefaultEntityLists, model.EnableCarouselsForDefaultEntityLists_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCarouselAutoPlay, model.EnableCarouselAutoPlay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselAutoPlayTimeout, model.CarouselAutoPlayTimeout_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselAutoPlayHoverPause, model.CarouselAutoPlayHoverPause_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCarouselLoop, model.EnableCarouselLoop_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCarouselNavigation, model.EnableCarouselNavigation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableCarouselPagination, model.EnableCarouselPagination_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselPaginationClickable, model.CarouselPaginationClickable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselPaginationTypeId, model.CarouselPaginationTypeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselPaginationDynamicBullets, model.CarouselPaginationDynamicBullets_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.CarouselPaginationDynamicMainBullets, model.CarouselPaginationDynamicMainBullets_OverrideForStore, storeScope, false);

            #endregion

            #region Home page

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.HideHomePageBestSellers, model.HideHomePageBestSellers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.HideHomePageFeaturedCategories, model.HideHomePageFeaturedCategories_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.HideHomePageProducts, model.HideHomePageProducts_OverrideForStore, storeScope, false);

            #endregion

            #region Footer description

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableFooterDescriptionBoxOne, model.EnableFooterDescriptionBoxOne_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxOneTitle, model.FooterDescriptionBoxOneTitle_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxOneText, model.FooterDescriptionBoxOneText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxOneUrl, model.FooterDescriptionBoxOneUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxOnePictureId, model.FooterDescriptionBoxOnePictureId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableFooterDescriptionBoxTwo, model.EnableFooterDescriptionBoxTwo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxTwoTitle, model.FooterDescriptionBoxTwoTitle_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxTwoText, model.FooterDescriptionBoxTwoText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxTwoUrl, model.FooterDescriptionBoxTwoUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxTwoPictureId, model.FooterDescriptionBoxTwoPictureId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableFooterDescriptionBoxThree, model.EnableFooterDescriptionBoxThree_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxThreeTitle, model.FooterDescriptionBoxThreeTitle_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxThreeText, model.FooterDescriptionBoxThreeText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxThreeUrl, model.FooterDescriptionBoxThreeUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxThreePictureId, model.FooterDescriptionBoxThreePictureId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.EnableFooterDescriptionBoxFour, model.EnableFooterDescriptionBoxFour_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxFourTitle, model.FooterDescriptionBoxFourTitle_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxFourText, model.FooterDescriptionBoxFourText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxFourUrl, model.FooterDescriptionBoxFourUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(StafastSettings, x => x.FooterDescriptionBoxFourPictureId, model.FooterDescriptionBoxFourPictureId_OverrideForStore, storeScope, false);

            #endregion

            await _settingService.ClearCacheAsync();
            await UpdateCssFilesAsync(storeScope);

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterContactUsText, localized.LanguageId, localized.FooterContactUsText);

                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxOneTitle, localized.LanguageId, localized.FooterDescriptionBoxOneTitle);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxOneText, localized.LanguageId, localized.FooterDescriptionBoxOneText);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxOneUrl, localized.LanguageId, localized.FooterDescriptionBoxOneUrl);

                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxTwoTitle, localized.LanguageId, localized.FooterDescriptionBoxTwoTitle);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxTwoText, localized.LanguageId, localized.FooterDescriptionBoxTwoText);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxTwoUrl, localized.LanguageId, localized.FooterDescriptionBoxTwoUrl);

                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxThreeTitle, localized.LanguageId, localized.FooterDescriptionBoxThreeTitle);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxThreeText, localized.LanguageId, localized.FooterDescriptionBoxThreeText);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxThreeUrl, localized.LanguageId, localized.FooterDescriptionBoxThreeUrl);

                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxFourTitle, localized.LanguageId, localized.FooterDescriptionBoxFourTitle);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxFourText, localized.LanguageId, localized.FooterDescriptionBoxFourText);
                await _localizationService.SaveLocalizedSettingAsync(StafastSettings,
                    x => x.FooterDescriptionBoxFourUrl, localized.LanguageId, localized.FooterDescriptionBoxFourUrl);
            }

            await _staticCacheManager.RemoveByPrefixAsync(ModelCacheKey.FooterDescriptionModelPattern);
            await _staticCacheManager.RemoveByPrefixAsync(ModelCacheKey.TopMenuModelPattern);
            await _staticCacheManager.RemoveByPrefixAsync(ModelCacheKey.SocialModelPattern);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
            return RedirectToAction("Configure");
        }

        #endregion
    }
}
