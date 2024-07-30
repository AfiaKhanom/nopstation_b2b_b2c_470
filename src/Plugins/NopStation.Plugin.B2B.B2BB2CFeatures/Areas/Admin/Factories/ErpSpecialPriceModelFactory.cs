using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpSpecialPriceModelFactory : IErpSpecialPriceModelFactory
    {
        #region Fields

        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSpecialPriceService _erpSpecialPriceService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ErpSpecialPriceModelFactory(
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            IErpSpecialPriceService erpSpecialPriceService,
            IDateTimeHelper dateTimeHelper)
        {
            _erpSalesOrgService = erpSalesOrgService;
            _erpAccountService = erpAccountService;
            _erpSpecialPriceService = erpSpecialPriceService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public async Task<ErpSpecialPriceSearchModel> PrepareErpProductPricingSearchModel(ErpSpecialPriceSearchModel searchModel, int productId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ProductId = productId;
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public async Task<ErpSpecialPriceListModel> PrepareErpProductPricingListModel(ErpSpecialPriceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var erpProductPricings = await _erpSpecialPriceService.GetAllErpSpecialPricesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, getOnlyTotalCount: false, productId: searchModel.ProductId, accountId: searchModel.SearchErpAccountId);
            var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync();
            var erpSalesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            var model = new ErpSpecialPriceListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
            {
                return erpProductPricings.Select(productPricing =>
                {
                    var erpAccount = erpAccounts.Where(w => w.Id == productPricing.ErpAccountId).FirstOrDefault();
                    var pricingModel = new ErpSpecialPriceModel
                    {
                        Id = productPricing.Id,
                        ProductId = productPricing.NopProductId,
                        Price = productPricing.Price,
                        PricingNote = productPricing.PricingNote,
                        DiscountPerc = productPricing.DiscountPerc,
                        PercentageOfAllocatedStock = productPricing.PercentageOfAllocatedStock
                    };

                    if (erpAccount != null)
                    {
                        var erpAccountSalesOrg = erpSalesOrgs.Where(w => w.Id == erpAccount.ErpSalesOrgId).FirstOrDefault();
                        pricingModel.ErpAccountId = productPricing.ErpAccountId;
                        pricingModel.ErpAccountNumber = erpAccount.AccountNumber;
                        pricingModel.ErpAccountSalesOrgId = erpAccount.ErpSalesOrgId;
                        pricingModel.ErpAccountSalesOrgName = erpAccountSalesOrg.Name;
                    }

                    return pricingModel;
                });
            });
            return model;
        }

        public async Task<ErpSpecialPriceModel> PrepareErpProductPricingModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing)
        {
            if (erpProductPricing != null)
            {
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpProductPricing.ErpAccountId);

                model = model ?? new ErpSpecialPriceModel();
                model.Id = erpProductPricing.Id;
                model.ProductId = erpProductPricing.NopProductId;
                model.Price = erpProductPricing.Price;
                model.DiscountPerc = erpProductPricing.DiscountPerc;
                model.PricingNote = erpProductPricing.PricingNote;
                model.PercentageOfAllocatedStock = erpProductPricing.PercentageOfAllocatedStock;
                if (erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.HasValue)
                    model.PercentageOfAllocatedStockResetTimeUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.Value, DateTimeKind.Utc);

                if (erpAccount != null)
                {
                    var erpAccountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

                    model.ErpAccountId = erpProductPricing.ErpAccountId;
                    model.ErpAccountNumber = erpAccount.AccountNumber;
                    model.ErpSalesOrgId = erpAccount.ErpSalesOrgId;
                    model.ErpAccountSalesOrgName = erpAccountSalesOrg?.Name;
                }
            }
            return model;
        }

        //public byte[] ExportErpSpecialPriceToXlsxAll(ProductSearchModel searchModel)
        //{
        //    var categoryIds = new List<int> { searchModel.SearchCategoryId };
        //    if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        //    {
        //        var childCategoryIds = _categoryService.GetChildCategoryIds(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
        //        categoryIds.AddRange(childCategoryIds);
        //    }

        //    if (categoryIds != null && categoryIds.Contains(0))
        //        categoryIds.Remove(0);

        //    var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

        //    var query = @"SELECT account.AccountNumber, account.AccountName, SO.SalesOrganisationName,  SO.SalesOrganisationCode, p.Sku,PAPP.Price,PAPP.PricingNote,PAPP.CustomerUoM,PAPP.PercentageOfAllocatedStock,PAPP.DiscountPerc
        //                  FROM ErpSpecialPrice  PAPP with(nolock) inner join   Product p on p.Id=PAPP.ProductId and p.Deleted=0 and p.Published=1
        //                  inner join B2BAccount account on account.Id=PAPP.B2BAccountId inner join B2BSalesOrganisation SO on SO.Id=account.B2BSalesOrganizationId ";

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
        //        var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "ErpSpecialPrice_" + currentDate);
        //        var fileBytesArray = excel.GetAsByteArray();
        //        return fileBytesArray;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "ErpSpecialPrice_ Export excel File Generate Fail",
        //               "Query: " + query + Environment.NewLine + ex);
        //    }
        //    return null;
        //}

        //public byte[] ExportErpSpecialPriceToXlsx(List<int> ids)
        //{
        //    var query = @"SELECT account.AccountNumber, account.AccountName, SO.SalesOrganisationName, p.Sku,PAPP.Price,PAPP.PricingNote,PAPP.CustomerUoM,PAPP.PercentageOfAllocatedStock,PAPP.DiscountPerc
        //                  FROM ErpSpecialPrice PAPP with(nolock) inner join Product p on p.Id=PAPP.ProductId and p.Deleted=0 and p.Published=1
        //                  and p.Id IN (" + string.Join(", ", ids) + ") inner join B2BAccount account on account.Id=PAPP.B2BAccountId inner join B2BSalesOrganisation SO on SO.Id=account.B2BSalesOrganizationId order by p.sku";
        //    try
        //    {
        //        var currentDate = DateTime.Now.ToString("g");
        //        var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "ErpSpecialPrice_" + currentDate);
        //        var fileBytesArray = excel.GetAsByteArray();
        //        return fileBytesArray;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "ErpSpecialPrice_ Export excel File Generate Fail",
        //               "Query: " + query + Environment.NewLine + ex);
        //    }
        //    return null;

        //    //var currentDate = DateTime.Now.ToString("g");
        //    //var excel = _b2BExportImportManager.GetExcelPackageByQuery(query, "ErpSpecialPrice_" + currentDate);
        //    //var fileBytesArray = excel.GetAsByteArray();
        //    //return fileBytesArray;
        //}

        //public void ImportErpSpecialPriceFromXlsx(Stream stream)
        //{
        //    _objectContext.ExecuteSqlCommand("Truncate TABLE [dbo].[ErpSpecialPriceImport];");

        //    var totalRow = _b2BExportImportManager.WriteStreamInDatabase(stream, "ErpSpecialPriceImport");

        //    if (totalRow > 0)
        //    {
        //        _objectContext.ExecuteSqlCommand("[dbo].[ErpSpecialPriceImportProcedure]",
        //        false, 3600);
        //    }
        //}

        #endregion
    }
}
