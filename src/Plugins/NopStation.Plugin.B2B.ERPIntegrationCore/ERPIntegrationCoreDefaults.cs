using Nop.Core.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore;

public static class ERPIntegrationCoreDefaults
{
    #region B2B-B2C Customer Roles

    public static string B2BCustomerRole => "B2B Customer";
    public static string B2BCustomerRoleSystemName => "B2BCustomer";
    public static string B2CCustomerRole => "B2C Customer";
    public static string B2CCustomerRoleSystemName => "B2CCustomer";
    public static string B2BQuoteAssistantRole => "B2B Quote Assistant";
    public static string B2BQuoteAssistantRoleSystemName => "B2BQuoteAssistant";
    public static string B2BOrderAssistantRole => "B2B Order Assistant";
    public static string B2BOrderAssistantRoleSystemName => "B2BOrderAssistant";
    public static string B2BB2CAdminRole => "B2B-B2C Admin";
    public static string B2BB2CAdminRoleSystemName => "B2BB2CAdmin";
    public static string B2BCustomerAccountingPersonnelRole => "B2B Customer Accounting Personnel";
    public static string B2BCustomerAccountingPersonnelRoleSystemName => "B2BCustomerAccountingPersonnel";
    public static string B2BSalesRepRole => "B2B Sales Rep";
    public static string B2BSalesRepRoleSystemName => "B2BSalesRep";
    public static string QuickOrderUserRole => "Quick Order User";
    public static string QuickOrderUserRoleSystemName => "QuickOrderUser";

    #endregion

    #region Cache keys and Prefixes

    public static CacheKey ErpProductSpecificationAttributeList => new("NopB2bB2cFeaturesAdminProductSpecificationAttributeForB2b");
    public static string ErpAccountPrefix => "B2B.ErpAccount.";
    public static string ShiptoAddressesByAccountrPrefix => "Nop.account.shiptoaddresses.{0}";
    public static CacheKey ShipToAddressCacheKey => new("Nop.Account.ShiptoAddress.{0}-{1}", ErpAccountPrefix, ShiptoAddressesByAccountrPrefix);

    #region ERP Nop user Cache key

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// </remarks>
    public static CacheKey ErpNopUserByCustomerCacheKey => new("Nop.erpnopuser.bycustomer.{0}", ErpNopUserByCustomerPrefixCacheKey);

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// {1} : Erp Account Id
    /// </remarks>
    public static CacheKey ErpNopUserByCustomerAndErpAccountCacheKey => new("Nop.erpnopuser.bycustomer.{0}-{1}", ErpNopUserByCustomerPrefixCacheKey);

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string ErpNopUserByCustomerPrefixCacheKey => "Nop.erpnopuser.bycustomer.";

    #endregion

    #region ERP Nop user Account Map Cache key

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : ErpUserId
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpUserCacheKey => new("Nop.erpnopuseraccountmap.byerpuser.{0}");

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpAccountCacheKey => new("Nop.erpnopuseraccountmap.byerpaccount.{0}", NopEntityCacheDefaults<ErpNopUserAccountMap>.Prefix);

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account Id
    /// {0} : erp user
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpAccountAndErpUserCacheKey => new("Nop.erpnopuseraccountmap.byerpaccount.{0}-{1}", NopEntityCacheDefaults<ErpNopUserAccountMap>.Prefix);

    #endregion

    #region ERP Account Cache key

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// {1} : roles of the current customer
    /// </remarks>
    public static CacheKey ErpAccountByCustomerCacheKey => new("Nop.erpaccount.bycustomer.{0}-{1}", ErpAccountByCustomerPrefixCacheKey);

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string ErpAccountByCustomerPrefixCacheKey => "Nop.erpaccount.bycustomer.{0}";

    #endregion

    #region Product Pricing Cache Key

