
using Newtonsoft.Json;
namespace NopStation.Plugin.B2B.D365BCIntegration.Models;

public class D365BCOrderModel
{
    [JsonProperty("@odata.etag")]
    public string ODataEtag { get; set; }

    // Order Header Information
    [JsonProperty("Document_Type")]
    public string Document_Type { get; set; }

    [JsonProperty("No")]
    public string No { get; set; }

    [JsonProperty("Order_Date")]
    public string Order_Date { get; set; }

    [JsonProperty("Status")]
    public string Status { get; set; }

    [JsonProperty("Payment_Terms_Code")]
    public string Payment_Terms_Code { get; set; }

    [JsonProperty("Due_Date")]
    public string Due_Date { get; set; }

    // Customer Information
    [JsonProperty("Sell_to_Customer_No")]
    public string Sell_to_Customer_No { get; set; }

    [JsonProperty("Sell_to_Customer_Name")]
    public string Sell_to_Customer_Name { get; set; }

    [JsonProperty("Sell_to_Address")]
    public string Sell_to_Address { get; set; }

    [JsonProperty("Sell_to_Address_2")]
    public string Sell_to_Address_2 { get; set; }

    [JsonProperty("Sell_to_City")]
    public string Sell_to_City { get; set; }

    [JsonProperty("Sell_to_County")]
    public string Sell_to_County { get; set; }

    [JsonProperty("Sell_to_Post_Code")]
    public string Sell_to_Post_Code { get; set; }

    [JsonProperty("Sell_to_Country_Region_Code")]
    public string Sell_to_Country_Region_Code { get; set; }

    [JsonProperty("Sell_to_Contact")]
    public string Sell_to_Contact { get; set; }

    [JsonProperty("Sell_to_Phone_No")]
    public string Sell_to_Phone_No { get; set; }

    [JsonProperty("Sell_to_E_Mail")]
    public string Sell_to_E_Mail { get; set; }

    // Billing Information
    [JsonProperty("Bill_to_Customer_No")]
    public string Bill_to_Customer_No { get; set; }

    [JsonProperty("Bill_to_Name")]
    public string Bill_to_Name { get; set; }

    [JsonProperty("Bill_to_Address")]
    public string Bill_to_Address { get; set; }

    [JsonProperty("Bill_to_Address_2")]
    public string Bill_to_Address_2 { get; set; }

    [JsonProperty("Bill_to_City")]
    public string Bill_to_City { get; set; }

    [JsonProperty("Bill_to_County")]
    public string Bill_to_County { get; set; }

    [JsonProperty("Bill_to_Post_Code")]
    public string Bill_to_Post_Code { get; set; }

    [JsonProperty("Bill_to_Country_Region_Code")]
    public string Bill_to_Country_Region_Code { get; set; }

    [JsonProperty("Bill_to_Contact")]
    public string Bill_to_Contact { get; set; }

    [JsonProperty("BillToContactPhoneNo")]
    public string BillToContactPhoneNo { get; set; }

    // Shipping Information
    [JsonProperty("Ship_to_Code")]
    public string Ship_to_Code { get; set; }

    [JsonProperty("Ship_to_Name")]
    public string Ship_to_Name { get; set; }

    [JsonProperty("Ship_to_Address")]
    public string Ship_to_Address { get; set; }

    [JsonProperty("Ship_to_Address_2")]
    public string Ship_to_Address_2 { get; set; }

    [JsonProperty("Ship_to_City")]
    public string Ship_to_City { get; set; }

    [JsonProperty("Ship_to_County")]
    public string Ship_to_County { get; set; }

    [JsonProperty("Ship_to_Post_Code")]
    public string Ship_to_Post_Code { get; set; }

    [JsonProperty("Ship_to_Country_Region_Code")]
    public string Ship_to_Country_Region_Code { get; set; }

    [JsonProperty("Ship_to_Contact")]
    public string Ship_to_Contact { get; set; }

    [JsonProperty("Ship_to_Phone_No")]
    public string Ship_to_Phone_No { get; set; }

    // Financial Information
    [JsonProperty("Currency_Code")]
    public string Currency_Code { get; set; }

    [JsonProperty("Amount")]
    public decimal Amount { get; set; }

    [JsonProperty("Amount_Including_VAT")]
    public decimal Amount_Including_VAT { get; set; }

    [JsonProperty("VAT_Amount")]
    public decimal VAT_Amount { get; set; }

    // Shipping Details
    [JsonProperty("Shipment_Method_Code")]
    public string Shipment_Method_Code { get; set; }

    [JsonProperty("Shipping_Agent_Code")]
    public string Shipping_Agent_Code { get; set; }

    [JsonProperty("Shipping_Agent_Service_Code")]
    public string Shipping_Agent_Service_Code { get; set; }

    [JsonProperty("Package_Tracking_No")]
    public string Package_Tracking_No { get; set; }

    // Dates
    [JsonProperty("Requested_Delivery_Date")]
    public string Requested_Delivery_Date { get; set; }

    [JsonProperty("Promised_Delivery_Date")]
    public string Promised_Delivery_Date { get; set; }

    [JsonProperty("Shipping_Date")]
    public string Shipping_Date { get; set; }

    // Additional Information
    [JsonProperty("External_Document_No")]
    public string External_Document_No { get; set; }

    [JsonProperty("Your_Reference")]
    public string Your_Reference { get; set; }

    [JsonProperty("Salesperson_Code")]
    public string Salesperson_Code { get; set; }

    [JsonProperty("Location_Code")]
    public string Location_Code { get; set; }

    [JsonProperty("Shortcut_Dimension_1_Code")]
    public string Shortcut_Dimension_1_Code { get; set; }

    [JsonProperty("Shortcut_Dimension_2_Code")]
    public string Shortcut_Dimension_2_Code { get; set; }

    [JsonProperty("Payment_Method_Code")]
    public string Payment_Method_Code { get; set; }

    [JsonProperty("Transaction_Type")]
    public string Transaction_Type { get; set; }

    [JsonProperty("Transport_Method")]
    public string Transport_Method { get; set; }

    [JsonProperty("Exit_Point")]
    public string Exit_Point { get; set; }

    [JsonProperty("Area")]
    public string Area { get; set; }

    // System Fields
    [JsonProperty("Last_Modified_Date_Time")]
    public string Last_Modified_Date_Time { get; set; }

    [JsonProperty("Created_Date_Time")]
    public string Created_Date_Time { get; set; }
    [JsonProperty("OrderLines")]
    public List<D365BCSalesLineModel> OrderLines { get; set; } = new List<D365BCSalesLineModel>();
}

public class D365OrdersResponse
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; }

    [JsonProperty("@odata.count")]
    public int? Count { get; set; }

    [JsonProperty("value")]
    public List<D365BCOrderModel> Value { get; set; }

    [JsonProperty("@odata.nextLink")]
    public string NextLink { get; set; }
}

