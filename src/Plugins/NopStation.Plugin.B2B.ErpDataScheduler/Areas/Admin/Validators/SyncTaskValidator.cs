using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Validators;

public partial class SyncTaskValidator : BaseNopValidator<SyncTaskModel>
{
    public SyncTaskValidator(ILocalizationService localizationService)
    {
        SetDatabaseValidationRules<SyncTask>();
    }
}