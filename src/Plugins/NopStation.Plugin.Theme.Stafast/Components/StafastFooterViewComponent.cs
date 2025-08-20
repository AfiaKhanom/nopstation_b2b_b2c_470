using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Media;
using NopStation.Plugin.Theme.Stafast.Infrastructure.Cache;
using NopStation.Plugin.Theme.Stafast.Models;

namespace NopStation.Plugin.Theme.Stafast.Components
{
    public class StafastFooterViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly StafastSettings _StafastSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public StafastFooterViewComponent(IWorkContext workContext,
            IStaticCacheManager cacheKeyService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            StafastSettings StafastSettings,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IWebHelper webHelper)
        {
            _workContext = workContext;
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _StafastSettings = StafastSettings;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_StafastSettings.EnableFooterDescriptionBoxOne && !_StafastSettings.EnableFooterDescriptionBoxTwo &&
                !_StafastSettings.EnableFooterDescriptionBoxThree && !_StafastSettings.EnableFooterDescriptionBoxFour)
            {
                return Content("");
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();

            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(ModelCacheKey.FooterDescriptionModelKey,
                store, language,
                _webHelper.IsCurrentConnectionSecured());

            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new FooterDescriptionModel();
                if (_StafastSettings.EnableFooterDescriptionBoxOne)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(_StafastSettings.FooterDescriptionBoxOnePictureId);
                    var titleOne = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxOneTitle, language.Id, store.Id);
                    model.Description1 = new FooterDescriptionModel.FooterDescriptionBoxModel
                    {
                        Enabled = true,
                        Title = titleOne,
                        Text = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxOneText, language.Id, store.Id),
                        Url = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxOneUrl, language.Id, store.Id),
                        Picture = new PictureModel
                        {
                            ThumbImageUrl = pictureUrl,
                            FullSizeImageUrl = pictureUrl,
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.AlterText"), titleOne),
                            Title = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.Title"), titleOne),
                        }
                    };
                }
                if (_StafastSettings.EnableFooterDescriptionBoxTwo)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(_StafastSettings.FooterDescriptionBoxTwoPictureId);
                    var titleTwo = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxTwoTitle, language.Id, store.Id);
                    model.Description2 = new FooterDescriptionModel.FooterDescriptionBoxModel
                    {
                        Enabled = true,
                        Title = titleTwo,
                        Text = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxTwoText, language.Id, store.Id),
                        Url = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxTwoUrl, language.Id, store.Id),
                        Picture = new PictureModel
                        {
                            ThumbImageUrl = pictureUrl,
                            FullSizeImageUrl = pictureUrl,
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.AlterText"), titleTwo),
                            Title = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.Title"), titleTwo),
                        }
                    };
                }
                if (_StafastSettings.EnableFooterDescriptionBoxThree)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(_StafastSettings.FooterDescriptionBoxThreePictureId);
                    var titleThree = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxThreeTitle, language.Id, store.Id);
                    model.Description3 = new FooterDescriptionModel.FooterDescriptionBoxModel
                    {
                        Enabled = true,
                        Title = titleThree,
                        Text = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxThreeText, language.Id, store.Id),
                        Url = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxThreeUrl, language.Id, store.Id),
                        Picture = new PictureModel
                        {
                            ThumbImageUrl = pictureUrl,
                            FullSizeImageUrl = pictureUrl,
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.AlterText"), titleThree),
                            Title = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.Title"), titleThree),
                        }
                    };
                }
                if (_StafastSettings.EnableFooterDescriptionBoxFour)
                {
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(_StafastSettings.FooterDescriptionBoxFourPictureId);
                    var titleFour = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxFourTitle, language.Id, store.Id);
                    model.Description4 = new FooterDescriptionModel.FooterDescriptionBoxModel
                    {
                        Enabled = true,
                        Title = titleFour,
                        Text = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxFourText, language.Id, store.Id),
                        Url = await _localizationService.GetLocalizedSettingAsync(_StafastSettings, x => x.FooterDescriptionBoxFourUrl, language.Id, store.Id),
                        Picture = new PictureModel
                        {
                            ThumbImageUrl = pictureUrl,
                            FullSizeImageUrl = pictureUrl,
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.AlterText"), titleFour),
                            Title = string.Format(await _localizationService.GetResourceAsync("NopStation.Theme.Stafast.Picture.Title"), titleFour),
                        }
                    };
                }
                return model;
            });

            return View(cachedModel);
        }

        #endregion
    }
}
