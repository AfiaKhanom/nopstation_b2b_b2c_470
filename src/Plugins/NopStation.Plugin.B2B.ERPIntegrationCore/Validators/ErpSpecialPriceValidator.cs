using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpSpecialPriceValidator : BaseNopValidator<ErpSpecialPrice>
{
    #region Ctor

    public ErpSpecialPriceValidator()
    {
        // Mandatory fields
        RuleFor(x => x.NopProductId)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ListPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0);
    }

    #endregion
}
