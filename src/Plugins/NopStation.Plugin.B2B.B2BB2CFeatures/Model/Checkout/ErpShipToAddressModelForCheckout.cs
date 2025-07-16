using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;

public record ErpShipToAddressModelForCheckout : BaseNopEntityModel
{
    public ErpShipToAddressModelForCheckout()
    {
        AvailableAreas = new List<SelectListItem>();
        AvailableCountries = new List<SelectListItem>();
        AvailableStates = new List<SelectListItem>();
        AvailableDeliveryDates = new List<SelectListItem>();
    }

    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Address.Fields.Email")]
    public string Email { get; set; }

    public bool OnePageCheckoutEnabled { get; set; }

    public bool IsQuoteOrder { get; set; }

    [StringLength(50)]
    [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ShipToCode")]
    public string ShipToCode { get; set; }

    [StringLength(100)]
    [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ShipToName")]
    public string ShipToName { get; set; }

    public int AddressId { get; set; }

    [StringLength(200)]
    [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.Suburb")]
    public string Suburb { get; set; }

    public IList<SelectListItem> AvailableAreas { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.DeliveryNotes")]
    public string DeliveryNotes { get; set; }

    public int ErpAccountId { get; set; }

    public string ErpAccountName { get; set; }

    public string ErpAccountNumber { get; set; }

    public int ErpSalesOrganizationId { get; set; }

    public string SalesOrganisationCode { get; set; }

    // From default
    [NopResourceDisplayName("Address.Fields.Company")]
    public string Company { get; set; }

    [NopResourceDisplayName("Address.Fields.Country")]
    public int? CountryId { get; set; }

    [NopResourceDisplayName("Address.Fields.Country")]
    public string CountryName { get; set; }

    [NopResourceDisplayName("Address.Fields.StateProvince")]
    public int? StateProvinceId { get; set; }

    [NopResourceDisplayName("Address.Fields.StateProvince")]
    public string StateProvinceName { get; set; }

    [NopResourceDisplayName("Address.Fields.City")]
    public string City { get; set; }

    [NopResourceDisplayName("Address.Fields.Address1")]
    public string Address1 { get; set; }

    [NopResourceDisplayName("Address.Fields.Address2")]
    public string Address2 { get; set; }

    [NopResourceDisplayName("Address.Fields.ZipPostalCode")]
    public string ZipPostalCode { get; set; }

    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("Address.Fields.PhoneNumber")]
    public string PhoneNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrder.Fields.CountryCode")]
    public string CountryCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
    public DateTime DeliveryDate { get; set; }
    public bool AllowEdit { get; set; }
    public string FormatedDeliveryDate { get; set; }
    public string MinDeliveryDate { get; set; }
    public string MaxDeliveryDate { get; set; }
    public bool ErpToDetermineDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
    public string DeliveryDateString { get; set; }
    public IList<SelectListItem> AvailableDeliveryDates { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SpecialInstructions")]
    public string SpecialInstructions { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.CustomerReference")]
    public string CustomerReference { get; set; }
    public bool IsActive { get; set; }
    public bool IsFullLoadRequired { get; set; }
    public IList<SelectListItem> AvailableCountries { get; set; }
    public IList<SelectListItem> AvailableStates { get; set; }

    public int DeliveryOptionId { get; set; }
}
