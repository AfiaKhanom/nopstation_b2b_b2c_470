using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpGroupPriceCodeService : IErpGroupPriceCodeService
{
    #region Fields

    private readonly IRepository<ErpGroupPriceCode> _erpGroupPriceCodeRepository;

    #endregion

    #region Ctor

    public ErpGroupPriceCodeService(IRepository<ErpGroupPriceCode> erpGroupPriceCodeRepository)
    {
        _erpGroupPriceCodeRepository = erpGroupPriceCodeRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
    {
        await _erpGroupPriceCodeRepository.InsertAsync(erpGroupPriceCode);
    }

    public async Task InsertErpGroupPriceCodesAsync(IList<ErpGroupPriceCode> erpGroupPriceCodes)
    {
        await _erpGroupPriceCodeRepository.InsertAsync(erpGroupPriceCodes);
    }

    public async Task UpdateErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
    {
        await _erpGroupPriceCodeRepository.UpdateAsync(erpGroupPriceCode);
    }

    public async Task UpdateErpGroupPriceCodesAsync(IList<ErpGroupPriceCode> erpGroupPriceCodes)
    {
        await _erpGroupPriceCodeRepository.UpdateAsync(erpGroupPriceCodes);
    }

    #endregion

    #region Delete

    private async Task DeleteErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpGroupPriceCode.IsDeleted = true;
        await _erpGroupPriceCodeRepository.UpdateAsync(erpGroupPriceCode);
    }

    public async Task DeleteErpGroupPriceCodeByIdAsync(int id)
    {
        var erpGroupPriceCode = await GetErpGroupPriceCodeByIdAsync(id);
        if (erpGroupPriceCode != null)
        {
            await DeleteErpGroupPriceCodeAsync(erpGroupPriceCode);
        }
    }

    #endregion

    #region Read

    public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpGroupPriceCode = await _erpGroupPriceCodeRepository.GetByIdAsync(id, cache => default);

        if (erpGroupPriceCode == null || erpGroupPriceCode.IsDeleted)
            return null;

        return erpGroupPriceCode;
    }

    public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpGroupPriceCode = await _erpGroupPriceCodeRepository.GetByIdAsync(id, cache => default);

        if (erpGroupPriceCode == null || !erpGroupPriceCode.IsActive || erpGroupPriceCode.IsDeleted)
            return null;

        return erpGroupPriceCode;
    }

    public async Task<IList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesAsync(bool showHidden = false)
    {
        var erpGroupPriceCodes = await _erpGroupPriceCodeRepository.GetAllAsync(query =>
        {
            query = query.Where(egpc => !string.IsNullOrWhiteSpace(egpc.Code));

            if (!showHidden)
                query = query.Where(egpc => egpc.IsActive);

            query = query.Where(egpc => !egpc.IsDeleted);
            query = query.OrderBy(egpc => egpc.Id);
            return query;

        });

        return erpGroupPriceCodes;
    }

    public async Task<IPagedList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesPagedAsync(string groupPriceCode, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue,
        bool? showHidden = null,
        bool getOnlyTotalCount = false)
    {
        var groupPriceCodes = await _erpGroupPriceCodeRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(v => !v.IsDeleted);

            // showHidden is null for getting all, true for only actives and false for only inactives
            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            if (!string.IsNullOrEmpty(groupPriceCode))
                query = query.Where(b => b.Code.Contains(groupPriceCode));

            query = query.OrderByDescending(ea => ea.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return groupPriceCodes;
    }

    public async Task<bool> CheckAnyErpGroupPriceCodeExistByCode(string groupPriceCode)
    {
        if (string.IsNullOrEmpty(groupPriceCode))
            return false;

        var erpGroupPriceCodeExists = await _erpGroupPriceCodeRepository.GetAllAsync(query =>
        {
            query = query.Where(egpc => egpc.Code.Equals(groupPriceCode) && !egpc.IsDeleted);
            return query;
        });

        return erpGroupPriceCodeExists.Any();
    }

    public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByCodeAsync(string groupPriceCode)
    {
        if (string.IsNullOrEmpty(groupPriceCode))
            return new ErpGroupPriceCode();

        return await _erpGroupPriceCodeRepository.Table
            .FirstOrDefaultAsync(egpc => egpc.Code.Equals(groupPriceCode) && !egpc.IsDeleted);
    }

    #endregion

    #endregion
}
