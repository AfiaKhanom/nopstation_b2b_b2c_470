using FluentValidation;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators.NopDomain;

public class CategoryValidator : BaseNopValidator<Category>
{
    #region Ctor

    public CategoryValidator()
    {
        RuleFor(category => category.Name)
            .NotEmpty()
            .MaximumLength(400);
    }

    #endregion
}
