using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpOrderItemAdditionalDataService : IErpOrderItemAdditionalDataService
{
    #region Fields

    private readonly IRepository<ErpOrderItemAdditionalData> _erpOrderItemAdditionalDataRepository;

    #endregion

    #region Ctor

    public ErpOrderItemAdditionalDataService(IRepository<ErpOrderItemAdditionalData> erpOrderItemAdditionalDataRepository)
    {
        _erpOrderItemAdditionalDataRepository= erpOrderItemAdditionalDataRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.InsertAsync(erpOrderItemAdditionalData);
    }

    public async Task UpdateErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.UpdateAsync(erpOrderItemAdditionalData);
    }

    #endregion

    #region Delete

    private async Task DeleteErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.DeleteAsync(erpOrderItemAdditionalData);
    }

    public async Task DeleteErpOrderItemAdditionalDataByIdAsync(int id)
    {
        var erpOrderItemAdditionalData = await GetErpOrderItemAdditionalDataByIdAsync(id);
        if (erpOrderItemAdditionalData != null)
        {
            await DeleteErpOrderItemAdditionalDataAsync(erpOrderItemAdditionalData);
        }
    }

    #endregion

    #region Read

    public async Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpOrderItemAdditionalDataRepository.GetByIdAsync(id, cache => default);
    }
    
    public async Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(int orderItemId)
    {
        if (orderItemId == 0)
            return null;

        return await (from eoiad in _erpOrderItemAdditionalDataRepository.Table
                      where eoiad.NopOrderItemId == orderItemId
                      select eoiad).FirstOrDefaultAsync();
    }

    public async Task<IPagedList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
    {
        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataRepository.GetAllPagedAsync(query =>
        {
            query = query.OrderBy(ei => ei.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpOrderItemAdditionalData;
    }

    public async Task<IList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(int orderId)
    {
        if (orderId == 0)
            return null;

        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataRepository.GetAllAsync(query =>
        {
            query = query.Where(ei => ei.ErpOrderId == orderId);
            query = query.OrderBy(ei => ei.Id);
            return query;

        });

        return erpOrderItemAdditionalData;
    }

    #endregion

    #endregion
}

