using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

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
        { typeof(ErpShiptoAddressErpAccountMap), "Erp_ShiptoAddress_Erp_Account_Map" },
        { typeof(ErpAccountCustomerRegistrationForm), "Erp_Account_CustomerRegistrationForm" },
        { typeof(ErpAccountCustomerRegistrationBankingDetails), "Erp_Account_CustomerRegistration_BankingDetails" },
        { typeof(ErpAccountCustomerRegistrationPhysicalTradingAddress), "Erp_Account_CustomerRegistration_PhysicalTradingAddress" },
        { typeof(ErpAccountCustomerRegistrationPremises), "Erp_Account_CustomerRegistration_Premises" },
        { typeof(ErpAccountCustomerRegistrationTradeReferences), "Erp_Account_CustomerRegistration_TradeReferences" }
    };

    public Dictionary<(Type, string), string> ColumnName => new()
    {
        
    };
}
