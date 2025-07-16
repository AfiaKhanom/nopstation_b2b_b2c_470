using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpNopUserValidator : BaseNopValidator<ErpNopUser>
{
    #region Ctor

    public ErpNopUserValidator()
    {
        // Mandatory integer field
        RuleFor(x => x.NopCustomerId)
            .GreaterThan(0);

        // Mandatory integer field with a foreign key
        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0);

        RuleFor(x => x.ErpShipToAddressId)
            .GreaterThan(0);

        // Mandatory integer field
        RuleFor(x => x.ErpUserTypeId)
            .GreaterThan(0);
    }

    #endregion
}
