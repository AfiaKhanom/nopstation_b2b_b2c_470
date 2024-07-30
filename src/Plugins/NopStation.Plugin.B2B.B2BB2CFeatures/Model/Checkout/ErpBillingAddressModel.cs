using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record ErpBillingAddressModel : BaseNopEntityModel
    {
        public ErpBillingAddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Address.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Address.Fields.LastName")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Address.Fields.Email")]
        public string Email { get; set; }

        [StringLength(200)]
        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.Suburb")]
        public string Suburb { get; set; }

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

        [NopResourceDisplayName("Address.Fields.County")]
        public string County { get; set; }

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

        [NopResourceDisplayName("Address.Fields.FaxNumber")]
        public string FaxNumber { get; set; }

        public bool AllowEdit { get; set; }

        public int ErpAccountId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
    }
}
