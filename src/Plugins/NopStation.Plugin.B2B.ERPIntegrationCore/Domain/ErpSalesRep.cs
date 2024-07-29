using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpSalesRep:ErpBaseEntity
    {
        private ICollection<ErpSalesRepSalesOrgMap> _erpSalesRepSalesOrgMaps; 

        public int NopCustomerId { get; set; }

        public int SalesRepTypeId { get; set; }

        public SalesRepType SalesRepType
        {
            get => (SalesRepType)SalesRepTypeId;
            set => SalesRepTypeId = (int)value;
        }

        public virtual ICollection<ErpSalesRepSalesOrgMap> ErpSalesRepSalesOrgsMap
        {
            get => _erpSalesRepSalesOrgMaps ?? (_erpSalesRepSalesOrgMaps = new List<ErpSalesRepSalesOrgMap>());
            protected set => _erpSalesRepSalesOrgMaps = value;
        } 
    }
}
