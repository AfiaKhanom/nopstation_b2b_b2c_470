using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpInvoiceModelFactory : IErpInvoiceModelFactory
    {
        #region Fields

        private readonly IErpInvoiceService _erpInvoiceService;
        private readonly ICommonHelper _commonHelper;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;

        #endregion

        #region ctor

        public ErpInvoiceModelFactory(
            IErpInvoiceService erpInvoiceService,
            ICommonHelper commonHelper,
            IErpAccountService erpAccountService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService
            )
        {
            _erpInvoiceService = erpInvoiceService;
            _commonHelper = commonHelper;
            _erpAccountService = erpAccountService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        }

        #endregion

        #region Method

        public async Task<ErpInvoiceSearchModel> PrepareErpInvoiceSearchModelAsync(ErpInvoiceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Prepare AvailableDocumentTypes dropdown options
            searchModel.AvailableDocumentTypes = await _commonHelper.PrepareDropdownDataFromEnumAsync<ErpDocumentType>();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ErpInvoiceListModel> PrepareErpInvoiceListModelAsync(ErpInvoiceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get ERP Accounts
            var erpInvoices = await _erpInvoiceService.GetAllErpInvoiceAsync(
                //postingDateUtc: searchModel.PostingDateUtc,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                erpOrderNumber: searchModel.ErpOrderNumber,
                erpAccountId: searchModel.ErpAccountId,
                documentTypeId: searchModel.DocumentTypeId,
                erpDocumentNumber: searchModel.ErpDocumentNumber
                );

            var erpOrders = (await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync()).ToList();

            //prepare list model
            var model = await new ErpInvoiceListModel().PrepareToGridAsync(searchModel, erpInvoices, () =>
            {
                //fill in model values from the entity
                return erpInvoices.SelectAwait(async erpInvoice =>
                {
                    //fill in model values from the entity
                    var erpInvoiceModel = erpInvoice.ToModel<ErpInvoiceModel>();
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
                    var erpOrder = erpOrders.Find(erpOrd => erpOrd.ErpOrderNumber == erpInvoice.ErpOrderNumber);

                    erpInvoiceModel.ErpAccountId = erpAccount.Id;
                    erpInvoiceModel.ErpOrderId = erpOrder?.Id ?? 0;
                    erpInvoiceModel.ErpAccountName = erpAccount?.AccountName + " (" + erpAccount?.AccountNumber + ")";
                    erpInvoiceModel.DocumentDisplayName = erpInvoice.DocumentType.ToString();
                    return erpInvoiceModel;
                });
            });

            return model;
        }

        public async Task<ErpInvoiceModel> PrepareErpInvoiceModelAsync(ErpInvoiceModel model, ErpInvoice erpInvoice)
        {
            if (erpInvoice != null)
            {
                //fill in model values from the entity
                model ??= erpInvoice.ToModel<ErpInvoiceModel>();
                model.DocumentDisplayName = erpInvoice.DocumentType.ToString();
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
                model.ErpAccountName = $"{erpAccount?.AccountName} ({erpAccount?.AccountNumber})";
            }

            return model;
        }

        #endregion
    }
}
