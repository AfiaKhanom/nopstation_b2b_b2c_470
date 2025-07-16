using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpInvoiceValidator : BaseNopValidator<ErpInvoice>
{
    #region Ctor

    public ErpInvoiceValidator()
    {
        // Mandatory string fields
        RuleFor(x => x.ErpDocumentNumber)
            .NotEmpty();

        // Mandatory integer fields
        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0);
    }

    #endregion
}