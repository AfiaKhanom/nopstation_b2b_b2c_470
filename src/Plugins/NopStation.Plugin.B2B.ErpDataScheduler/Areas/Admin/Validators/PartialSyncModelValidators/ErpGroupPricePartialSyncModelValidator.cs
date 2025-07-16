using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators.PartialSyncModelValidators;

public class ErpGroupPricePartialSyncModelValidator : BaseNopValidator<ErpGroupPricePartialSyncModel>
{
    #region Ctor

    public ErpGroupPricePartialSyncModelValidator()
    {
        RuleFor(model => model.PriceCode).NotEmpty();
        RuleFor(model => model.StockCode).NotEmpty();
    }

    #endregion
}
