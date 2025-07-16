using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpProductSingleSyncModelModelValidator : BaseNopValidator<ErpProductPartialSyncModel>
{
    #region Ctor

    public ErpProductSingleSyncModelModelValidator()
    {
        RuleFor(model => model.StockCode).NotEmpty();
    }

    #endregion
}
