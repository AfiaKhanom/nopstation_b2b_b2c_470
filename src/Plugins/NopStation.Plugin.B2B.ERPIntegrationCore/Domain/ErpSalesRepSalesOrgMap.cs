using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpSalesRepSalesOrgMap:BaseEntity
    {
        public int ErpSalesRepId { get; set; }

        public int ErpSalesOrgId { get; set; }

        public virtual ErpSalesRep ErpSalesRep { get; set; }

        public virtual ErpSalesOrg ErpSalesOrg { get; set; }
    }
}
