using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators;

public class ErpSalesRepValidator : BaseNopValidator<ErpSalesRepModel>
{
    public ErpSalesRepValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.NopCustomerId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.SalesRepresentatives.RequiredErrMsg.NopCustomerId"));

        RuleFor(x => x.SalesRepTypeId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.SalesRepresentatives.RequiredErrMsg.SalesRepTypeId"));

        RuleFor(x => x.SalesOrgIds)
           .NotEmpty()
           .When(x => x.SalesRepTypeId == (int)SalesRepType.BySalesOrganisation)
           .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.SalesRepresentatives.RequiredErrMsg.SalesOrgIds"));

        SetDatabaseValidationRules<ErpSalesRep>();
    }
}