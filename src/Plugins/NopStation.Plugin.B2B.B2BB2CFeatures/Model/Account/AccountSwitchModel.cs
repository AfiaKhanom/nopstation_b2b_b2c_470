using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Account
{
    public record AccountSwitchModel
    {
        public AccountSwitchModel()
        {
            AvailableErpAccounts = new List<SelectListItem>();
        }
        public int ErpAccountId { get; set; }
        public int CustomerId { get; set; }
        public string RedirectUrl { get; set; }
        public IList<SelectListItem> AvailableErpAccounts { get; set; }
    }
}
