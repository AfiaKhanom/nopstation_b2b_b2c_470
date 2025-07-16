using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpGroupPriceValidator : BaseNopValidator<ErpGroupPrice>
{
    #region Ctor

    public ErpGroupPriceValidator()
    {
        RuleFor(x => x.ErpNopGroupPriceCodeId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.NopProductId)
            .GreaterThanOrEqualTo(0);
    }

    #endregion
}
