using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpGroupPriceService
    {
        Task InsertErpGroupPriceAsync(ErpGroupPrice erpGroupPrice);

        Task UpdateErpGroupPriceAsync(ErpGroupPrice erpGroupPrice);

        Task DeleteErpGroupPriceByIdAsync(int id);

        Task<ErpGroupPrice> GetErpGroupPriceByIdAsync(int id);

        Task<ErpGroupPrice> GetErpGroupPriceByIdWithActiveAsync(int id);

        Task<IPagedList<ErpGroupPrice>> GetAllErpGroupPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int productId = 0, string groupCode = null);

        Task<IList<ErpGroupPrice>> GetErpGroupPriceByProductIdAsync(int productId);

        Task<ErpGroupPrice> GetB2BPriceGroupProductPricingByErpPriceGroupCodeAndProductId(int priceGroupCodeId, int productId);

        Task<bool> CheckAnyPriceGroupProductPricingExistWithProductIdAndPriceGroupCodeId(int prouctId, int priceGroupCodeId);
        Task InActiveAllOldGroupPrice(DateTime syncStartTime);
    }
}

