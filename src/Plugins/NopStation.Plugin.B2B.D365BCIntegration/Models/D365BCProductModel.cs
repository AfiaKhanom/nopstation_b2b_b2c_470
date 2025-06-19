using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.D365BCIntegration.Models;
public class D365BCProductModel
{
    [JsonProperty("@odata.etag")]
    public string ODataEtag { get; set; }

    [JsonProperty("No")]
    public string No { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Description_2")]
    public string Description_2 { get; set; }

    [JsonProperty("Type")]
    public string Type { get; set; }

    [JsonProperty("InventoryField")]
    public decimal InventoryField { get; set; }

    [JsonProperty("Created_From_Nonstock_Item")]
    public bool Created_From_Nonstock_Item { get; set; }

    [JsonProperty("Substitutes_Exist")]
    public bool Substitutes_Exist { get; set; }

    [JsonProperty("Stockkeeping_Unit_Exists")]
    public bool Stockkeeping_Unit_Exists { get; set; }

    [JsonProperty("Assembly_BOM")]
    public bool Assembly_BOM { get; set; }

    [JsonProperty("Production_BOM_No")]
    public string Production_BOM_No { get; set; }

    [JsonProperty("Routing_No")]
    public string Routing_No { get; set; }

    [JsonProperty("Base_Unit_of_Measure")]
    public string Base_Unit_of_Measure { get; set; }

    [JsonProperty("Shelf_No")]
    public string Shelf_No { get; set; }

    [JsonProperty("Costing_Method")]
    public string Costing_Method { get; set; }

    [JsonProperty("Cost_is_Adjusted")]
    public bool Cost_is_Adjusted { get; set; }

    [JsonProperty("Standard_Cost")]
    public decimal Standard_Cost { get; set; }

    [JsonProperty("Unit_Cost")]
    public decimal Unit_Cost { get; set; }

    [JsonProperty("Last_Direct_Cost")]
    public decimal Last_Direct_Cost { get; set; }

    [JsonProperty("Price_Profit_Calculation")]
    public string Price_Profit_Calculation { get; set; }

    [JsonProperty("Profit_Percent")]
    public decimal Profit_Percent { get; set; }

    [JsonProperty("Unit_Price")]
    public decimal Unit_Price { get; set; }

    [JsonProperty("Inventory_Posting_Group")]
    public string Inventory_Posting_Group { get; set; }

    [JsonProperty("Gen_Prod_Posting_Group")]
    public string Gen_Prod_Posting_Group { get; set; }

    [JsonProperty("VAT_Prod_Posting_Group")]
    public string VAT_Prod_Posting_Group { get; set; }

    [JsonProperty("Item_Disc_Group")]
    public string Item_Disc_Group { get; set; }

    [JsonProperty("Vendor_No")]
    public string Vendor_No { get; set; }

    [JsonProperty("Vendor_Item_No")]
    public string Vendor_Item_No { get; set; }

    [JsonProperty("Tariff_No")]
    public string Tariff_No { get; set; }

    [JsonProperty("Search_Description")]
    public string Search_Description { get; set; }

    [JsonProperty("Overhead_Rate")]
    public decimal Overhead_Rate { get; set; }

    [JsonProperty("Indirect_Cost_Percent")]
    public decimal Indirect_Cost_Percent { get; set; }

    [JsonProperty("Item_Category_Code")]
    public string Item_Category_Code { get; set; }

    [JsonProperty("Blocked")]
    public bool Blocked { get; set; }

    [JsonProperty("Last_Date_Modified")]
    public string Last_Date_Modified { get; set; }

    [JsonProperty("Sales_Unit_of_Measure")]
    public string Sales_Unit_of_Measure { get; set; }

    [JsonProperty("Replenishment_System")]
    public string Replenishment_System { get; set; }

    [JsonProperty("Purch_Unit_of_Measure")]
    public string Purch_Unit_of_Measure { get; set; }

    [JsonProperty("Lead_Time_Calculation")]
    public string Lead_Time_Calculation { get; set; }

    [JsonProperty("Manufacturing_Policy")]
    public string Manufacturing_Policy { get; set; }

    [JsonProperty("Flushing_Method")]
    public string Flushing_Method { get; set; }

    [JsonProperty("Assembly_Policy")]
    public string Assembly_Policy { get; set; }

    [JsonProperty("Item_Tracking_Code")]
    public string Item_Tracking_Code { get; set; }

    [JsonProperty("Default_Deferral_Template_Code")]
    public string Default_Deferral_Template_Code { get; set; }

    [JsonProperty("Coupled_to_CRM")]
    public bool Coupled_to_CRM { get; set; }

    [JsonProperty("Coupled_to_Dataverse")]
    public bool Coupled_to_Dataverse { get; set; }

    [JsonProperty("GTIN")]
    public string GTIN { get; set; }

    [JsonProperty("Global_Dimension_1_Filter")]
    public string Global_Dimension_1_Filter { get; set; }

    [JsonProperty("Global_Dimension_2_Filter")]
    public string Global_Dimension_2_Filter { get; set; }

    [JsonProperty("Location_Filter")]
    public string Location_Filter { get; set; }

    [JsonProperty("Drop_Shipment_Filter")]
    public string Drop_Shipment_Filter { get; set; }

    [JsonProperty("Variant_Filter")]
    public string Variant_Filter { get; set; }

    [JsonProperty("Lot_No_Filter")]
    public string Lot_No_Filter { get; set; }

    [JsonProperty("Serial_No_Filter")]
    public string Serial_No_Filter { get; set; }

    [JsonProperty("Unit_of_Measure_Filter")]
    public string Unit_of_Measure_Filter { get; set; }

    [JsonProperty("Package_No_Filter")]
    public string Package_No_Filter { get; set; }
}

public class D365ProductsResponse
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; }

    [JsonProperty("value")]
    public List<D365BCProductModel> Value { get; set; }
}