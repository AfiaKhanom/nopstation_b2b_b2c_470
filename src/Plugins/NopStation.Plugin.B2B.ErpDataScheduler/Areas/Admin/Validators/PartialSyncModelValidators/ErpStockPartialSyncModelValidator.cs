using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpStockPartialSyncModelValidator : BaseNopValidator<ErpStockPartialSyncModel>
{
    #region Ctor

    public ErpStockPartialSyncModelValidator()
    {
        RuleFor(model => model.StockCode).NotEmpty();
    }

    #endregion
}
