using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Theme.Stafast.Components;

namespace NopStation.Plugin.Theme.Stafast;

public class StafastPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly IPictureService _pictureService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly IEmailAccountService _emailAccountService;

    #endregion Fields

    #region Ctor

    public StafastPlugin(ISettingService settingService,
        IWebHelper webHelper,
        INopFileProvider nopFileProvider,
        IPictureService pictureService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        INopStationCoreService nopStationCoreService,
        IStoreContext storeContext,
        IStoreService storeService,
        IEmailAccountService emailAccountService)
    {
        _settingService = settingService;
        _webHelper = webHelper;
        _fileProvider = nopFileProvider;
        _pictureService = pictureService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _nopStationCoreService = nopStationCoreService;
        _storeContext = storeContext;
        _storeService = storeService;
        _emailAccountService = emailAccountService;
    }

    #endregion Ctor

    #region Utilities

    private async Task CreateSampleDataAsync()
    {
        var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Theme.Stafast/Content/sample/");
        var store = _storeContext.GetCurrentStore() ?? (await _storeService.GetAllStoresAsync()).FirstOrDefault();
        var email = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        var settings = new StafastSettings()
        {
            EnableImageLazyLoad = true,
            LazyLoadPictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "lazy-load.png")), MimeTypes.ImagePng, "lazy-load")).Id,
            FooterSupportedCardsPictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "footer-card-icons.png"))), MimeTypes.ImagePng, "footer-cards")).Id,
            FooterLogoPictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "footer-logo-white.png"))), MimeTypes.ImagePng, "footer-logo")).Id,
            ShowLogoAtFooter = true,
            ShowSupportedCardsAtFooter = true,
            FooterDescriptionBoxOneTitle = "Free Shipping",
            FooterDescriptionBoxOneText = "Free Shipping for orders over $150",
            FooterDescriptionBoxOnePictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "free-shipping.png"))), MimeTypes.ImagePng, "free-shipping")).Id,
            FooterDescriptionBoxTwoTitle = "Money Guarantee",
            FooterDescriptionBoxTwoText = "Within 30 days for an exchange",
            FooterDescriptionBoxTwoPictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "30-days-return-policy.png"))), MimeTypes.ImagePng, "30-days-return-policy")).Id,
            FooterDescriptionBoxThreeTitle = "Online Support",
            FooterDescriptionBoxThreeText = "24 hours a day, 7 days a week",
            FooterDescriptionBoxThreePictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "support-24-7.png"))), MimeTypes.ImagePng, "support-24-7")).Id,
            FooterDescriptionBoxFourTitle = "Flexible Payment",
            FooterDescriptionBoxFourText = "Pay with Multiple Credit Cards",
            FooterDescriptionBoxFourPictureId = (await _pictureService.InsertPictureAsync((await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "flexible-payment.png"))), MimeTypes.ImagePng, "flexible-payment")).Id,
            FooterContactUsText = $"<p>{store?.CompanyAddress}</p><div>&nbsp;</div><div style=\"line-height: 1.5;\">Email : {email?.Email}</div><div style=\"line-height: 1.5;\">Phone : +8801719304086</div>",

            HideDesignedByNopStationAtFooter = false,
            EnableFooterDescriptionBoxFour = true,
            EnableFooterDescriptionBoxOne = true,
            EnableFooterDescriptionBoxThree = true,
            EnableFooterDescriptionBoxTwo = true,
            EnableStickyHeader = true,
            HideHomePageBestSellers = true,
            HideHomePageFeaturedCategories = true,
            HideHomePageProducts = true,
            HideProductBoxButtonsOnMobile = true,
            EnableCategoryCustomGridLayout = false,

            PrimaryThemeColor = StafastDefaults.PrimaryThemeColor,
            SecondaryThemeColor = StafastDefaults.SecondaryThemeColor,
            NuberOfItemsToShowInProductFilters = 5,
            PhoneNumber = "+8801719304086",
            PinterestLink = "#",
            LinkedInLink = "#",
            CustomCss = "body {" + Environment.NewLine + "}",
            CarouselAutoPlayHoverPause = true,
            CarouselAutoPlayTimeout = 3000,
            CarouselPaginationClickable = true,
            CarouselPaginationDynamicBullets = true,
            CarouselPaginationDynamicMainBullets = 3,
            CarouselPaginationTypeId = (int)PaginationType.Bullets,
            EnableCarouselAutoPlay = true,
            EnableCarouselLoop = true,
            EnableCarouselNavigation = true,
            EnableCarouselPagination = true,
            EnableCarouselsForDefaultEntityLists = true,
            EnableLoginBoxAtHeader = true,
            FooterDescriptionBoxFourUrl = "",
            FooterDescriptionBoxOneUrl = "",
            FooterDescriptionBoxThreeUrl = "",
            FooterDescriptionBoxTwoUrl = ""
        };
        await _settingService.SaveSettingAsync(settings);
    }

    #endregion Utilities

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/Stafast/Configure";
    }

    public override async Task InstallAsync()
    {
        await CreateSampleDataAsync();
        await this.InstallPluginAsync(new StafastPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new StafastPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Theme.Stafast.Menu.Stafast"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(StafastPermissionProvider.ManageStafast))
        {
            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Theme.Stafast.Menu.Configuration"),
                Url = "~/Admin/Stafast/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Stafast.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "#",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Theme);
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == StafastDefaults.FooterBeforeWidgetZone)
            return typeof(StafastFooterViewComponent);
        if (widgetZone == StafastDefaults.HeaderLinksMiddleWidgetZone)
            return typeof(StafastHeaderLoginViewComponent);
        if (widgetZone == StafastDefaults.SocialButtonsAfterWidgetZone)
            return typeof(StafastSocialButtonsViewComponent);

        return typeof(StafastViewComponent);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var result = new List<string> {
            StafastDefaults.HeaderLinksMiddleWidgetZone,
            StafastDefaults.FooterBeforeWidgetZone,
            StafastDefaults.SocialButtonsAfterWidgetZone,
            PublicWidgetZones.Footer
        };

        return Task.FromResult<IList<string>>(result);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>()
        {
            ["Admin.NopStation.Theme.Stafast.Menu.Stafast"] = "Stafast",
            ["Admin.NopStation.Theme.Stafast.Menu.Configuration"] = "Configuration",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PrimaryThemeColor"] = "Primary theme color",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PrimaryThemeColor.Hint"] = "Choose primary color for theme.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.SecondaryThemeColor"] = "Secondary theme color",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.SecondaryThemeColor.Hint"] = "Choose dark color for theme.",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PhoneNumber"] = "Phone number",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PhoneNumber.Hint"] = "Specify phone number which which will be displayed in public store.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.LinkedInLink"] = "LinkedIn Link",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.LinkedInLink.Hint"] = "Specify your LinkedIn link which will be displayed in your public store.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PinterestLink"] = "Pinterest Link",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.PinterestLink.Hint"] = "Specify your Pinterest link which will be displayed in your public store.",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableLoginBoxAtHeader"] = "Enable login box at header",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableLoginBoxAtHeader.Hint"] = "Check to enable login box at header.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableImageLazyLoad"] = "Enable image lazy-load",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableImageLazyLoad.Hint"] = "Check to enable lazy-load for product box image.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.LazyLoadPictureId"] = "Lazy-load picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.LazyLoadPictureId.Hint"] = "This picture will be displayed initially in product box. Uploaded picture size should not be more than 4-5 KB.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableStickyHeader"] = "Enable stickey header",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableStickyHeader.Hint"] = "Check to enable stickey header.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideProductBoxButtonsOnMobile"] = "Hide product box buttons on mobile",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideProductBoxButtonsOnMobile.Hint"] = "Check to hide product box buttons on mobile.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.NuberOfItemsToShowInProductFilters"] = "Nuber of items to show in product filters",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.NuberOfItemsToShowInProductFilters.Hint"] = "Define nuber of items to show in product filters.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CustomCss"] = "Custom CSS",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CustomCss.Hint"] = "Write custom CSS for your site.",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterContactUsText"] = "Footer. Contact us text",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterContactUsText.Hint"] = "Footer. Contact us text at footer.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideDesignedByNopStationAtFooter"] = "Footer. Hide \"designed by nopStation\"",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideDesignedByNopStationAtFooter.Hint"] = "Footer. Check to hide \"designed by nopStation\" at footer.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowLogoAtFooter"] = "Footer. Show logo",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowLogoAtFooter.Hint"] = "Footer. Check to show logo at footer.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterLogoPictureId"] = "Footer. Logo",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterLogoPictureId.Hint"] = "Footer. Logo to display at footer.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowSupportedCardsAtFooter"] = "Footer. Show supported cards",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.ShowSupportedCardsAtFooter.Hint"] = "Footer. Check to show supported cards at footer.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterSupportedCardsPictureId"] = "Footer. Supported cards picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterSupportedCardsPictureId.Hint"] = "Footer. Select footer supported cards picture.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselsForDefaultEntityLists"] = "Enable carousels for default entity lists",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselsForDefaultEntityLists.Hint"] = "Check to enable carousels for default entity lists (i.e. Home page categories, Featured products, Related products, Products also purchased, Cross sell products).",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselAutoPlay"] = "Carousel. Enable auto play",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselAutoPlay.Hint"] = "Carousel. Check to enable auto play.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayTimeout"] = "Carousel. Auto play timeout",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayTimeout.Hint"] = "Carousel. It's autoplay interval timeout. (e.g 5000)",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayHoverPause"] = "Carousel. Auto play hover pause",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselAutoPlayHoverPause.Hint"] = "Carousel. Check to enable pause on mouse hover.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselLoop"] = "Carousel. Enable loop",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselLoop.Hint"] = "Carousel. Check to enable loop.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselNavigation"] = "Carousel. Enable navigation",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselNavigation.Hint"] = "Carousel. Check to enable next/prev buttons.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselPagination"] = "Carousel. Enable pagination",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCarouselPagination.Hint"] = "Carousel. Check to enable pagination.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationTypeId"] = "Carousel. Pagination type",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationTypeId.Hint"] = "Carousel. Select pagination type.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicMainBullets"] = "Carousel. Dynamic main bullets",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicMainBullets.Hint"] = "Carousel. The number of main bullets visible when dynamic bullets enabled.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicBullets"] = "Carousel. Dynamic bullets",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationDynamicBullets.Hint"] = "Carousel. Good to enable if you use bullets pagination with a lot of slides. So it will keep only few bullets visible at the same time.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationClickable"] = "Carousel. Pagination clickable",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.CarouselPaginationClickable.Hint"] = "Carousel. If true then clicking on pagination button will cause transition to appropriate slide. Only for bullets pagination type.",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageBestSellers"] = "Home page. Hide best sellers",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageBestSellers.Hint"] = "Home page. Check to hide home page best sellers.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageFeaturedCategories"] = "Home page. Hide featured categories",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageFeaturedCategories.Hint"] = "Home page. Check to hide home page featured categories.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageProducts"] = "Home page. Hide home page products",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.HideHomePageProducts.Hint"] = "Home page. Check to hide home page products.",

            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxOne"] = "Description box. Enable box one",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxOne.Hint"] = "Description box. Check to enable description box one.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneTitle"] = "Description box. Box one title",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneTitle.Hint"] = "Description box. Enter box one title.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneText"] = "Description box. Box one text",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneText.Hint"] = "Description box. Enter description box one text.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneUrl"] = "Description box. Box one URL",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOneUrl.Hint"] = "Description box. Enter description box one URL.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOnePictureId"] = "Description box. Box one picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxOnePictureId.Hint"] = "Description box. Enter description box one picture.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxTwo"] = "Description box. Enable box two",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxTwo.Hint"] = "Description box. Check to enable description box two.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoTitle"] = "Description box. Box two title",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoTitle.Hint"] = "Description box. Enter description box two title.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoText"] = "Description box. Box two text",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoText.Hint"] = "Description box. Enter description box two text.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoUrl"] = "Description box. Box two URL",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoUrl.Hint"] = "Description box. Enter description box two URL.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoPictureId"] = "Description box. Box two picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxTwoPictureId.Hint"] = "Description box. Enter description box two picture.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxThree"] = "Description box. Enable box three",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxThree.Hint"] = "Description box. Check to enable description box three.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeTitle"] = "Description box. Box three title",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeTitle.Hint"] = "Description box. Enter description box three title.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeText"] = "Description box. Box three text",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeText.Hint"] = "Description box. Enter description box three text.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeUrl"] = "Description box. Box three URL",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreeUrl.Hint"] = "Description box. Enter description box three URL.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreePictureId"] = "Description box. Box three picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxThreePictureId.Hint"] = "Description box. Enter description box three picture.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxFour"] = "Description box. Enable box four",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableFooterDescriptionBoxFour.Hint"] = "Description box. Check to enable description box four.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourTitle"] = "Description box. Box four title",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourTitle.Hint"] = "Description box. Enter description box four title.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourText"] = "Description box. Box four text",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourText.Hint"] = "Description box. Enter description box four text.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourUrl"] = "Description box. Box four URL",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.FooterDescriptionBoxFourUrl.Hint"] = "Description box. Enter description box four URL.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.NewsLetterSideImageId"] = "News letter background picture",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.NewsLetterSideImageId.Hint"] = "News letter background picture.",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCategoryCustomGridLayout"] = "Enable category custom grid layout",
            ["Admin.NopStation.Theme.Stafast.Configuration.Fields.EnableCategoryCustomGridLayout.Hint"] = "Enable custom grid layout(zigzag) for category page.",

            ["Admin.NopStation.Theme.Stafast.Configuration"] = "Stafast theme settings",
            ["Admin.NopStation.Theme.Stafast.Configuration.TabTitle.Color"] = "Color",
            ["Admin.NopStation.Theme.Stafast.Configuration.TabTitle.General"] = "General",
            ["Admin.NopStation.Theme.Stafast.Configuration.TabTitle.Carousel"] = "Carousel",
            ["Admin.NopStation.Theme.Stafast.Configuration.TabTitle.HomePage"] = "Home page",
            ["Admin.NopStation.Theme.Stafast.Configuration.TabTitle.FooterDescription"] = "Footer description",

            ["Admin.NopStation.Theme.Stafast.Configuration.FooterDescriptionOne"] = "Description one",
            ["Admin.NopStation.Theme.Stafast.Configuration.FooterDescriptionTwo"] = "Description two",
            ["Admin.NopStation.Theme.Stafast.Configuration.FooterDescriptionThree"] = "Description three",
            ["Admin.NopStation.Theme.Stafast.Configuration.FooterDescriptionFour"] = "Description four",

            ["Admin.NopStation.Theme.Stafast.Configuration.Carousel.Hint"] = "Settings to configure carousel for Home page categories (home page), Featured products (home page), Related products (product details page), Products also purchased (product details page), Cross sell products (cart page)",

            ["NopStation.Theme.Stafast.Picture.AlterText"] = "Picture of {0}",
            ["NopStation.Theme.Stafast.Picture.Title"] = "Picture of {0}",
            ["NopStation.Theme.Stafast.FooterSupportedCards.AlterText"] = "Supported cards",
            ["NopStation.Theme.Stafast.FooterSupportedCards.Title"] = "Supported cards",
            ["NopStation.Theme.Stafast.ProductDetailsSupportedCards.AlterText"] = "Supported cards",
            ["NopStation.Theme.Stafast.ProductDetailsSupportedCards.Title"] = "Supported cards",
            ["NopStation.Theme.Stafast.ProductDetailsSupportedCards.WeAccept"] = "Supported cards",
            ["NopStation.Theme.Stafast.FooterLogo.AlterText"] = "Logo of {0}",
            ["NopStation.Theme.Stafast.FooterLogo.Title"] = "Logo of {0}",

            ["NopStation.Theme.Stafast.UserAccount"] = "Account",
            ["NopStation.Theme.Stafast.Footer.ContactUs"] = "Contact Us",
            ["NopStation.Theme.Stafast.HeaderLogin.DoNotHaveAccount"] = "Don't have an account?",
            ["NopStation.Theme.Stafast.HeaderLogin.CreateAccount"] = "Create an account",
            ["NopStation.Theme.Stafast.HomePage.FeaturedCategories"] = "Featured Categories",
            ["NopStation.Theme.Stafast.HomePage.BlogNews.LatestNews"] = "Latest News",
            ["NopStation.Theme.Stafast.HomePage.BlogNews.LatestBlogs"] = "Latest Blogs",
            ["NopStation.Theme.Stafast.HomePage.BlogNews.ViewAll"] = "View All",
            ["NopStation.Theme.Stafast.Catalog.PerPage"] = "Per Page",
            ["NopStation.Theme.Stafast.ProductDetails.AddToWisthlist"] = "Wisthlist",
            ["NopStation.Theme.Stafast.ProductDetails.AddToCompare"] = "Compare",
            ["NopStation.Theme.Stafast.ProductDetails.FullDescription"] = "Full Description",
            ["NopStation.Theme.Stafast.ProductDetails.SpecificationAttributes"] = "Specifications",
            ["NopStation.Theme.Stafast.ProductDetails.Tags"] = "Tags",
            ["NopStation.Theme.Stafast.Mobile.ShoppingCart"] = "Cart",
            ["NopStation.Theme.Stafast.Filter.Button"] = "Filter",
            ["Nopstation.Theme.Stafast.order.complete"] = "Order Completed",
            ["NopStation.Theme.Stafast.Filter.Min"] = "Min",
            ["NopStation.Theme.Stafast.Filter.Max"] = "Max",
            ["common.wait..."] = "Wait",
            ["nopstation.Theme.Stafast.carttable.info"] = "Cart Info",
            ["NopStation.Theme.Stafast.Mobile.ShoppingCart"] = "Cart",
            ["NopStation.Theme.Stafast.Footer.FollowUs.LinkedIn"] = "LinkedIn",
            ["NopStation.Theme.Stafast.Footer.FollowUs.Pinterest"] = "Pinterest",

            ["NopStation.Theme.Stafast.Shopping.Cart"] = "Cart",
            ["NopStation.Theme.Stafast.Customer.Login"] = "Log in",
            ["NopStation.Theme.Stafast.customer.Hello"] = "Hello",
            ["NopStation.Theme.Stafast.Wishlist.Header.Icon"] = "Wish list",
            ["NopStation.Theme.Stafast.Cart.Mobiletoggle.Info"] = "Info",
            ["NopStation.Theme.Stafast.Newsletter.Subtitle"] = "Newsletter subtitle",
            ["nopstation.theme.newsletter.subtitle"] = "Newsletter subtitle",
            ["admin.nopstation.theme.Stafast.configuration.fields.footerdescriptionboxfourpictureid"] = "Footer Description",
        };

        return list.ToList();
    }

    #endregion Methods
}