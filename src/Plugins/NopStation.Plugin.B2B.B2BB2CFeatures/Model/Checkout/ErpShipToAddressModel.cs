using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;

public record ErpShipToAddressModel : BaseNopEntityModel
{
    public int ErpNopUserId { get; set; }
    public int ErpNopUserName { get; set; }
    public int ErpSalesOrganisationId { get; set; }
    public string ErpSalesOrganisationName { get; set; }
    public string ErpSalesOrganisationCode { get; set; }
    public int NearestWarehouseId { get; set; }
    public string NearestWarehouseCode { get; set; }
    public int DeliveryOption { get; set; }
    public double DistanceToNearestWarehouse { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int CreatedById { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    public int UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public int AddressId { get; set; }
    public string DeliveryNotes { get; set; }
    public string EmailAddresses { get; set; }
    public int ErpAccountId { get; set; }
    public int ShipToAddressCreatedByTypeId { get; set; }
    public int OrderId { get; set; }

    #region address

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Phone")]
    public string Phone { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Company")]
    public string Company { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Latitude")]
    public string Latitude { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Longitude")]
    public string Longitude { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.HouseNumber")]
    public string HouseNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Street")]
    public string Street { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Suburb")]
    public string Suburb { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.City")]
    public string City { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.StateProvince")]
    public string StateProvince { get; set; }
    public int? StateProvinceId { get; set; }
    public string StateProvinceCode { get; set; }
    public string StateProvinceName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.Country")]
    public string Country { get; set; }
    public int? CountryId { get; set; }
    public string CountryCode { get; set; }
    public string CountryName { get; set; }

    #endregion

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpShipToAddressModel.Fields.ZipPostalCode")]
    public string PostalCode { get; set; }
    public int IsSelected { get; set; }
    public bool IsCustomerOnDeliveryRoute { get; set; }
}

