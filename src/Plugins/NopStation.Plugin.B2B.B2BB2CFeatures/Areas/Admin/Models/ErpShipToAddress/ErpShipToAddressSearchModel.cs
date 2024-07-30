using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress
{
    public record ErpShipToAddressSearchModel : BaseSearchModel
    {
        #region ctor

        public ErpShipToAddressSearchModel() 
        {
            ShowInActiveOption = new List<SelectListItem>();
            AvailableErpAccounts = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchShipToCode")]
        public string SearchShipToCode { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchShipToName")]
        public string SearchShipToName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchEmailAddresses")]
        public string SearchEmailAddresses { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchErpAccountId")]
        public int SearchErpAccountId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchRepNumber")]
        public string SearchRepNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchRepFullName")]
        public string SearchRepFullName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchRepPhoneNumber")]
        public string SearchRepPhoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.SearchRepEmail")]
        public string SearchRepEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpShipToAddress.Fields.Show")]
        public int ShowInActive { get; set; }

        public IList<SelectListItem> ShowInActiveOption { get; set; }
        public IList<SelectListItem> AvailableErpAccounts { get; set; }

        #endregion
    }
}
