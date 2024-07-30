using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpPriceGroupProductPricingModelFactory : IErpPriceGroupProductPricingModelFactory
    {
        #region Fields

        private readonly IErpGroupPriceService _erpGroupPriceService;
        private readonly IErpGroupPriceCodeService _erpPriceGroupCodeService;
        private readonly IErpGroupPriceCodeModelFactory _erpPriceGroupCodeModelFactory;

        #endregion

        #region Ctor

        public ErpPriceGroupProductPricingModelFactory(
            IErpGroupPriceService erpGroupPriceService,
            IErpGroupPriceCodeService erpPriceGroupCodeService,
            IErpGroupPriceCodeModelFactory erpPriceGroupCodeModelFactory)
        {
            _erpGroupPriceService = erpGroupPriceService;
            _erpPriceGroupCodeService = erpPriceGroupCodeService;
            _erpPriceGroupCodeModelFactory = erpPriceGroupCodeModelFactory;
        }

        #endregion

        #region Methods

        public async Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductPricingSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ProductId = productId;
            searchModel.SetGridPageSize();
            await PrepareErpProductPricingModel(searchModel.AddErpPriceGroupProductPricing, null);
            return searchModel;
        }

        public async Task<ErpPriceGroupProductPricingListModel> PrepareErpProductPricingListModel(ErpPriceGroupProductPricingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var erpProductPricings = await _erpGroupPriceService.GetAllErpGroupPricesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: false, getOnlyTotalCount: false, overridePublished: false, productId: searchModel.ProductId, groupCode: searchModel.SearchErpPriceGroupCode);

            var erpGroupPriceCodes = await _erpPriceGroupCodeService.GetAllErpGroupPriceCodesAsync();

            var model = new ErpPriceGroupProductPricingListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
            {
                return erpProductPricings.Select(productPricing =>
                {
                    var erpGroupPriceCodeCheck = erpGroupPriceCodes.FirstOrDefault(f => f.Id == productPricing.ErpNopGroupPriceCodeId);

                    if (erpGroupPriceCodeCheck is null)
                    {
                        return null;
                    }
                    var pricingModel = new ErpPriceGroupProductPricingModel
                    {
                        Id = productPricing.Id,
                        ProductId = productPricing.NopProductId,
                        ErpGroupPriceCodeId = productPricing.ErpNopGroupPriceCodeId,
                        ErpGroupPriceCode = erpGroupPriceCodeCheck.Code,
                        Price = productPricing.Price
                    };
                    return pricingModel;
                });
            });
            return model;
        }

        public async Task<ErpPriceGroupProductPricingModel> PrepareErpProductPricingModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing)
        {
            if (erpProductPricing != null)
            {
                var erpGroupPriceCode = await _erpPriceGroupCodeService.GetErpGroupPriceCodeByIdAsync(erpProductPricing.ErpNopGroupPriceCodeId);

                model ??= new ErpPriceGroupProductPricingModel();
                model.Id = erpProductPricing.Id;
                model.ProductId = erpProductPricing.NopProductId;
                model.ErpGroupPriceCodeId = erpProductPricing.ErpNopGroupPriceCodeId;
                model.ErpGroupPriceCode = erpGroupPriceCode.Code;
                model.Price = erpProductPricing.Price;
            }

            _erpPriceGroupCodeModelFactory.PrepareErpGroupPriceCodes(model.AvailableErpPriceGroupCodes, false);
            return model;
        }

        //public byte[] ExportB2BPriceGroupProductPricingToXlsxAll(ProductSearchModel searchModel)
        //{

        //    //var query = @"SELECT TOP (1000000) PGC.PriceGroupCodeName, p.Sku, PGPP.Price 
        //    //            from B2BPriceGroupProductPricing PGPP with(nolock) 
        //    //            inner join Product p on p.Id = PGPP.ProductId and p.Deleted = 0 and p.Published = 1
        //    //            inner join B2BPriceGroupCode PGC on PGPP.B2BPriceGroupCodeId = PGC.Id order by p.sku";

        //    var categoryIds = new List<int> { searchModel.SearchCategoryId };
        //    if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        //    {
        //        var childCategoryIds = _categoryService.GetChildCategoryIds(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
        //        categoryIds.AddRange(childCategoryIds);
        //    }

        //    if (categoryIds != null && categoryIds.Contains(0))
        //        categoryIds.Remove(0);
        //    var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

        //    var query = @"SELECT PGC.PriceGroupCodeName, p.Sku, PGPP.Price 
        //                from B2BPriceGroupProductPricing PGPP with(nolock) 
        //                inner join Product p on p.Id = PGPP.ProductId and p.Deleted = 0 and p.Published = 1
        //                inner join B2BPriceGroupCode PGC on PGPP.B2BPriceGroupCodeId = PGC.Id ";

        //    if (categoryIds.Count > 0)
        //        query += " INNER JOIN Product_Category_Mapping pcm with (NOLOCK) ON p.Id = pcm.ProductId ";

        //    query += " where p.Deleted=0 ";

        //    if (!string.IsNullOrEmpty(searchModel.SearchProductName))
        //        query += " and p.[Name] like '%" + searchModel.SearchProductName + "%' ";

        //    if (searchModel.SearchPublishedId > 0)
        //        query += " and p.Published = " + searchModel.SearchPublishedId % 2;

        //    if (searchModel.SearchWarehouseId > 0)
        //    {
        //        query += " AND  ( (p.UseMultipleWarehouses = 0 AND p.WarehouseId = " + searchModel.SearchWarehouseId + " )OR (p.UseMultipleWarehouses > 0 AND EXISTS(SELECT 1 FROM ProductWarehouseInventory[pwi] WHERE[pwi].WarehouseId = " + searchModel.SearchWarehouseId + " AND[pwi].ProductId = p.Id)) )";
        //    }

        //    if (searchModel.SearchProductTypeId > 0)
        //        query += " and p.ProductTypeId = " + searchModel.SearchProductTypeId;

        //    if (categoryIds.Count > 0)
        //        query += " AND pcm.CategoryId IN ( " + commaSeparatedCategoryIds + ")";

        //    query += " order by p.sku;";

        //    try
        //    {
        //        var currentDate = DateTime.Now.ToString("g");
        //        var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "B2BPriceGroupProductPricing" + currentDate);
        //        var fileBytesArray = excel.GetAsByteArray();
        //        return fileBytesArray;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "B2BPriceGroupProductPricing Export excel File Generate Fail",
        //            "Query: " + query + Environment.NewLine + ex);
        //    }
        //    return null;

        //    //var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "B2BPriceGroupProductPricing");
        //    //var fileBytesArray = excel.GetAsByteArray();
        //    //return fileBytesArray;
        //}

        //public byte[] ExportB2BPriceGroupProductPricingToXlsx(List<int> ids)
        //{
        //    var query = @"SELECT PGC.PriceGroupCodeName, p.Sku, PGPP.Price 
        //                from B2BPriceGroupProductPricing PGPP with(nolock) 
        //                inner join Product p on p.Id = PGPP.ProductId and p.Deleted = 0 and p.Published = 1
        //                and p.Id IN (" + string.Join(", ", ids) + ") inner join B2BPriceGroupCode PGC on PGPP.B2BPriceGroupCodeId = PGC.Id order by p.sku";
        //    try
        //    {
        //        var currentDate = DateTime.Now.ToString("g");
        //        var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "B2BPriceGroupProductPricing" + currentDate);
        //        var fileBytesArray = excel.GetAsByteArray();
        //        return fileBytesArray;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "B2BPriceGroupProductPricing Export excel File Generate Fail",
        //               "Query: " + query + Environment.NewLine + ex);
        //    }
        //    return null;

        //    //var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "B2BPriceGroupProductPricing");
        //    //var fileBytesArray = excel.GetAsByteArray();
        //    //return fileBytesArray;  
        //}

        //public void ImportB2BPriceGroupProductPricingFromXlsx(Stream stream)
        //{
        //    _objectContext.ExecuteSqlCommand("Truncate TABLE [dbo].[B2BPriceGroupProductPricingImport];");

        //    var totalRow = _b2BExportImportManager.WriteStreamInDatabase(stream, "B2BPriceGroupProductPricingImport");

        //    if (totalRow > 0)
        //    {
        //        _objectContext.ExecuteSqlCommand("[dbo].[B2BPriceGroupProductPricingImportProcedure]",
        //        false, 3600);
        //    }
        //}

        #endregion
    }
}