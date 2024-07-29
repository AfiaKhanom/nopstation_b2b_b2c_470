using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpShipToAddress : ErpBaseEntity
    {
        public string ShipToCode { get; set; }

        public string ShipToName { get; set; }

        public int AddressId { get; set; }

        public string Suburb { get; set; }

        public string ProvinceCode { get; set; }

        public string DeliveryNotes { get; set; }

        public string EmailAddresses { get; set; }

        public string RepNumber { get; set; }

        public string RepFullName { get; set; }

        public string RepPhoneNumber { get; set; }

        public string RepEmail { get; set; }

        public DateTime? LastShipToAddressSyncDate { get; set; }
    }
}
