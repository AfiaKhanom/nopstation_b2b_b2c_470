using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpInvoiceModelFactory : IErpInvoiceModelFactory
{
    #region Fields

    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ErpInvoiceModelFactory(IErpInvoiceService erpInvoiceService,
        IErpAccountService erpAccountService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        ILocalizationService localizationService)
    {
        _erpInvoiceService = erpInvoiceService;
        _erpAccountService = erpAccountService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    public async Task PrepareAvailableDocumentTypesAsync(IList<SelectListItem> model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var availableSalesRepTypes = await ErpDocumentType.Invoice.ToSelectListAsync(false);
        foreach (var types in availableSalesRepTypes)
        {
            model.Add(types);
        }
        model.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
        });
    }

    #endregion

    #region Methods

    public async Task<ErpInvoiceSearchModel> PrepareErpInvoiceSearchModelAsync(ErpInvoiceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        await PrepareAvailableDocumentTypesAsync(searchModel.AvailableDocumentTypes);
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpInvoiceListModel> PrepareErpInvoiceListModelAsync(ErpInvoiceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpInvoices = await _erpInvoiceService.GetAllErpInvoiceAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            erpOrderNumber: searchModel.ErpOrderNumber,
            erpAccountId: searchModel.ErpAccountId,
            documentTypeId: searchModel.DocumentTypeId,
            erpDocumentNumber: searchModel.ErpDocumentNumber);

        var erpOrders = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync();

        var model = await new ErpInvoiceListModel().PrepareToGridAsync(searchModel, erpInvoices, () =>
        {
            return erpInvoices.SelectAwait(async erpInvoice =>
            {
                if (erpInvoice == null)
                    return null;

                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
                var erpOrder = erpOrders.FirstOrDefault(erpOrd => erpOrd.ErpOrderNumber == erpInvoice.ErpOrderNumber);

                if (erpAccount == null || erpOrder == null)
                    return null;

                var erpInvoiceModel = erpInvoice.ToModel<ErpInvoiceModel>();
                erpInvoiceModel.ErpAccountId = erpAccount.Id;
                erpInvoiceModel.ErpOrderId = erpOrder.Id;
                erpInvoiceModel.ErpAccountName = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})";
                erpInvoiceModel.DocumentDisplayName = CommonHelper.ConvertEnum(((ErpDocumentType)erpInvoice.DocumentTypeId).ToString());

                return erpInvoiceModel;
            }).Where(model => model != null);
        });

        return model;
    }

    public async Task<ErpInvoiceModel> PrepareErpInvoiceModelAsync(ErpInvoiceModel model, ErpInvoice erpInvoice)
    {
        if (erpInvoice == null)
            return model;

        model ??= erpInvoice.ToModel<ErpInvoiceModel>();

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
        if (erpAccount == null)
            return model;

        model.DocumentDisplayName = CommonHelper.ConvertEnum(((ErpDocumentType)erpInvoice.DocumentTypeId).ToString());
        model.ErpAccountName = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})";

        return model;
    }

    #endregion
}
