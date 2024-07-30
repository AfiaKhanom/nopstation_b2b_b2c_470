using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpSalesOrgWarehouseModel : BaseNopEntityModel
    {
        public ErpSalesOrgWarehouseModel()
        {
            AvailableWarehouses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.Fields.Warehouse")]
        public int WarehouseId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
        public string WarehouseName { get; set; }

        public int ErpSalesOrgId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.Fields.SalesOrgCode")]
        public string SalesOrgCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.Fields.SalesOrgName")]
        public string SalesOrgName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.ErpWarehouseCode")]
        public string ErpWarehouseCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.LastUpdateTime")]
        public string LastUpdateTime { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}
