using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpGroupPriceModelFactory : IErpGroupPriceModelFactory
{
    #region Fields

    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpGroupPriceCodeService _erpPriceGroupCodeService;
    private readonly IErpGroupPriceCodeModelFactory _erpPriceGroupCodeModelFactory;

    #endregion

    #region Ctor

    public ErpGroupPriceModelFactory(
        IErpGroupPriceService erpGroupPriceService,
        IErpGroupPriceCodeService erpPriceGroupCodeService,
        IErpGroupPriceCodeModelFactory erpPriceGroupCodeModelFactory)
    {
        _erpGroupPriceService = erpGroupPriceService;
        _erpPriceGroupCodeService = erpPriceGroupCodeService;
        _erpPriceGroupCodeModelFactory = erpPriceGroupCodeModelFactory;
    }

    #endregion

    #region Methods

    public async Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductPricingSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ProductId = productId;
        searchModel.SetGridPageSize();
        await PrepareErpProductPricingModel(searchModel.AddErpPriceGroupProductPricing, null);
        return searchModel;
    }

    public async Task<ErpPriceGroupProductPricingListModel> PrepareErpProductPricingListModel(ErpPriceGroupProductPricingSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpProductPricings = await _erpGroupPriceService.GetAllErpGroupPricesAsync(
            pageIndex: searchModel.Page - 1, 
            pageSize: searchModel.PageSize, 
            showHidden: false, 
            getOnlyTotalCount: false, 
            overridePublished: false, 
            productId: searchModel.ProductId, 
            groupCode: searchModel.SearchErpPriceGroupCode);

        var erpGroupPriceCodes = await _erpPriceGroupCodeService.GetAllErpGroupPriceCodesAsync();

        var model = new ErpPriceGroupProductPricingListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
        {
            return erpProductPricings.Select(productPricing =>
            {
                var erpGroupPriceCodeCheck = erpGroupPriceCodes.FirstOrDefault(f => f.Id == productPricing.ErpNopGroupPriceCodeId);

                if (erpGroupPriceCodeCheck is null)
                {
                    return null;
                }
                var pricingModel = new ErpPriceGroupProductPricingModel
                {
                    Id = productPricing.Id,
                    ProductId = productPricing.NopProductId,
                    ErpGroupPriceCodeId = productPricing.ErpNopGroupPriceCodeId,
                    ErpGroupPriceCode = erpGroupPriceCodeCheck.Code,
                    Price = productPricing.Price
                };

                return pricingModel;
            }).Where(x => x != null);
        });
        return model;
    }

    public async Task<ErpPriceGroupProductPricingModel> PrepareErpProductPricingModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing)
    {
        if (erpProductPricing != null)
        {
            var erpGroupPriceCode = await _erpPriceGroupCodeService.GetErpGroupPriceCodeByIdAsync(erpProductPricing.ErpNopGroupPriceCodeId);

            model ??= new ErpPriceGroupProductPricingModel();
            model.Id = erpProductPricing.Id;
            model.ProductId = erpProductPricing.NopProductId;
            model.ErpGroupPriceCodeId = erpProductPricing.ErpNopGroupPriceCodeId;
            model.ErpGroupPriceCode = erpGroupPriceCode.Code;
            model.Price = erpProductPricing.Price;
        }

        _erpPriceGroupCodeModelFactory.PrepareErpGroupPriceCodes(model.AvailableErpPriceGroupCodes, false);
        return model;
    }

    #endregion
}