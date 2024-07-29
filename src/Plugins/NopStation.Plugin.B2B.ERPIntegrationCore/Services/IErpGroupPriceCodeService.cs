using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpGroupPriceCodeService
    {
        Task InsertErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode);

        Task UpdateErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode);

        Task DeleteErpGroupPriceCodeByIdAsync(int id);

        Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdAsync(int id);

        Task<ErpGroupPriceCode> GetErpGroupPriceCodeByCodedAsync(string code);

        Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdWithActiveAsync(int id);

        Task<IList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesAsync(bool showHidden = false);

        Task<IPagedList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesPagedAsync(string groupPriceCode, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue, 
            bool? showHidden = null,
            bool getOnlyTotalCount = false);

        Task<bool> CheckAnyErpGroupPriceCodeExistByCode(string groupPriceCode);

    }
}

