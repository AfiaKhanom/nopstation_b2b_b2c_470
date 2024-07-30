using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpNopUserAccountMapModel : BaseNopEntityModel
    {
        #region Ctor
        public ErpNopUserAccountMapModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpAccountId")]
        public int ErpAccountId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpAccountId")]
        public string ErpAccountNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpUserId")]
        public int ErpUserId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.CustomerRoles")]
        public string CustomerRolesIds { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.CustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        #endregion
    }
}
