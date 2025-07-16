using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderTemplates;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;

public class QuickOrderTemplateModelFactory : IQuickOrderTemplateModelFactory
{
    #region Fields

    private readonly IShoppingCartService _shoppingCartService;
    private readonly IQuickOrderTemplateService _quickOrderTemplateService;
    private readonly IQuickOrderItemService _quickOrderItemService;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IDateTimeHelper _dateTimeHelper;

    #endregion

    #region Ctor

    public QuickOrderTemplateModelFactory(IQuickOrderTemplateService quickOrderTemplateService,
        IQuickOrderItemService quickOrderItemService,
        IProductService productService,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IDateTimeHelper dateTimeHelper,
        IShoppingCartService shoppingCartService)
    {
        _quickOrderTemplateService = quickOrderTemplateService;
        _quickOrderItemService = quickOrderItemService;
        _productService = productService;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _storeContext = storeContext;
        _dateTimeHelper = dateTimeHelper;
        _shoppingCartService = shoppingCartService;
    }

    #endregion

    #region Methods

    private async Task<(int, decimal)> QuickOrderTemplateCountAndTotalPriceAsync(QuickOrderTemplate quickOrder, bool loadTotalPrice = false)
    {
        decimal total = 0;
        var items = await _quickOrderItemService.GetAllQuickOrderItemsAsync(quickOrder.Id);

        if (items == null)
        {
            return (0, 0);
        }

        if (!loadTotalPrice)
            return (items.Count(), quickOrder.TotalPriceOfItems);

        if (quickOrder.LastPriceCalculatedOnUtc == null || quickOrder.LastPriceCalculatedOnUtc.Value < DateTime.UtcNow)
        {
            foreach (var item in items)
            {
                var product = await _productService.GetProductBySkuAsync(item.ProductSku);
                if (product == null)
                    continue;

                var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                        await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(),
                        ShoppingCartType.ShoppingCart, 1, item.AttributesXml, 0, null, null, true);

                total += finalPrice * item.Quantity;
            }

            quickOrder.TotalPriceOfItems = total;
            quickOrder.LastPriceCalculatedOnUtc = DateTime.UtcNow;
            await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrder);
        }

        return (items.Count(), quickOrder?.TotalPriceOfItems ?? 0);
    }

    public async Task<QuickOrderTemplateListModel> PrepareQuickOrderTemplateListModelAsync(QuickOrderTemplateSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var customer = await _workContext.GetCurrentCustomerAsync();
        //customer currency
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
           await _genericAttributeService.GetAttributeAsync<int>(customer, customer.CustomCustomerAttributesXML, (await _storeContext.GetCurrentStoreAsync()).Id));
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        DateTime? createdOnUtc = null;
        if (searchModel.SearchCreatedOn.HasValue)
        {
            createdOnUtc = _dateTimeHelper.ConvertToUtcTime(searchModel.SearchCreatedOn.Value);
        }

        // get QuickOrderTemplates
        var quickOrderTemplates = await _quickOrderTemplateService.GetAllQuickOrderTemplatesAsync(name: searchModel.SearchName, customerId: searchModel.SearchCustomerId,
            createdOnUtc: createdOnUtc, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new QuickOrderTemplateListModel().PrepareToGridAsync(searchModel, quickOrderTemplates, () =>
        {
            return quickOrderTemplates.SelectAwait(async quickOrder =>
            {
                //fill in model values from the entity
                var quickOrderTemplate = new QuickOrderTemplateModel
                {
                    Id = quickOrder.Id,
                    Name = quickOrder.Name,
                    CustomerId = quickOrder.CustomerId,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(quickOrder.CreatedOnUtc)
                };

                if (searchModel.LoadTotalPrice)
                {
                    (var totalCount, var totalPriceValue) = await QuickOrderTemplateCountAndTotalPriceAsync(quickOrder, searchModel.LoadTotalPrice);

                    quickOrderTemplate.TotalValue = totalPriceValue;
                    quickOrderTemplate.TotalValueText = await _priceFormatter.FormatPriceAsync(quickOrderTemplate.TotalValue, true,
                    customerCurrencyCode, (await _workContext.GetWorkingLanguageAsync()).Id, true);
                    quickOrderTemplate.TotalOrderItems = totalCount.ToString();
                }
                else
                {
                    quickOrderTemplate.TotalValue = quickOrder.TotalPriceOfItems;
                    quickOrderTemplate.TotalValueText = "loading...";
                    quickOrderTemplate.TotalOrderItems = "loading...";
                }

                if (quickOrder.LastOrderDate.HasValue)
                    quickOrderTemplate.LastOrderDate = await _dateTimeHelper.ConvertToUserTimeAsync(quickOrder.LastOrderDate ?? DateTime.UtcNow);
                return quickOrderTemplate;
            });
        });

        return model;
    }

    public async Task<QuickOrderTemplateModel> PrepareQuickOrderTemplateModelAsync(QuickOrderTemplateModel model, QuickOrderTemplate quickOrderTemplate)
    {
        if (quickOrderTemplate != null)
        {
            model = model ?? new QuickOrderTemplateModel();
            model.Id = quickOrderTemplate.Id;
            model.Name = quickOrderTemplate.Name;
            model.CustomerId = quickOrderTemplate.CustomerId;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(quickOrderTemplate.CreatedOnUtc);
            if (quickOrderTemplate.EditedOnUtc.HasValue)
                model.EditedOn = await _dateTimeHelper.ConvertToUserTimeAsync(quickOrderTemplate.EditedOnUtc ?? DateTime.UtcNow);
            if (quickOrderTemplate.LastOrderDate.HasValue)
                model.LastOrderDate = await _dateTimeHelper.ConvertToUserTimeAsync(quickOrderTemplate.LastOrderDate ?? DateTime.UtcNow);
            model.TotalOrderItems = quickOrderTemplate.QuickOrderItems.Count.ToString();
        }

        // prepare nested search model
        model.QuickOrderItemSearchModel = new QuickOrderItemSearchModel();
        model.QuickOrderItemSearchModel.SetGridPageSize();
        model.QuickOrderItemSearchModel.QuickOrderTemplateId = model.Id;
        // set always validate
        model.QuickOrderItemSearchModel.Validate = true;

        model.AddQuickOrderItem = new QuickOrderItemModel();

        return model;
    }

    public async Task<QuickOrderTemplateSearchModel> PrepareQuickOrderTemplateSearchModelAsync(QuickOrderTemplateSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    #endregion
}