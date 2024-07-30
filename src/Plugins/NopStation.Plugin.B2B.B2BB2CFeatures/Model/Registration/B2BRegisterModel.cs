using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Nop.Web.Models.Customer;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record B2BRegisterModel: RegisterModel
    {
        public B2BRegisterModel()
        {
            AvailableB2BSalesOrganizations = new List<SelectListItem>();
            B2BSalesOrganizationId = 1; //**will be implemented later if needed
        }

        [NopResourceDisplayName("B2B.Account.Fields.B2BSalesOrganization")]
        public int B2BSalesOrganizationId { get; set; }

        public IList<SelectListItem> AvailableB2BSalesOrganizations { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.SpecialInstructions")]
        public string SpecialInstructions { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.RegistrationAuthorisedBy")]
        public string RegistrationAuthorizedBy { get; set; }


        [NopResourceDisplayName("B2B.Account.Fields.B2CIdentificationNumber")]
        public string B2CIdentificationNumber { get; set; }
         

        [NopResourceDisplayName("B2B.Account.Fields.ShipToCode")]
        public string ShipToCode { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.ShipToName")]

        public string ShipToName { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.RepNumber")]

        public string RepNumber { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.RepFullName")]

        public string RepFullName { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.RepPhoneNumber")]

        public string RepPhoneNumber { get; set; }

        [NopResourceDisplayName("B2B.Account.Fields.RepEmail")]

        public string RepEmail { get; set; }

        public bool IsB2BUser { get; set; }
    }
}
