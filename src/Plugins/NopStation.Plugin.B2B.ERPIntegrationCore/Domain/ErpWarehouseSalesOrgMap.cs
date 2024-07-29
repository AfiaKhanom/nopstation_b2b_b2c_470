using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpWarehouseSalesOrgMap:BaseEntity
    {
        public int NopWarehouseId { get; set; }

        public int ErpWarehouseId { get; set; }

        public int ErpSalesOrgId { get; set; }

        public ErpWarehouseAdditionalData ErpWarehouseAdditionalData { get; set; }
    }
}
