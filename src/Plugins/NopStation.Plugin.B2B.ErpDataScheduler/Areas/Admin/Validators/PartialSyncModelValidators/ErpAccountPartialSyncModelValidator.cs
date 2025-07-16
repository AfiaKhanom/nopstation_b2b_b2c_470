using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpAccountPartialSyncModelValidator : BaseNopValidator<ErpAccountPartialSyncModel>
{
    #region Ctor

    public ErpAccountPartialSyncModelValidator()
    {
        RuleFor(model => model.ErpAccountNumber).NotEmpty();
    }

    #endregion
}
