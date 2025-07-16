using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpProductService : IErpProductService
{
    #region Fields

    private readonly INopDataProvider _nopDataProvider;
    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
    private readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
    private readonly IRepository<Product> _productRepository;

    #endregion

    #region Ctor

    public ErpProductService(INopDataProvider nopDataProvider,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
        IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
        IRepository<Product> productRepository
)
    {
        _nopDataProvider = nopDataProvider;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
        _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
        _productRepository = productRepository;
    }

    #endregion

    #region Methods

    public async Task<Product> GetProductBySkuAsync(string sku, bool filterOutDeleted = true)
    {
        if (string.IsNullOrEmpty(sku))
            return null;

        sku = sku.Trim().ToLower();

        var query = from p in _productRepository.Table
                    orderby p.Id
                    where p.Sku.Trim().ToLower() == sku
                    select p;

        if (filterOutDeleted)
            query = query.Where(p => !p.Deleted);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<Product>> GetProductsBySkuAsync(string[] skuArray, int vendorId = 0, bool filterOutDeleted = false, bool filterOutUnpublished = false)
    {
        ArgumentNullException.ThrowIfNull(skuArray);

        var query = _productRepository.Table;
        query = query.Where(p => skuArray.Any(s => p.Sku.Trim().ToLower() == s.Trim().ToLower()));

        if (filterOutDeleted)
            query = query.Where(p => !p.Deleted);

        if (filterOutUnpublished)
            query = query.Where(p => p.Published);

        if (vendorId != 0)
            query = query.Where(p => p.VendorId == vendorId);

        return await query.ToListAsync();
    }

    public async Task UnpublishAllOldProduct(DateTime syncStartTime)
    {
        if (syncStartTime == DateTime.MinValue)
            return;

        var connectionString = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

        var sqlCommand = $"Update [{connectionString.InitialCatalog}].[dbo].[Product] Set [Published] = 0 Where [UpdatedOnUtc] < '{syncStartTime:yyyy-MM-dd HH:mm:ss}'";

        await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
    }

    public async Task<List<ProductWarehouseInventory>> GetProductWarehouseInventoryByProductIdsAndNopWarehouseIdsAsync(int[] productIds, int nopWarehouseId)
    {
        if (productIds == null || productIds.Length == 0)
            return null;

        return await _productWarehouseInventoryRepository.Table.Where(x => productIds.Contains(x.ProductId) && x.WarehouseId == nopWarehouseId).ToListAsync();
    }

    public async Task UpdateProductsAsync(IList<Product> products)
    {
        if (products == null || !products.Any())
            return;

        await _productRepository.UpdateAsync(products);
    }

    public async Task UpdateBulkProductWarehouseInventoryAsync(List<ProductWarehouseInventory> pwiToUpdate)
    {
        if (pwiToUpdate == null || !pwiToUpdate.Any())
            return;

        await _productWarehouseInventoryRepository.UpdateAsync(pwiToUpdate);
    }

    public async Task InsertBulkProductWarehouseInventoryAsync(List<ProductWarehouseInventory> pwiToInsert)
    {
        if (pwiToInsert == null || !pwiToInsert.Any())
            return;

        await _productWarehouseInventoryRepository.InsertAsync(pwiToInsert);
    }

    public async Task InsertBulkStockQuantityHistoryAsync(List<StockQuantityHistory> stockQuantityHistoriesToInsert)
    {
        if (stockQuantityHistoriesToInsert == null || !stockQuantityHistoriesToInsert.Any())
            return;

        await _stockQuantityHistoryRepository.InsertAsync(stockQuantityHistoriesToInsert);
    }

    #endregion
}