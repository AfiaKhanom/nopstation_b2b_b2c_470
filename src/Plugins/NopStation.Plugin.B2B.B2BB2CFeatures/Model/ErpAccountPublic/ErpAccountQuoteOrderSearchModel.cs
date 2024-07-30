using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record ErpAccountQuoteOrderSearchModel : BaseSearchModel
    {
        public int ErpAccountId { get; set; }

        public string ErpAccountNumber { get; set; }

        public int ErpNopUserId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.SearchQuoteNumberOrName")]
        public string SearchQuoteNumberOrName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.SearchOrderDateFrom")]
        [UIHint("DateNullable")]
        [DataType(DataType.Date)]
        public DateTime? SearchOrderDateFrom { get; set; }
        public string SearchOrderDateFromStr { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.SearchOrderDateTo")]
        [UIHint("DateNullable")]
        [DataType(DataType.Date)]
        public DateTime? SearchOrderDateTo { get; set; }
    }
}
