using Nop.Core.Configuration;

namespace NopStation.Plugin.Theme.Stafast;

public class StafastSettings : ISettings
{
    #region Color

    public string PrimaryThemeColor { get; set; }
    public string SecondaryThemeColor { get; set; }

    #endregion

    #region General

    public string PhoneNumber { get; set; }
    public string LinkedInLink { get; set; }
    public string PinterestLink { get; set; }
    public bool EnableLoginBoxAtHeader { get; set; }
    public bool EnableImageLazyLoad { get; set; }
    public int LazyLoadPictureId { get; set; }
    public bool EnableStickyHeader { get; set; }
    public bool HideProductBoxButtonsOnMobile { get; set; }
    public int NuberOfItemsToShowInProductFilters { get; set; }
    public string CustomCss { get; set; }

    public string FooterContactUsText { get; set; }
    public bool HideDesignedByNopStationAtFooter { get; set; }
    public bool ShowLogoAtFooter { get; set; }
    public int FooterLogoPictureId { get; set; }
    public bool ShowSupportedCardsAtFooter { get; set; }
    public int FooterSupportedCardsPictureId { get; set; }
    public int NewsLetterSideImageId { get; set; }

    public bool EnableCategoryCustomGridLayout { get; set; }

    #endregion

    #region Carousels                                                   

    public bool EnableCarouselsForDefaultEntityLists { get; set; }
    public bool EnableCarouselAutoPlay { get; set; }
    public int CarouselAutoPlayTimeout { get; set; }
    public bool CarouselAutoPlayHoverPause { get; set; }
    public bool EnableCarouselLoop { get; set; }
    public bool EnableCarouselNavigation { get; set; }
    public bool EnableCarouselPagination { get; set; }
    public bool CarouselPaginationClickable { get; set; }
    public int CarouselPaginationTypeId { get; set; }
    public bool CarouselPaginationDynamicBullets { get; set; }
    public int CarouselPaginationDynamicMainBullets { get; set; }

    public PaginationType PaginationType
    {
        get => (PaginationType)CarouselPaginationTypeId;
        set => CarouselPaginationTypeId = (int)value;
    }

    #endregion

    #region Home page

    public bool HideHomePageBestSellers { get; set; }
    public bool HideHomePageFeaturedCategories { get; set; }
    public bool HideHomePageProducts { get; set; }

    #endregion

    #region Footer description

    public bool EnableFooterDescriptionBoxOne { get; set; }
    public string FooterDescriptionBoxOneTitle { get; set; }
    public string FooterDescriptionBoxOneText { get; set; }
    public string FooterDescriptionBoxOneUrl { get; set; }
    public int FooterDescriptionBoxOnePictureId { get; set; }

    public bool EnableFooterDescriptionBoxTwo { get; set; }
    public string FooterDescriptionBoxTwoTitle { get; set; }
    public string FooterDescriptionBoxTwoText { get; set; }
    public string FooterDescriptionBoxTwoUrl { get; set; }
    public int FooterDescriptionBoxTwoPictureId { get; set; }

    public bool EnableFooterDescriptionBoxThree { get; set; }
    public string FooterDescriptionBoxThreeTitle { get; set; }
    public string FooterDescriptionBoxThreeText { get; set; }
    public string FooterDescriptionBoxThreeUrl { get; set; }
    public int FooterDescriptionBoxThreePictureId { get; set; }

    public bool EnableFooterDescriptionBoxFour { get; set; }
    public string FooterDescriptionBoxFourTitle { get; set; }
    public string FooterDescriptionBoxFourText { get; set; }
    public string FooterDescriptionBoxFourUrl { get; set; }
    public int FooterDescriptionBoxFourPictureId { get; set; }

    #endregion
}
