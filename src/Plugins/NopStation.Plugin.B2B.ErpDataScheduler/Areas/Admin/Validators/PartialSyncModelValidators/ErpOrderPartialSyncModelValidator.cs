using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpOrderPartialSyncModelValidator : BaseNopValidator<ErpOrderPartialSyncModel>
{
    #region Ctor

    public ErpOrderPartialSyncModelValidator()
    {
        RuleFor(model => model.ErpAccountNumber).NotEmpty();
        RuleFor(model => model.OrderNumber).NotEmpty();
    }

    #endregion
}
