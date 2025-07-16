using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpInvoicePartialSyncModelValidator : BaseNopValidator<ErpInvoicePartialSyncModel>
{
    #region Ctor

    public ErpInvoicePartialSyncModelValidator()
    {
        RuleFor(model => model.ErpAccountNumber).NotEmpty();
    }

    #endregion
}

