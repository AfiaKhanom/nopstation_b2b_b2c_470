using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;

public record ErpShipToAddressSelectorModel : BaseNopEntityModel
{
    public ErpShipToAddressSelectorModel()
    {
        AvailableShipToAddresses = new List<ErpShipToAddressModel>();
    }
    public IList<ErpShipToAddressModel> AvailableShipToAddresses { get; set; }
    public int ErpNopUserId { get; set; }
    public bool IsSalesOrgDifferent { get; set; }
    public int NextDefaultErpShipToAddressId { get; set; }
    public int SelectedErpShipToAddressId { get; set; }
    public int DefaultAddressChanged { get; internal set; }
}
