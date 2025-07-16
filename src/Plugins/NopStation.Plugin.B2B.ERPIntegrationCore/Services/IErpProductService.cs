using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpProductService
{
    Task<Product> GetProductBySkuAsync(string sku, bool filterOutDeleted = true);
    Task<IList<Product>> GetProductsBySkuAsync(string[] skuArray, int vendorId = 0, bool filterOutDeleted = false, bool filterOutUnpublished = false);
    Task UnpublishAllOldProduct(DateTime syncStartTime);
    Task<List<ProductWarehouseInventory>> GetProductWarehouseInventoryByProductIdsAndNopWarehouseIdsAsync(int[] productIds, int nopWarehouseId);
    Task UpdateProductsAsync(IList<Product> products);
    Task UpdateBulkProductWarehouseInventoryAsync(List<ProductWarehouseInventory> pwiToUpdate);
    Task InsertBulkProductWarehouseInventoryAsync(List<ProductWarehouseInventory> pwiToInsert);
    Task InsertBulkStockQuantityHistoryAsync(List<StockQuantityHistory> stockQuantityHistoriesToInsert);
}