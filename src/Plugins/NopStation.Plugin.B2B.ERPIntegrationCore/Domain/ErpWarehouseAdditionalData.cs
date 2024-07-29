using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpWarehouseAdditionalData:ErpBaseEntity
    {
        public string Code { get; set; }

        public string NopProductId { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
