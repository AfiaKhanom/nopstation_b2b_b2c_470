using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpSpecialPrices;

public partial class ErpSpecialPriceValidator : BaseNopValidator<ErpSpecialPriceModel>
{
    public ErpSpecialPriceValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.RequiredErrMsg.ErpAccountId"));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.RequiredErrMsg.Price"));

        SetDatabaseValidationRules<ErpSpecialPrice>();
    }
}