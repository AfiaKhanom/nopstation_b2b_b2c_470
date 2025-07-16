using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;

public class QuartzJobDetailService : IQuartzJobDetailService
{
    #region Fields

    private readonly IRepository<QRTZ_JOB_DETAILS> _quartzJobDetailRepository;

    #endregion

    #region Ctor

    public QuartzJobDetailService(IRepository<QRTZ_JOB_DETAILS> quartzJobDetailRepository)
    {
        _quartzJobDetailRepository = quartzJobDetailRepository;
    }

    #endregion

    #region Methods

    public async Task<QRTZ_JOB_DETAILS> GetQuartzJobDetails(string jobIdentity)
    {
        ArgumentException.ThrowIfNullOrEmpty(jobIdentity, nameof(jobIdentity));

        return await _quartzJobDetailRepository.Table.FirstOrDefaultAsync(x => x.JOB_NAME == jobIdentity);
    }

    public async Task<IPagedList<QRTZ_JOB_DETAILS>> GetPagedListAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = await _quartzJobDetailRepository.GetAllPagedAsync(query =>
        {
            return query;
        }, pageIndex, pageSize);

        return query;
    }

    public async Task UpdateQuartzJobDetailsAsync(QRTZ_JOB_DETAILS quartzJobDetail)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(quartzJobDetail));

        await _quartzJobDetailRepository.UpdateAsync(quartzJobDetail);
    }

    #endregion
}
