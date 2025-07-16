using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpShiptoAddressErpAccountMapValidator : BaseNopValidator<ErpShiptoAddressErpAccountMap>
{
    #region Ctor

    public ErpShiptoAddressErpAccountMapValidator()
    {
        // Mandatory integer fields
        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0);

        RuleFor(x => x.ErpShiptoAddressId)
            .GreaterThan(0);
    }

    #endregion
}
