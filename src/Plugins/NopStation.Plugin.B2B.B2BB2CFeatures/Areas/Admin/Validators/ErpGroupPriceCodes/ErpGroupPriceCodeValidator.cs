using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpGroupPriceCodes;

public partial class ErpGroupPriceValidator : BaseNopValidator<ErpGroupPriceCodeModel>
{
    public ErpGroupPriceValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.GroupPriceCode)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.RequiredErrMsg.Code"));

        SetDatabaseValidationRules<ErpGroupPriceCode>();
    }
}