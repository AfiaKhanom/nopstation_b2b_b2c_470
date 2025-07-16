using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpShipToAddressValidator : BaseNopValidator<ErpShipToAddress>
{
    #region Ctor

    public ErpShipToAddressValidator()
    {
        // Mandatory string fields
        RuleFor(x => x.ShipToCode)
            .NotEmpty();

        // Mandatory integer field
        RuleFor(x => x.AddressId)
            .GreaterThan(0);
    }

    #endregion
}
