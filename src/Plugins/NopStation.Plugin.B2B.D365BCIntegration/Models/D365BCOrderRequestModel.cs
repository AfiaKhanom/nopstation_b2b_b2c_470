using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.D365BCIntegration.Models;
public class D365BCOrderRequestModel
{
    [JsonProperty("Document_Type")]
    public string Document_Type { get; set; } = "Order";

    [JsonProperty("Sell_to_Customer_No")]
    public string Sell_to_Customer_No { get; set; }

    [JsonProperty("Posting_Description")]
    public string Posting_Description { get; set; }

    [JsonProperty("Document_Date")]
    public string Document_Date { get; set; }

    [JsonProperty("Posting_Date")]
    public string Posting_Date { get; set; }

    [JsonProperty("Order_Date")]
    public string Order_Date { get; set; }

    [JsonProperty("Due_Date")]
    public string Due_Date { get; set; }

    [JsonProperty("Requested_Delivery_Date")]
    public string Requested_Delivery_Date { get; set; }

    [JsonProperty("Your_Reference")]
    public string Your_Reference { get; set; }

    [JsonProperty("Salesperson_Code")]
    public string Salesperson_Code { get; set; }

    [JsonProperty("Payment_Terms_Code")]
    public string Payment_Terms_Code { get; set; }

    [JsonProperty("Shipment_Method_Code")]
    public string Shipment_Method_Code { get; set; }

    [JsonProperty("Shipping_Agent_Code")]
    public string Shipping_Agent_Code { get; set; }

    [JsonProperty("Shipping_Agent_Service_Code")]
    public string Shipping_Agent_Service_Code { get; set; }

    [JsonProperty("Package_Tracking_No")]
    public string Package_Tracking_No { get; set; }

    [JsonProperty("Location_Code")]
    public string Location_Code { get; set; }

    [JsonProperty("Shipping_Advice")]
    public string Shipping_Advice { get; set; }

    [JsonProperty("Shipping_Time")]
    public string Shipping_Time { get; set; }

    [JsonProperty("Language_Code")]
    public string Language_Code { get; set; }
}