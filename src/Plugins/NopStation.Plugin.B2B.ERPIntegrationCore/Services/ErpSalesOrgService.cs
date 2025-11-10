using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSalesOrgService : IErpSalesOrgService
{
    #region Fields

    private readonly IRepository<ErpSalesOrg> _erpErpSalesOrgRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;
    private readonly IRepository<ErpWarehouseAdditionalData> _erpWarehouseAdditionalDataRepository;
    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;

    #endregion Fields

    #region Ctor

    public ErpSalesOrgService(IRepository<ErpSalesOrg> erpSalesOrgRepository,
        IRepository<ErpAccount> erpAccountRepository,
        IRepository<ErpWarehouseAdditionalData> erpWarehouseAdditionalDataRepository,
        IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository)
    {
        _erpErpSalesOrgRepository = erpSalesOrgRepository;
        _erpAccountRepository = erpAccountRepository;
        _erpWarehouseAdditionalDataRepository = erpWarehouseAdditionalDataRepository;
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
    }

    #endregion Ctor

    #region Methods

    #region Insert/Update

    public async Task InsertErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        await _erpErpSalesOrgRepository.InsertAsync(erpSalesOrg);
    }

    public async Task UpdateErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
    }

    #endregion Insert/Update

    #region Delete

    private async Task DeleteErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpSalesOrg.IsDeleted = true;
        await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
    }

    public async Task DeleteErpSalesOrgByIdAsync(int id)
    {
        var erpSalesOrg = await GetErpSalesOrgByIdAsync(id);
        if (erpSalesOrg != null)
        {
            await DeleteErpSalesOrgAsync(erpSalesOrg);
        }
    }

    #endregion Delete

    #region Read

    public async Task<ErpSalesOrg> GetErpSalesOrgByIdAsync(int id, bool filterOutDeleted = true)
    {
        if (id == 0)
            return null;

        var query = _erpErpSalesOrgRepository.Table.Where(x => x.Id == id);

        if (filterOutDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ErpSalesOrg> GetErpSalesOrgByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesOrg = await _erpErpSalesOrgRepository.GetByIdAsync(id, cache => default);

        if (erpSalesOrg == null || !erpSalesOrg.IsActive || erpSalesOrg.IsDeleted)
            return null;

        return erpSalesOrg;
    }

    public async Task<IPagedList<ErpSalesOrg>> GetAllErpSalesOrgAsync(int pageIndex = 0, int pageSize = int.MaxValue, string name = null, string email = null, string code = null, bool? showHidden = null, bool getOnlyTotalCount = false)
    {
        var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllPagedAsync(query =>
        {
            // showHidden is null for getting all, true for only actives and false for only inactives
            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(c => c.Code.Contains(code));

            query = query.Where(egp => !egp.IsDeleted);
            query = query.OrderBy(egp => egp.Id);
            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpSalesOrgs;
    }

    public async Task<IList<ErpSalesOrg>> GetAllErpSalesOrgsAsync()
    {
        var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllAsync(query =>
        {
            query = query.Where(v => v.IsActive && !v.IsDeleted);
            query = query.OrderBy(ea => ea.Id);
            return query;
        });

        return erpSalesOrgs;
    }

    public async Task<bool> IsMappedWithAnyERPAccountAsync(int erpSalesOrgId)
    {
        var isMapped = await _erpAccountRepository.Table.AnyAsync(ea => !ea.IsDeleted && ea.ErpSalesOrgId == erpSalesOrgId);
        return isMapped;
    }

    public async Task<ErpSalesOrg> GetSalesOrgByCodeAsync(string salesorgCode)
    {
        if (string.IsNullOrWhiteSpace(salesorgCode))
            return null;

        var erpSalesOrg = await _erpErpSalesOrgRepository.Table.FirstOrDefaultAsync(x => x.Code.Trim().ToLower() == salesorgCode.Trim().ToLower());

        if (erpSalesOrg == null || erpSalesOrg.IsDeleted)
            return null;

        return erpSalesOrg;
    }

    /// <summary>
    /// This method retrieves the ErpSalesOrg associated with a given warehouseCode
    /// </summary>
    /// <param name="warehouseCode"></param>
    /// <returns></returns>
    public async Task<ErpSalesOrg> GetSalesOrgByWarehouseCodeAsync(string warehouseCode)
    {
        if (string.IsNullOrEmpty(warehouseCode))
            return null;
        warehouseCode = warehouseCode.Trim();

        return await (from warehouse in _erpWarehouseAdditionalDataRepository.Table
                       where warehouse.IsActive && !warehouse.IsDeleted && warehouse.Code.Trim() == warehouseCode
                       join map in _erpWarehouseSalesOrgMapRepository.Table
                           on warehouse.Id equals map.ErpWarehouseId
                       join salesOrg in _erpErpSalesOrgRepository.Table
                           on map.ErpSalesOrgId equals salesOrg.Id
                       select salesOrg).FirstOrDefaultAsync();
    }

    public async Task<IList<ErpSalesOrg>> GetErpSalesOrgsAsync(bool isActive = true, bool filterOutDeleted = true)
    {
        var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllAsync(query =>
        {
            if (isActive)
                query = query.Where(v => v.IsActive);
            if (filterOutDeleted)
                query = query.Where(v => !v.IsDeleted);
            query = query.OrderBy(ea => ea.Id);
            return query;
        });

        return erpSalesOrgs;
    }

    #endregion

    #endregion Methods
}