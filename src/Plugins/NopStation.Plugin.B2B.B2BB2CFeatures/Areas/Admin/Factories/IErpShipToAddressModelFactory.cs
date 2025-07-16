using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpShipToAddressModelFactory
{
    Task<ErpShipToAddressListModel> PrepareErpShipToAddressListModelAsync(ErpShipToAddressSearchModel searchModel);
    Task<ErpShipToAddressModel> PrepareErpShipToAddressModelAsync(ErpShipToAddressModel model, ErpShipToAddress erpShipToAddress, bool excludeProperties = false);
    Task<ErpShipToAddressSearchModel> PrepareErpShipToAddressSearchModelAsync(ErpShipToAddressSearchModel searchModel);
}