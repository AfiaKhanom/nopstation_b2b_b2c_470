using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpGetRequestModel
{
    public string AccountNumber { get; set; }

    public string DocumentNumber { get; set; }

    public string OrderNumber { get; set; }

    public string ProductSku { get; set; }

    public string Start { get; set; } = "0";

    public int Limit { get; set; }

    public DateTime? DateFrom { get; set; }

    public string Location { get; set; }

    public string PriceCode { get; set; }

    public string UrlExtention { get; set; }
}
