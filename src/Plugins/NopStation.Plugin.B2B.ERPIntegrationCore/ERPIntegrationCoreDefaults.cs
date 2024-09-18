using Nop.Core.Caching;

namespace NopStation.Plugin.B2B.ERPIntegrationCore
{
    public static class ERPIntegrationCoreDefaults
    {
        public static string B2BCustomerRole => "B2BUser";
        public static string B2CCustomerRole => "B2CUser";
        public static string B2BB2CAdminRole => "B2B-B2C Admin";
        public static string B2BB2CAdminRoleSystemName => "B2BB2CAdmin";
        public static string B2BOrderAssistantRoleSystemName => "B2BOrderAssistant";
        public static string B2BQuoteAssistantRoleSystemName => "B2BQuoteAssistant";
        public static string B2BCustomerAccountingPersonnelRoleSystemName => "B2BCustomerAccountingPersonnel";
        public static string B2BSalesRepRoleSystemName => "ERPSalesRep";

        public static string QuickOrderUserRoleSystemName = "QuickOrderUser";
        public static string ErpGroupPriceCodeInsert => "GroupPriceCodeInserted";
        public static string ErpGroupPriceCodeUpdate => "GroupPriceCodeUpdated";
        public static string ErpGroupPriceCodeDelete => "GroupPriceCodeDeleted";
        public static string ErpSpecialPriceInsert => "SpecialPriceInserted";
        public static string ErpSpecialPriceUpdate => "SpecialPriceUpdated";
        public static string ErpSpecialPriceDelete => "SpecialPriceDeleted";
        public static string ErpNopUserAccountMapInsert => "ErpNopUserAccountMapInserted";
        public static string ErpPriceGroupProductPricingCacheKey => "ErpPriceGroupProductPricingCacheKey";
        public static CacheKey ErpProductSpecificationAttributeList => new CacheKey("NopB2bB2cFeaturesAdminProductSpecificationAttributeForB2b");

        public static string ErpAccountByCustomerCacheKey => "ERP.Account.customer-{0}-{1}";
        public static string ERPIntegrationPluginGroupName => "Nopstation_ErpIntegration";

        public static CacheKey SalesRepOrgCacheKey => new("Nop.salesrep.salesreporgs.{0}-{1}", SalesRepOrgBySalesRepPrefix, SalesRepOrgPrefix);
        public static CacheKey ShipToAddressCacheKey => new("Nop.Account.ShiptoAddress.{0}-{1}", ErpAccountPrefix, ShiptoAddressesByAccountrPrefix);
        public static string ErpAccountPrefix => "B2B.ErpAccount."; 
        public static string ShiptoAddressesByAccountrPrefix => "Nop.account.shiptoaddresses.{0}";

        #region Product Pricing

        /// <summary>
        /// Gets a key for ERP product Special Price
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// </remarks>
        public static CacheKey ErpProductPricingSpecialPriceByProductIdCacheKey => new("Erp.Product.Pricing.SpecialPrice.{0}", ErpProductPricingPrefix);

        /// <summary>
        /// Gets a key for ERP product Special Price
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : account id
        /// </remarks>
        public static CacheKey ErpProductPricingSpecialPriceByProductIdAndAccountIdCacheKey => new("Erp.product.pricing.specialprice.{0}-{1}", ErpProductPricingPrefix);

        /// <summary>
        /// Gets a key for ERP product Group Price
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// </remarks>
        public static CacheKey ErpProductPricingGroupPriceByProductIdCacheKey => new("Erp.product.pricing.groupprice.{0}", ErpProductPricingPrefix);

        /// <summary>
        /// Gets a key for ERP product Group Price
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : price group id
        /// </remarks>
        public static CacheKey ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey => new("Erp.product.pricing.groupprice.{0}-{1}", ErpProductPricingPrefix);
        
        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ErpProductPricingPrefix => "Erp.product.pricing.";

        #endregion

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string SalesRepOrgPrefix => "Nop.salesrep.salesreporgs.";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// </remarks>
        public static string SalesRepOrgBySalesRepPrefix => "Nop.salesrep.salesreporgs.{0}";

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
    }
}