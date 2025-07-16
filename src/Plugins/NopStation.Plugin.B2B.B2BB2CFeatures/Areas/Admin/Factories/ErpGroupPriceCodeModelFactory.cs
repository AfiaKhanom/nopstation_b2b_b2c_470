using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpGroupPriceCodeModelFactory : IErpGroupPriceCodeModelFactory
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;

    #endregion

    #region Ctor

    public ErpGroupPriceCodeModelFactory(ILocalizationService localizationService,
        IErpGroupPriceCodeService erpGroupPriceCodeService)
    {
        _localizationService = localizationService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
    }

    #endregion

    #region Method

    public async Task<ErpGroupPriceCodeListModel> PrepareErpGroupPriceCodeListModelAsync(ErpGroupPriceCodeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpGroupPriceCodes = await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesPagedAsync(
            searchModel.SearchGroupPriceCode,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2));

        var model = new ErpGroupPriceCodeListModel().PrepareToGrid(searchModel, erpGroupPriceCodes, () =>
        {
            return erpGroupPriceCodes.Select(priceGroup =>
            {
                if (string.IsNullOrWhiteSpace(priceGroup.Code))
                    return null;

                var priceGroupModel = new ErpGroupPriceCodeModel
                {
                    Id = priceGroup.Id,
                    GroupPriceCode = priceGroup.Code,
                    LastPriceUpdatedOnUTC = priceGroup.LastUpdateTime,
                    IsActive = priceGroup.IsActive,
                };

                return priceGroupModel;
            }).Where(model => model != null);
        });

        return model;
    }

    public async Task<ErpGroupPriceCodeModel> PrepareErpGroupPriceCodeModelAsync(ErpGroupPriceCodeModel model, ErpGroupPriceCode erpGroupPriceCode)
    {
        if (erpGroupPriceCode != null)
        {
            model = model ?? new ErpGroupPriceCodeModel();
            model.Id = erpGroupPriceCode.Id;
            model.GroupPriceCode = erpGroupPriceCode.Code;
            model.IsActive = erpGroupPriceCode.IsActive;
            model.LastPriceUpdatedOnUTC = erpGroupPriceCode.LastUpdateTime;
        }

        return model;
    }

    public async Task<ErpGroupPriceCodeSearchModel> PrepareErpGroupPriceCodeSearchModelAsync(ErpGroupPriceCodeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowAll"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyActive"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyInactive"),
        });

        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task PrepareErpGroupPriceCodes(IList<SelectListItem> items, bool withSpecialDefaultItem = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        var availablePriceGroup = await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync();
        foreach (var priceGroup in availablePriceGroup)
        {
            items.Add(new SelectListItem { Value = priceGroup.Id.ToString(), Text = priceGroup.Code });
        }

        if (withSpecialDefaultItem)
            await PrepareDefaultItem(items);
    }

    protected async Task PrepareDefaultItem(IList<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        const string value = "0";
        var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.All");
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
    }

    #endregion
}
