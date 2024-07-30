using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Areas.Admin.Models.Common;
using System.ComponentModel.DataAnnotations;
using System;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress
{
    public record ErpShipToAddressModel : ErpBaseEntityModel
    {
        public ErpShipToAddressModel()
        {
            AvailableShipToAddressCreatedByTypes = new List<SelectListItem>();
            AddressModel = new AddressModel();
        }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ShipToCode")]
        public string ShipToCode { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ShipToName")]
        public string ShipToName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.AddressId")]
        public int AddressId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.AddressId")]
        public AddressModel AddressModel { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.Suburb")]
        public string Suburb { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ProvinceCode")]
        public string ProvinceCode { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.DeliveryNotes")]
        public string DeliveryNotes { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.EmailAddresses")]
        public string EmailAddresses { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ErpAccountId")]
        public int ErpAccountId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ErpAccountId")]
        public string ErpAccount { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.RepNumber")]
        public string RepNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.RepFullName")]
        public string RepFullName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.RepPhoneNumber")]
        public string RepPhoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.RepEmail")]
        public string RepEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.ShipToAddressCreatedByTypeId")]
        public int ShipToAddressCreatedByTypeId { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.LastShipToAddressSyncDate")]
        public DateTime? LastShipToAddressSyncDate { get; set; }

        public IList<SelectListItem> AvailableShipToAddressCreatedByTypes { get; set; }

        public int ErpAccountSalesOrgId { get; set; }

        public string ErpAccountSalesOrgName { get; set;  }

        public bool IsDeletedErpAccount { get; set; }   
    }
}
