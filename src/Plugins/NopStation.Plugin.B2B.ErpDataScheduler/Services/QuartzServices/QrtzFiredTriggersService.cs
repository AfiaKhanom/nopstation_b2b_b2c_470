using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;
public class QrtzFiredTriggersService : IQrtzFiredTriggersService
{
    #region Fields

    private readonly IRepository<QRTZ_FIRED_TRIGGERS> _qrtzFiredTriggersRepository;

    #endregion

    #region Ctor

    public QrtzFiredTriggersService(IRepository<QRTZ_FIRED_TRIGGERS> qrtzFiredTriggersRepository)
    {
        _qrtzFiredTriggersRepository = qrtzFiredTriggersRepository;
    }

    #endregion

    #region Methods

    public async Task<bool> CheckJobIsRunningAsync(string jodIdentityKey)
    {
        var query = _qrtzFiredTriggersRepository.Table
            .Where(x => x.JOB_NAME == jodIdentityKey);

        return await query.AnyAsync();
    }

    #endregion
}
