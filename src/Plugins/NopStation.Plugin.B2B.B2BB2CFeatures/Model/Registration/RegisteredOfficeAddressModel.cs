using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public partial record RegisteredOfficeAddressModel : BaseNopEntityModel
    {
        public RegisteredOfficeAddressModel()
        {
            AvailableCountries_ROA = new List<SelectListItem>();
            AvailableStates_ROA = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Address.Fields.FirstName")]
        public string FirstName_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.LastName")]
        public string LastName_ROA { get; set; }
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Address.Fields.Email")]
        public string Email_ROA { get; set; }


        public bool CompanyEnabled_ROA { get; set; }
        public bool CompanyRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.Company")]
        public string Company_ROA { get; set; }

        public bool CountryEnabled_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public int? CountryId_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public string CountryName_ROA { get; set; }

        public bool StateProvinceEnabled_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public int? StateProvinceId_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public string StateProvinceName_ROA { get; set; }

        public bool CountyEnabled_ROA { get; set; }
        public bool CountyRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.County")]
        public string County_ROA { get; set; }

        public bool CityEnabled_ROA { get; set; }
        public bool CityRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.City")]
        public string City_ROA { get; set; }

        public bool StreetAddressEnabled_ROA { get; set; }
        public bool StreetAddressRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.Address1")]
        public string Address1_ROA { get; set; }

        public bool StreetAddress2Enabled_ROA { get; set; }
        public bool StreetAddress2Required_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.Address2")]
        public string Address2_ROA { get; set; }

        public bool ZipPostalCodeEnabled_ROA { get; set; }
        public bool ZipPostalCodeRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.ZipPostalCode")]
        public string ZipPostalCode_ROA { get; set; }

        public bool PhoneEnabled_ROA { get; set; }
        public bool PhoneRequired_ROA { get; set; }
        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Address.Fields.PhoneNumber")]
        public string PhoneNumber_ROA { get; set; }

        public bool FaxEnabled_ROA { get; set; }
        public bool FaxRequired_ROA { get; set; }
        [NopResourceDisplayName("Address.Fields.FaxNumber")]
        public string FaxNumber_ROA { get; set; }

        public IList<SelectListItem> AvailableCountries_ROA { get; set; }
        public IList<SelectListItem> AvailableStates_ROA { get; set; }
    }
}
