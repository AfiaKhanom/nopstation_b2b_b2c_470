using FluentValidation;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators.NopDomain;

public class ManufacturerValidator : BaseNopValidator<Manufacturer>
{
    #region Ctor

    public ManufacturerValidator()
    {
        RuleFor(manufacturer => manufacturer.Name)
            .NotEmpty()
            .MaximumLength(400);
    }

    #endregion
}
