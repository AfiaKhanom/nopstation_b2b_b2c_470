using Nop.Core;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;

public interface IQuartzJobDetailService
{
    Task<QRTZ_JOB_DETAILS> GetQuartzJobDetails(string jobIdentity);

    Task<IPagedList<QRTZ_JOB_DETAILS>> GetPagedListAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task UpdateQuartzJobDetailsAsync(QRTZ_JOB_DETAILS quartzJobDetail);
}
