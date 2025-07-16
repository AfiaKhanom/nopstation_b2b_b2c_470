using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpInvoiceModelFactory
{
    Task<ErpInvoiceListModel> PrepareErpInvoiceListModelAsync(ErpInvoiceSearchModel searchModel);
    Task<ErpInvoiceModel> PrepareErpInvoiceModelAsync(ErpInvoiceModel model, ErpInvoice erpInvoice);
    Task<ErpInvoiceSearchModel> PrepareErpInvoiceSearchModelAsync(ErpInvoiceSearchModel searchModel);
}