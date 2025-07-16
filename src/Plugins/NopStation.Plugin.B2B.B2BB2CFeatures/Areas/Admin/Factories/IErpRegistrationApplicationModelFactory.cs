using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpRegistrationApplicationModelFactory
{
    Task<ErpRegistrationApplicationSearchModel> PrepareErpRegistrationApplicationSearchModelAsync(ErpRegistrationApplicationSearchModel searchModel);
    Task<ErpRegistrationApplicationListModel> PrepareErpRegistrationApplicationListModelAsync(ErpRegistrationApplicationSearchModel searchModel);
    Task<ApplicationFormModel> PrepareErpRegistrationApplicationModelAsync(ApplicationFormModel model, ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm);
}