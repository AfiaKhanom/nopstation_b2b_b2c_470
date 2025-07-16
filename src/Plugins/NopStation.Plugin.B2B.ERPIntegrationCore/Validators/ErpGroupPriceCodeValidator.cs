using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpGroupPriceCodeValidator : BaseNopValidator<ErpGroupPriceCode>
{
    #region Ctor

    public ErpGroupPriceCodeValidator()
    {
        // Validate Code
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        // Validate LastUpdateTime
        RuleFor(x => x.LastUpdateTime)
            .NotEmpty();
    }

    #endregion
}
