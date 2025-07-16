using FluentValidation;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators.NopDomain;

public class ProductValidator : BaseNopValidator<Product>
{
    #region Ctor

    public ProductValidator()
    {
        RuleFor(product => product.Sku)
            .NotEmpty()
            .MaximumLength(400);
    }

    #endregion
}