    /// <summary>
    /// Gets a key for ERP product Special Price
    /// </summary>
    /// <remarks>
    /// {0} : product id
    /// </remarks>
    public static CacheKey ErpProductPricingSpecialPriceByProductCacheKey => new("Nop.totals.erpproductpricing.specialprice.byproduct.{0}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for ERP product Special Price
    /// </summary>
    /// <remarks>
    /// {0} : product id
    /// {1} : account id
    /// </remarks>
    public static CacheKey ErpProductPricingSpecialPriceByProductIdAndAccountCacheKey => new("Nop.totals.erpproductpricing.specialprice.byproduct.{0}-{1}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for ERP product Group Price
    /// </summary>
    /// <remarks>
    /// {0} : product id
    /// </remarks>
    public static CacheKey ErpProductPricingGroupPriceByProductIdCacheKey => new("Nop.totals.erpproductpricing.groupprice.byproduct.{0}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for ERP product Group Price
    /// </summary>
    /// <remarks>
    /// {0} : product id
    /// {1} : price group id
    /// </remarks>
    public static CacheKey ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey => new("Nop.totals.erpproductpricing.groupprice.byproduct.{0}-{1}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string ErpProductPricingPrefix => "Nop.totals.erpproductpricing.";

    #endregion

    #region ERP Account Cache keys and Prefixes

    // Prefixes
    public static string ErpAccountByIdPrefixCacheKey => "Nop.erpaccount.byid.";
    public static string ErpAccountByErpShipToAddressPrefixCacheKey => "Nop.erpaccount.byshiptoaddress.";
    public static string ErpSalesRepErpAccountMapByIdsPrefixCacheKey => "Nop.erpsalesreperpaccountmap.byids.";
    public static string ErpSalesRepErpAccountMapBySalesRepIdPrefixCacheKey => "Nop.erpsalesreperpaccountmap.bysalesrepid.";
    public static string ErpAccountByIdWithActivePrefixCacheKey => "Nop.erpaccount.byidwithactive.";
    public static string ErpAccountAllActivePrefixCacheKey => "Nop.erpaccount.allactive.";
    public static string ErpAccountPagedPrefixCacheKey => "Nop.erpaccount.paged.";
    public static string ErpAccountListPrefixCacheKey => "Nop.erpaccount.list.";
    public static string ErpAccountPagedByIdsPrefixCacheKey => "Nop.erpaccount.pagedbyids.";
    public static string ErpAccountOfActiveNopUsersPrefixCacheKey => "Nop.erpaccount.ofactivenopusers.";
    public static string ErpAccountByAccountNumberPrefixCacheKey => "Nop.erpaccount.byaccountnumber.";
    public static string ErpAccountByCustomerIdPrefixCacheKey => "Nop.erpaccount.bycustomerid.";
    public static string ErpAccountBySalesOrgIdPrefixCacheKey => "Nop.erpaccount.bysalesorgid.";

    // CacheKeys
    public static CacheKey ErpAccountByIdCacheKey => new("Nop.erpaccount.byid.{0}-{1}", ErpAccountByIdPrefixCacheKey);
    public static CacheKey ErpAccountByErpShipToAddressCacheKey => new("Nop.erpaccount.byshiptoaddress.{0}", ErpAccountByErpShipToAddressPrefixCacheKey);
    public static CacheKey ErpSalesRepErpAccountMapByIdsCacheKey => new("Nop.erpsalesreperpaccountmap.byids.{0}-{1}", ErpSalesRepErpAccountMapByIdsPrefixCacheKey);
    public static CacheKey ErpSalesRepErpAccountMapBySalesRepIdCacheKey => new("Nop.erpsalesreperpaccountmap.bysalesrepid.{0}", ErpSalesRepErpAccountMapBySalesRepIdPrefixCacheKey);
    public static CacheKey ErpAccountByIdWithActiveCacheKey => new("Nop.erpaccount.byidwithactive.{0}", ErpAccountByIdWithActivePrefixCacheKey);
    public static CacheKey ErpAccountAllActiveCacheKey => new("Nop.erpaccount.allactive", ErpAccountAllActivePrefixCacheKey);
    public static CacheKey ErpAccountPagedCacheKey => new("Nop.erpaccount.paged.{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}", ErpAccountPagedPrefixCacheKey);
    public static CacheKey ErpAccountListCacheKey => new("Nop.erpaccount.list.{0}-{1}-{2}-{3}-{4}-{5}-{6}", ErpAccountListPrefixCacheKey);
    public static CacheKey ErpAccountPagedByIdsCacheKey => new("Nop.erpaccount.pagedbyids.{0}-{1}-{2}-{3}-{4}-{5}", ErpAccountPagedByIdsPrefixCacheKey);
    public static CacheKey ErpAccountOfActiveNopUsersCacheKey => new("Nop.erpaccount.ofactivenopusers.{0}-{1}", ErpAccountOfActiveNopUsersPrefixCacheKey);
    public static CacheKey ErpAccountByAccountNumberCacheKey => new("Nop.erpaccount.byaccountnumber.{0}", ErpAccountByAccountNumberPrefixCacheKey);
    public static CacheKey ErpAccountByCustomerIdCacheKey => new("Nop.erpaccount.bycustomerid.{0}", ErpAccountByCustomerIdPrefixCacheKey);
    public static CacheKey ErpAccountBySalesOrgIdCacheKey => new("Nop.erpaccount.bysalesorgid.{0}", ErpAccountBySalesOrgIdPrefixCacheKey);

    #endregion

    public static string SalesRepOrgPrefix => "Nop.salesrep.salesreporgs.";

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    /// <remarks>
    /// {0} : customer identifier
    /// </remarks>
    public static string SalesRepOrgBySalesRepPrefix => "Nop.salesrep.salesreporgs.{0}";
    public static CacheKey SalesRepOrgCacheKey => new("Nop.salesrep.salesreporgs.{0}-{1}", SalesRepOrgBySalesRepPrefix, SalesRepOrgPrefix);  
    public static CacheKey ErpWarehouseSalesOrgMapByCodeCacheKey => new("ERPIntegration.warehouse.salesOrgMap.byWarehouseCode-{0}", ErpWarehouseSalesOrgMapByCodePrefix);
    public static string ErpWarehouseSalesOrgMapByCodePrefix => "ERPwarehouse.salesOrgMap.ByWarehouseCode";

    #endregion

    #region ERP Order Type

    public static string ErpB2BOrderType => "ORDER";
    public static string ErpB2BOrderFromQuoteType => "ORDER FROM QUOTE";
    public static string ErpB2COrderType => "B2CORDER";
    public static string ErpB2COrderFromQuoteType => "ORDER FROM B2C QUOTE";
    public static string ErpB2BQuoteType => "QUOTE";
    public static string ErpB2CQuoteType => "B2CQuote";

    #endregion

    #region ERP Order Status

    public static string ERPOrderStatusApproved => "Approved";
    public static string ERPOrderStatusPendingApproval => "Pending Approval";
    public static string ERPOrderStatusProcessing => "Processing";

    #endregion

    public static string NewB2BCustomerNeedsApproval => "NewB2BCustomerNeedsApproval";
    public static string ERPIntegrationPluginGroupName => "Nopstation_ErpIntegration";
}