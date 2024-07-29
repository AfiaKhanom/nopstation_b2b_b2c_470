using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using Nop.Services.Catalog;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpWarehouseAdditionalDataService : IErpWarehouseAdditionalDataService
    {
        #region Fields

        private readonly IRepository<ErpWarehouseAdditionalData> _erpErpWarehouseAdditionalDataRepository;
        private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
        private readonly IProductService _productService;

        #endregion

        #region ctor

        public ErpWarehouseAdditionalDataService(IRepository<ErpWarehouseAdditionalData> erpWarehouseAdditionalDataRepository,
            IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
            IProductService productService)
        {
            _erpErpWarehouseAdditionalDataRepository= erpWarehouseAdditionalDataRepository;
            _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
            _productService = productService;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpWarehouseAdditionalDataAsync(ErpWarehouseAdditionalData erpWarehouseAdditionalData)
        {
            await _erpErpWarehouseAdditionalDataRepository.InsertAsync(erpWarehouseAdditionalData);
        }

        public async Task UpdateErpWarehouseAdditionalDataAsync(ErpWarehouseAdditionalData erpWarehouseAdditionalData)
        {
            await _erpErpWarehouseAdditionalDataRepository.UpdateAsync(erpWarehouseAdditionalData);
        }

        #endregion

        #region Delete

        private async Task DeleteErpWarehouseAdditionalDataAsync(ErpWarehouseAdditionalData erpWarehouseAdditionalData)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpWarehouseAdditionalData.IsDeleted = true;
            await _erpErpWarehouseAdditionalDataRepository.UpdateAsync(erpWarehouseAdditionalData);
        }

        public async Task DeleteErpWarehouseAdditionalDataByIdAsync(int id)
        {
            var erpWarehouseAdditionalData = await GetErpWarehouseAdditionalDataByIdAsync(id);
            if (erpWarehouseAdditionalData != null)
            {
                await DeleteErpWarehouseAdditionalDataAsync(erpWarehouseAdditionalData);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccount by Id
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount
        /// </returns>
        public async Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpWarehouseAdditionalData = await _erpErpWarehouseAdditionalDataRepository.GetByIdAsync(id, cache => default);

            if (erpWarehouseAdditionalData== null || erpWarehouseAdditionalData.IsDeleted)
                return null;

            return erpWarehouseAdditionalData;
        }

        /// <summary>
        /// Gets an ErpAccount by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount if it is activ
        /// </returns>
        public async Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpWarehouseAdditionalData = await _erpErpWarehouseAdditionalDataRepository.GetByIdAsync(id, cache => default);

            if (erpWarehouseAdditionalData== null || !erpWarehouseAdditionalData.IsActive || erpWarehouseAdditionalData.IsDeleted)
                return null;

            return erpWarehouseAdditionalData;
        }

        /// <summary>
        /// Gets all ErpAccounts
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccounts
        /// </returns>
        public async Task<IPagedList<ErpWarehouseAdditionalData>> GetAllErpWarehouseAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false)
        {
            var erpWarehouseAdditionalData = await _erpErpWarehouseAdditionalDataRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                query = query.Where(egp => !egp.IsDeleted);
                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpWarehouseAdditionalData;
        }

        public async Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataBySalesOrgIdAsync(int salesOrgId)
        {
            if (salesOrgId == 0)
                return null;
            var map = _erpWarehouseSalesOrgMapRepository.Table;
            var erpWarehouseId = map.FirstOrDefault(f => f.ErpSalesOrgId == salesOrgId)?.ErpWarehouseId ?? 0;


            var erpWarehouseAdditionalData = await _erpErpWarehouseAdditionalDataRepository.GetByIdAsync(erpWarehouseId, cache => default);

            if (erpWarehouseAdditionalData == null || erpWarehouseAdditionalData.IsDeleted)
                return new ErpWarehouseAdditionalData();

            return erpWarehouseAdditionalData;
        }

        public async Task<ErpWarehouseSalesOrgMap> GetErpSalesOrgWarehouseForProductAsync(Product product, int erpSalesOrgId, int quantity)
        {
            if (erpSalesOrgId == 0)
                return null;

            var map = _erpWarehouseSalesOrgMapRepository.Table;
            var salesOrgAndWarehouseMap = map.FirstOrDefault(f => f.ErpSalesOrgId == erpSalesOrgId);

            if (salesOrgAndWarehouseMap == null)
                return null;

            var warehouse = new ErpWarehouseSalesOrgMap();
            var pwiList = await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id);

            var salesOrgWarehouseId = salesOrgAndWarehouseMap.NopWarehouseId;
            var selectedPwi = pwiList.Where(x => (x.WarehouseId == salesOrgWarehouseId) && (quantity <= x.StockQuantity)).FirstOrDefault();
            if (selectedPwi == null)
            {
                var otherSalesOrgWarehouseIds = map.Where(sowh => sowh.ErpSalesOrgId == erpSalesOrgId)?
                    .Where(x => x.NopWarehouseId != salesOrgWarehouseId)?
                    .Select(x => x.NopWarehouseId);

                selectedPwi = pwiList.Where(x => otherSalesOrgWarehouseIds.Contains(x.WarehouseId) && quantity <= x.StockQuantity)
                                .OrderByDescending(x => x.StockQuantity)
                                .FirstOrDefault();
            }

            if (selectedPwi == null)
                return null;

            warehouse = map.FirstOrDefault(f => f.NopWarehouseId == selectedPwi.WarehouseId);
            return warehouse;
        }
        #endregion

        #endregion
    }
}

