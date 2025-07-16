using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services;

public class ProcessFailedErpOrdersTask : IScheduleTask
{
    #region Fields

    private readonly IOverriddenOrderProcessingService _overridenOrderProcessingService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public ProcessFailedErpOrdersTask(IOverriddenOrderProcessingService overridenOrderProcessingService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _overridenOrderProcessingService = overridenOrderProcessingService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Methods

    public async Task ExecuteAsync()
    {
        var retryErpOrders = await _erpOrderAdditionalDataService.GetAllFailedOrProcessingOrQueuedErpOrders(_b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries);

        foreach (var erpOrders in retryErpOrders)
            await _overridenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrders);
    }

    #endregion
}
