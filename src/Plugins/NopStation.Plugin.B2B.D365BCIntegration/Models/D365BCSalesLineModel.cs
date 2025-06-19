using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.D365BCIntegration.Models;
public class D365BCSalesLineModel
{
    [JsonProperty("@odata.etag")]
    public string ODataEtag { get; set; }

    [JsonProperty("Document_Type")]
    public string Document_Type { get; set; }

    [JsonProperty("Document_No")]
    public string Document_No { get; set; }

    [JsonProperty("Line_No")]
    public int Line_No { get; set; }

    [JsonProperty("Type")]
    public string Type { get; set; }

    [JsonProperty("No")]
    public string No { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Quantity")]
    public decimal Quantity { get; set; }

    [JsonProperty("Unit_of_Measure_Code")]
    public string Unit_of_Measure_Code { get; set; }

    [JsonProperty("Unit_Price")]
    public decimal Unit_Price { get; set; }

    [JsonProperty("Line_Amount")]
    public decimal Line_Amount { get; set; }

    [JsonProperty("Line_Discount_Percent")]
    public decimal Line_Discount_Percent { get; set; }

    [JsonProperty("Line_Discount_Amount")]
    public decimal Line_Discount_Amount { get; set; }
}

public class D365SalesLinesResponse
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; }

    [JsonProperty("value")]
    public List<D365BCSalesLineModel> Value { get; set; }
}
