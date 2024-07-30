using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public partial record PhysicalTradingAddressModel : BaseNopEntityModel
    {
        public PhysicalTradingAddressModel()
        {
            AvailableCountries_PTA = new List<SelectListItem>();
            AvailableStates_PTA = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Address.Fields.FirstName")]
        public string FirstName_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.LastName")]
        public string LastName_PTA { get; set; }
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Address.Fields.Email")]
        public string Email_PTA { get; set; }


        public bool CompanyEnabled_PTA { get; set; }
        public bool CompanyRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.Company")]
        public string Company_PTA { get; set; }

        public bool CountryEnabled_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public int? CountryId_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public string CountryName_PTA { get; set; }

        public bool StateProvinceEnabled_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public int? StateProvinceId_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public string StateProvinceName_PTA { get; set; }

        public bool CountyEnabled_PTA { get; set; }
        public bool CountyRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.County")]
        public string County_PTA { get; set; }

        public bool CityEnabled_PTA { get; set; }
        public bool CityRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.City")]
        public string City_PTA { get; set; }

        public bool StreetAddressEnabled_PTA { get; set; }
        public bool StreetAddressRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.Address1")]
        public string Address1_PTA { get; set; }

        public bool StreetAddress2Enabled_PTA { get; set; }
        public bool StreetAddress2Required_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.Address2")]
        public string Address2_PTA { get; set; }

        public bool ZipPostalCodeEnabled_PTA { get; set; }
        public bool ZipPostalCodeRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.ZipPostalCode")]
        public string ZipPostalCode_PTA { get; set; }

        public bool PhoneEnabled_PTA { get; set; }
        public bool PhoneRequired_PTA { get; set; }
        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Address.Fields.PhoneNumber")]
        public string PhoneNumber_PTA { get; set; }

        public bool FaxEnabled_PTA { get; set; }
        public bool FaxRequired_PTA { get; set; }
        [NopResourceDisplayName("Address.Fields.FaxNumber")]
        public string FaxNumber_PTA { get; set; }

        public IList<SelectListItem> AvailableCountries_PTA { get; set; }
        public IList<SelectListItem> AvailableStates_PTA { get; set; }
    }
}
