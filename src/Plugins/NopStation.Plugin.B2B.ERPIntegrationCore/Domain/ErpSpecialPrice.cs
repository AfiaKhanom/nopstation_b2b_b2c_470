using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpSpecialPrice:BaseEntity
    {

        public int ErpAccountId { get; set; }

        public int NopProductId { get; set; }
         
        public decimal Price { get; set; }

        public decimal ListPrice { get; set; }

        public decimal PercentageOfAllocatedStock { get; set; }

        public DateTime? PercentageOfAllocatedStockResetTimeUtc { get; set; }

        public bool VolumeDiscount { get; set; }

        public decimal DiscountPerc { get; set; }

        public string PricingNote { get; set; }

        public ErpAccount ErpAccount { get; set; }
    }
}
