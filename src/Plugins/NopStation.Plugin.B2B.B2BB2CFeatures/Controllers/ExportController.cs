using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class ExportController : BasePublicController
{
    #region Fields

    private readonly ICategoryService _categoryService;
    private readonly CatalogSettings _catalogSettings;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly INotificationService _notificationService;
    private readonly ICategoryProductsExportManager _categoryProductsExportManager;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;

    #endregion

    #region Ctor

    public ExportController(ICategoryService categoryService,
        CatalogSettings catalogSettings,
        IProductService productService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICurrencyService currencyService,
        INotificationService notificationService,
        ICategoryProductsExportManager categoryExportManager,
        IUrlRecordService urlRecordService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService)
    {
        _categoryService = categoryService;
        _catalogSettings = catalogSettings;
        _productService = productService;
        _workContext = workContext;
        _storeContext = storeContext;
        _notificationService = notificationService;
        _categoryProductsExportManager = categoryExportManager;
        _urlRecordService = urlRecordService;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> ExportProductsByCategory(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var language = await _workContext.GetWorkingLanguageAsync();
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        ArgumentNullException.ThrowIfNull(category);

        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        var categoryIds = new List<int>();
        var categories = await _categoryService.GetAllCategoriesAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        foreach (var item in categories)
        {
            categoryIds.Add(item.Id);

            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: item.Id, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
            if (_catalogSettings.ShowProductsFromSubcategories)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(item.Id, currentStore.Id));
        }

        if (categoryIds.Count <= 0)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.ExportCategoryProducts.Error.NoCategoryFound"));
            return Redirect(returnUrl);
        }

        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            var bytes = await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);

            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
        }

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> ExportProductsByCategoryId(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var language = await _workContext.GetWorkingLanguageAsync();
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        ArgumentNullException.ThrowIfNull(category);

        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var categoryIds = new List<int> { category.Id };

        var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: category.Id, showHidden: true);
        categoryIds.AddRange(childCategoryIds);

        //include subcategories
        if (_catalogSettings.ShowProductsFromSubcategories)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));


        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            var bytes = await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);

            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
        }

        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        return Redirect(returnUrl);
    }

    #endregion
}