using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(ErpAccount), "Erp_Account" },
            { typeof(ErpGroupPrice), "Erp_Group_Price" },
            { typeof(ErpGroupPriceCode), "Erp_Group_Price_Code" },
            { typeof(ErpInvoice), "Erp_Invoice" },
            { typeof(ErpNopUser), "Erp_Nop_User" },
            { typeof(ErpNopUserAccountMap), "Erp_Nop_User_Account_Map" },
            { typeof(ErpOrderAdditionalData), "Erp_Order_Additional_Data" },
            { typeof(ErpOrderItemAdditionalData), "Erp_Order_Item_Additional_Data" },
            { typeof(ErpSalesOrg), "Erp_Sales_Org" },
            { typeof(ErpSalesRep), "Erp_Sales_Rep" },
            { typeof(ErpSalesRepSalesOrgMap), "Erp_Sales_Rep_Sales_Org_Map" },
            { typeof(ErpSalesRepErpAccountMap), "Erp_Sales_Rep_Erp_Account_Map" },
            { typeof(ErpShipToAddress), "Erp_ShipToAddress" },
            { typeof(ErpSpecialPrice), "Erp_Special_Price" },
            { typeof(ErpWarehouseAdditionalData), "Erp_Warehouse_Additional_Data" },
            { typeof(ErpWarehouseSalesOrgMap), "Erp_Warehouse_Sales_Org_Map" },
            { typeof(ErpLogs), "Erp_Logs" },
            { typeof(QuickOrderTemplate), "Erp_Quick_Order_Template" },
            { typeof(QuickOrderItem), "Erp_Quick_Order_Item" },
            { typeof(ErpActivityLogs), "Erp_Activity_Logs" },
            { typeof(ErpShiptoAddressErpAccountMap), "Erp_ShiptoAddress_Erp_Account_Map" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
            { (typeof(ErpAccount),"ErpSalesOrgId"), "ErpSalesOrg_Id" },
            { (typeof(ErpAccount),"BillingAddressId"), "BillingAddress_Id" },
            { (typeof(ErpGroupPrice),"ErpNopGroupPriceCodeId"), "ErpNopGroupPriceCode_Id" },
            { (typeof(ErpGroupPrice),"NopProductId"), "NopProduct_Id" },
            { (typeof(ErpNopUser),"NopCustomerId"), "NopCustomer_Id" },
            { (typeof(ErpNopUser),"ErpAccountId"), "ErpAccount_Id" },
            { (typeof(ErpNopUser),"ErpShipToAddressId"), "ErpShipToAddress_Id" },
            { (typeof(ErpNopUser),"BillingErpShipToAddressId"), "BillingErpShipToAddress_Id" },
            { (typeof(ErpNopUser),"ShippingErpShipToAddressId"), "ShippingErpShipToAddress_Id" },
            { (typeof(ErpNopUserAccountMap),"ErpAccountId"), "ErpAccount_Id" },
            { (typeof(ErpNopUserAccountMap),"ErpUserId"), "ErpUser_Id" },
            { (typeof(ErpNopUserAccountMap),"CustomerRolesIds"), "CustomerRoles_Ids" },
            { (typeof(ErpOrderAdditionalData),"NopOrderId"), "NopOrder_Id" },
            { (typeof(ErpOrderAdditionalData),"OrderPlacedByNopCustomerId"), "OrderPlacedByNopCustomer_Id" },
            { (typeof(ErpOrderAdditionalData),"ErpAccountId"), "ErpAccount_Id" },
            { (typeof(ErpOrderAdditionalData),"QuoteSalesOrderId"), "QuoteSalesOrder_Id" },
            { (typeof(ErpOrderAdditionalData),"ErpShipToAddressId"), "ErpShipToAddress_Id" },
            { (typeof(ErpOrderItemAdditionalData),"NopOrderItemId"), "NopOrderItem_Id" },
            { (typeof(ErpOrderItemAdditionalData),"ErpOrderId"), "ErpOrder_Id" },
            { (typeof(ErpSalesOrg),"AddressId"), "Address_Id" },
            { (typeof(ErpSalesRep),"NopCustomerId"), "NopCustomer_Id" },
            { (typeof(ErpSalesRepSalesOrgMap),"ErpSalesRepId"), "ErpSalesRep_Id" },
            { (typeof(ErpSalesRepSalesOrgMap),"ErpSalesOrgId"), "ErpSalesOrg_Id" },
            { (typeof(ErpSalesRepErpAccountMap),"ErpSalesRepId"), "ErpSalesRep_Id" },
            { (typeof(ErpSalesRepErpAccountMap),"ErpAccountId"), "ErpAccount_Id" }, 
            { (typeof(ErpShipToAddress),"AddressId"), "Address_Id" },
            { (typeof(ErpSpecialPrice),"ErpAccountId"), "ErpAccount_Id" },
            { (typeof(ErpWarehouseAdditionalData),"NopProductId"), "NopProduct_Id" },
            { (typeof(ErpWarehouseSalesOrgMap),"ErpSalesOrgId"),"ErpSalesOrg_Id" },
            { (typeof(ErpWarehouseSalesOrgMap),"ErpWarehouseId"),"ErpWarehouse_Id" },


            // Quick Order 
            { (typeof(QuickOrderTemplate),"CustomerId"), "Customer_Id" },
            { (typeof(QuickOrderTemplate),"LastOrderDate"), "Last_Order_Date" },
            { (typeof(QuickOrderTemplate),"IsDeleted"), "Is_Deleted" },
            { (typeof(QuickOrderTemplate),"EditedOnUtc"), "Edited_On_Utc" },
            { (typeof(QuickOrderTemplate),"CreatedOnUtc"), "Created_On_Utc" },
            { (typeof(QuickOrderTemplate),"LastPriceCalculatedOnUtc"), "Last_Price_Calculated_On_Utc" },


            { (typeof(QuickOrderItem),"QuickOrderTemplateId"), "Quick_Order_Template_Id" },
            { (typeof(QuickOrderItem),"ProductSku"), "Product_Sku" },
            { (typeof(QuickOrderItem),"AttributesXml"), "Attributes_Xml" },

            { (typeof(ErpShiptoAddressErpAccountMap),"ErpShiptoAddressId"), "ErpShiptoAddress_Id" },
            { (typeof(ErpShiptoAddressErpAccountMap),"ErpAccountId"), "ErpAccount_Id" },
        };
    }
}
