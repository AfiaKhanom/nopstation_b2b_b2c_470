using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpGroupPriceCodeModelFactory
{
    Task<ErpGroupPriceCodeSearchModel> PrepareErpGroupPriceCodeSearchModelAsync(ErpGroupPriceCodeSearchModel searchModel);
    Task<ErpGroupPriceCodeListModel> PrepareErpGroupPriceCodeListModelAsync(ErpGroupPriceCodeSearchModel searchModel);
    Task<ErpGroupPriceCodeModel> PrepareErpGroupPriceCodeModelAsync(ErpGroupPriceCodeModel model, ErpGroupPriceCode erpGroupPriceCode);
    Task PrepareErpGroupPriceCodes(IList<SelectListItem> items, bool withSpecialDefaultItem = false);
}