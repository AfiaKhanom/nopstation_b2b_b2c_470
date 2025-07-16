namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;
public interface IQrtzFiredTriggersService
{
    Task<bool> CheckJobIsRunningAsync(string jodIdentityKey);
}
