using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpGroupPrices;

public partial class ErpGroupPriceValidator : BaseNopValidator<ErpPriceGroupProductPricingModel>
{
    public ErpGroupPriceValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.ErpGroupPriceCodeId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.RequiredErrMsg.Code"));

        SetDatabaseValidationRules<ErpGroupPriceCode>();
    }
}