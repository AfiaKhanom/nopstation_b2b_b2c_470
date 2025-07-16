using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderTemplates;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;

public interface IQuickOrderTemplateModelFactory
{
    Task<QuickOrderTemplateListModel> PrepareQuickOrderTemplateListModelAsync(QuickOrderTemplateSearchModel searchModel);

    Task<QuickOrderTemplateModel> PrepareQuickOrderTemplateModelAsync(QuickOrderTemplateModel model, QuickOrderTemplate quickOrderTemplate);

    Task<QuickOrderTemplateSearchModel> PrepareQuickOrderTemplateSearchModelAsync(QuickOrderTemplateSearchModel searchModel);
}