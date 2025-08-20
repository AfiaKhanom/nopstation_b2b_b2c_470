using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Theme.Stafast.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel, ILocalizedModel<ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }

        #region Color
        [UIHint("Color")]
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.PrimaryThemeColor")]
        public string PrimaryThemeColor { get; set; }
        public bool PrimaryThemeColor_OverrideForStore { get; set; }

        [UIHint("Color")]
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.SecondaryThemeColor")]
        public string SecondaryThemeColor { get; set; }
        public bool SecondaryThemeColor_OverrideForStore { get; set; }

        #endregion

        #region General

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.LinkedInLink")]
        public string LinkedInLink { get; set; }
        public bool LinkedInLink_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.PinterestLink")]
        public string PinterestLink { get; set; }
        public bool PinterestLink_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableLoginBoxAtHeader")]
        public bool EnableLoginBoxAtHeader { get; set; }
        public bool EnableLoginBoxAtHeader_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableImageLazyLoad")]
        public bool EnableImageLazyLoad { get; set; }
        public bool EnableImageLazyLoad_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.LazyLoadPictureId")]
        [UIHint("Picture")]
        public int LazyLoadPictureId { get; set; }
        public bool LazyLoadPictureId_OverrideForStore { get; set; }
       
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableStickyHeader")]
        public bool EnableStickyHeader { get; set; }
        public bool EnableStickyHeader_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.HideProductBoxButtonsOnMobile")]
        public bool HideProductBoxButtonsOnMobile { get; set; }
        public bool HideProductBoxButtonsOnMobile_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.NuberOfItemsToShowInProductFilters")]
        public int NuberOfItemsToShowInProductFilters { get; set; }
        public bool NuberOfItemsToShowInProductFilters_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CustomCss")]
        public string CustomCss { get; set; }
        public bool CustomCss_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterContactUsText")]
        public string FooterContactUsText { get; set; }
        public bool FooterContactUsText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.HideDesignedByNopStationAtFooter")]
        public bool HideDesignedByNopStationAtFooter { get; set; }
        public bool HideDesignedByNopStationAtFooter_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowLogoAtFooter")]
        public bool ShowLogoAtFooter { get; set; }
        public bool ShowLogoAtFooter_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterLogoPictureId")]
        [UIHint("Picture")]
        public int FooterLogoPictureId { get; set; }
        public bool FooterLogoPictureId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowSupportedCardsAtFooter")]
        public bool ShowSupportedCardsAtFooter { get; set; }
        public bool ShowSupportedCardsAtFooter_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterSupportedCardsPictureId")]
        [UIHint("Picture")]
        public int FooterSupportedCardsPictureId { get; set; }
        public bool FooterSupportedCardsPictureId_OverrideForStore { get; set; }
        

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.NewsLetterSideImageId")]
        [UIHint("Picture")]
        public int NewsLetterSideImageId { get; set; }
        public bool NewsLetterSideImageId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCategoryCustomGridLayout")]
        public bool EnableCategoryCustomGridLayout { get; set; }
        public bool EnableCategoryCustomGridLayout_OverrideForStore { get; set; }

        #endregion

        #region Carousels                                                   

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselsForDefaultEntityLists")]
        public bool EnableCarouselsForDefaultEntityLists { get; set; }
        public bool EnableCarouselsForDefaultEntityLists_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselAutoPlay")]
        public bool EnableCarouselAutoPlay { get; set; }
        public bool EnableCarouselAutoPlay_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayTimeout")]
        public int CarouselAutoPlayTimeout { get; set; }
        public bool CarouselAutoPlayTimeout_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayHoverPause")]
        public bool CarouselAutoPlayHoverPause { get; set; }
        public bool CarouselAutoPlayHoverPause_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselLoop")]
        public bool EnableCarouselLoop { get; set; }
        public bool EnableCarouselLoop_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselNavigation")]
        public bool EnableCarouselNavigation { get; set; }
        public bool EnableCarouselNavigation_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselPagination")]
        public bool EnableCarouselPagination { get; set; }
        public bool EnableCarouselPagination_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationClickable")]
        public bool CarouselPaginationClickable { get; set; }
        public bool CarouselPaginationClickable_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationTypeId")]
        public int CarouselPaginationTypeId { get; set; }
        public bool CarouselPaginationTypeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicBullets")]
        public bool CarouselPaginationDynamicBullets { get; set; }
        public bool CarouselPaginationDynamicBullets_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicMainBullets")]
        public int CarouselPaginationDynamicMainBullets { get; set; }
        public bool CarouselPaginationDynamicMainBullets_OverrideForStore { get; set; }

        #endregion

        #region Home page

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageBestSellers")]
        public bool HideHomePageBestSellers { get; set; }
        public bool HideHomePageBestSellers_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageFeaturedCategories")]
        public bool HideHomePageFeaturedCategories { get; set; }
        public bool HideHomePageFeaturedCategories_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageProducts")]
        public bool HideHomePageProducts { get; set; }
        public bool HideHomePageProducts_OverrideForStore { get; set; }

        #endregion

        #region Footer description

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxOne")]
        public bool EnableFooterDescriptionBoxOne { get; set; }
        public bool EnableFooterDescriptionBoxOne_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneTitle")]
        public string FooterDescriptionBoxOneTitle { get; set; }
        public bool FooterDescriptionBoxOneTitle_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneText")]
        public string FooterDescriptionBoxOneText { get; set; }
        public bool FooterDescriptionBoxOneText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneUrl")]
        public string FooterDescriptionBoxOneUrl { get; set; }
        public bool FooterDescriptionBoxOneUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOnePictureId")]
        [UIHint("Picture")]
        public int FooterDescriptionBoxOnePictureId { get; set; }
        public bool FooterDescriptionBoxOnePictureId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxTwo")]
        public bool EnableFooterDescriptionBoxTwo { get; set; }
        public bool EnableFooterDescriptionBoxTwo_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoTitle")]
        public string FooterDescriptionBoxTwoTitle { get; set; }
        public bool FooterDescriptionBoxTwoTitle_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoText")]
        public string FooterDescriptionBoxTwoText { get; set; }
        public bool FooterDescriptionBoxTwoText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoUrl")]
        public string FooterDescriptionBoxTwoUrl { get; set; }
        public bool FooterDescriptionBoxTwoUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoPictureId")]
        [UIHint("Picture")]
        public int FooterDescriptionBoxTwoPictureId { get; set; }
        public bool FooterDescriptionBoxTwoPictureId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxThree")]
        public bool EnableFooterDescriptionBoxThree { get; set; }
        public bool EnableFooterDescriptionBoxThree_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeTitle")]
        public string FooterDescriptionBoxThreeTitle { get; set; }
        public bool FooterDescriptionBoxThreeTitle_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeText")]
        public string FooterDescriptionBoxThreeText { get; set; }
        public bool FooterDescriptionBoxThreeText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeUrl")]
        public string FooterDescriptionBoxThreeUrl { get; set; }
        public bool FooterDescriptionBoxThreeUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreePictureId")]
        [UIHint("Picture")]
        public int FooterDescriptionBoxThreePictureId { get; set; }
        public bool FooterDescriptionBoxThreePictureId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxFour")]
        public bool EnableFooterDescriptionBoxFour { get; set; }
        public bool EnableFooterDescriptionBoxFour_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourTitle")]
        public string FooterDescriptionBoxFourTitle { get; set; }
        public bool FooterDescriptionBoxFourTitle_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourText")]
        public string FooterDescriptionBoxFourText { get; set; }
        public bool FooterDescriptionBoxFourText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourUrl")]
        public string FooterDescriptionBoxFourUrl { get; set; }
        public bool FooterDescriptionBoxFourUrl_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourPictureId")]
        [UIHint("Picture")]
        public int FooterDescriptionBoxFourPictureId { get; set; }
        public bool FooterDescriptionBoxFourPictureId_OverrideForStore { get; set; }

        #endregion

        public int ActiveStoreScopeConfiguration { get; set; }
        public IList<ConfigurationLocalizedModel> Locales { get; set; }
    }

    public class ConfigurationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        #region Footer description

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterContactUsText")]
        public string FooterContactUsText { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneTitle")]
        public string FooterDescriptionBoxOneTitle { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneText")]
        public string FooterDescriptionBoxOneText { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneUrl")]
        public string FooterDescriptionBoxOneUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoTitle")]
        public string FooterDescriptionBoxTwoTitle { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoText")]
        public string FooterDescriptionBoxTwoText { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoUrl")]
        public string FooterDescriptionBoxTwoUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeTitle")]
        public string FooterDescriptionBoxThreeTitle { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeText")]
        public string FooterDescriptionBoxThreeText { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeUrl")]
        public string FooterDescriptionBoxThreeUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourTitle")]
        public string FooterDescriptionBoxFourTitle { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourText")]
        public string FooterDescriptionBoxFourText { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourUrl")]
        public string FooterDescriptionBoxFourUrl { get; set; }

        #endregion
    }
}
