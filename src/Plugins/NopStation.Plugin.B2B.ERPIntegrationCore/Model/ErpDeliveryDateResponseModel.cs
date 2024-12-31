using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpDeliveryDateResponseModel
{
    public bool IsFullLoadRequired { get; set; }
    public IList<DeliveryDate> DeliveryDates { get; set; }
}

public class DeliveryDate
{
    public string DelDate { get; set; }
}
