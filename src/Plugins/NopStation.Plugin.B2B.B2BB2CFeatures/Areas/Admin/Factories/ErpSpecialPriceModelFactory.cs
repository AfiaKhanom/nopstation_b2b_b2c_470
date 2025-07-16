using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Helpers;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpSpecialPriceModelFactory : IErpSpecialPriceModelFactory
{
    #region Fields

    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IDateTimeHelper _dateTimeHelper;

    #endregion

    #region Ctor

    public ErpSpecialPriceModelFactory(
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSpecialPriceService erpSpecialPriceService,
        IDateTimeHelper dateTimeHelper)
    {
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _dateTimeHelper = dateTimeHelper;
    }

    #endregion

    #region Methods

    public async Task<ErpSpecialPriceSearchModel> PrepareErpProductSpecialPriceSearchModel(ErpSpecialPriceSearchModel searchModel, int productId)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ProductId = productId;
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<ErpSpecialPriceListModel> PrepareErpProductSpecialPriceListModel(ErpSpecialPriceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpProductPricings = await _erpSpecialPriceService.GetAllErpSpecialPricesAsync(
            pageIndex: searchModel.Page - 1, 
            pageSize: searchModel.PageSize, 
            getOnlyTotalCount: false, 
            productId: searchModel.ProductId, 
            accountId: searchModel.SearchErpAccountId,
            onlyIncludeActiveErpAccountsMappedPrices: true);

        var erpAccounts = await _erpAccountService.GetErpAccountListAsync();
        var erpSalesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
        var model = new ErpSpecialPriceListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
        {
            return erpProductPricings.Select(productPricing =>
            {
                var pricingModel = new ErpSpecialPriceModel
                {
                    Id = productPricing.Id,
                    ProductId = productPricing.NopProductId,
                    Price = productPricing.Price,
                    PricingNote = productPricing.PricingNote,
                    DiscountPerc = productPricing.DiscountPerc,
                    PercentageOfAllocatedStock = productPricing.PercentageOfAllocatedStock
                };

                var erpAccount = erpAccounts.FirstOrDefault(w => w.Id == productPricing.ErpAccountId);
                if (erpAccount != null)
                {
                    var erpAccountSalesOrg = erpSalesOrgs.FirstOrDefault(w => w.Id == erpAccount.ErpSalesOrgId);

                    if (erpAccountSalesOrg != null)
                    {
                        pricingModel.ErpAccountId = productPricing.ErpAccountId;
                        pricingModel.ErpAccountNumber = erpAccount.AccountNumber;
                        pricingModel.ErpAccountSalesOrgId = erpAccount.ErpSalesOrgId;
                        pricingModel.ErpAccountSalesOrgName = erpAccountSalesOrg.Name;
                    }
                }

                return pricingModel;
            }).Where(x => x.ErpAccountId > 0 && x.ErpAccountSalesOrgId > 0);
        });
        return model;
    }

    public async Task<ErpSpecialPriceModel> PrepareErpProductSpecialPriceModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing)
    {
        if (erpProductPricing != null)
        {
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpProductPricing.ErpAccountId);

            model = model ?? new ErpSpecialPriceModel();
            model.Id = erpProductPricing.Id;
            model.ProductId = erpProductPricing.NopProductId;
            model.Price = erpProductPricing.Price;
            model.DiscountPerc = erpProductPricing.DiscountPerc;
            model.PricingNote = erpProductPricing.PricingNote;
            model.PercentageOfAllocatedStock = erpProductPricing.PercentageOfAllocatedStock;
            if (erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.HasValue)
                model.PercentageOfAllocatedStockResetTimeUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.Value, DateTimeKind.Utc);

            if (erpAccount != null)
            {
                var erpAccountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

                if (erpAccountSalesOrg != null)
                {
                    model.ErpAccountId = erpProductPricing.ErpAccountId;
                    model.ErpAccountNumber = erpAccount.AccountNumber;
                    model.ErpAccountSalesOrgId = erpAccount.ErpSalesOrgId;
                    model.ErpAccountSalesOrgName = erpAccountSalesOrg.Name;
                }
            }
        }

        return model;
    }

    #endregion
}
