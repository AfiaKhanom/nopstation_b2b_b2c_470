using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpShiptoAddressErpAccountMap : BaseEntity
    {
        public int ErpAccountId {get;set;}
        public int ErpShiptoAddressId {get;set;}

        public ErpAccount ErpAccount { get;set;}
        public ErpShipToAddress ErpShipToAddress { get;set;}
    }
}
