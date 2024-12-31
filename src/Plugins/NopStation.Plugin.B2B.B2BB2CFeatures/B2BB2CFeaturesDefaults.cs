using Nop.Core.Caching;

namespace NopStation.Plugin.B2B.B2BB2CFeatures;

public static class B2BB2CFeaturesDefaults
{
    /// <summary>
    /// Gets a path to the file that contains Resource string xml File
    /// </summary>
    public static string XmlResourceStringFilePath => "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/ResourceString/B2BB2CFeatures.Resources.en-us.xml";
    public static string ErpPriceGroupProductPricingInsert => "ErpPriceGroupProductPricingInsert";
    public static string ErpPriceGroupProductPricingUpdate => "ErpPriceGroupProductPricingUpdate";
    public static string ErpPriceGroupProductPricingDelete => "ErpPriceGroupProductPricingDelete";
    public static string ErpDateFormatForDataTable => "DD/MM/YYYY";
    public static string ErpDateFormatForPublicInputField => "mm/dd/yy";
    public static string IsCartActivityOn => "IsCartActivityOn";
    public static string ErpQuoteAssistantRoleSystemName => "ErpQuoteAssistant";

    # region Erp Customer Navigation

    public static int ErpCustomerNavigationEnum_ErpCompareValue => 1000;
    public static int ErpCustomerNavigationEnum_ErpAccountInfo => 1010;
    public static int ErpCustomerNavigationEnum_ErpAccountOrders => 1020;
    public static int ErpCustomerNavigationEnum_ErpAccountQuoteOrders => 1030;
    public static int ErpCustomerNavigationEnum_ErpBillingAddresses => 1040;
    public static int ErpCustomerNavigationEnum_ErpShippingAddresses => 1050;
    public static int ErpCustomerNavigationEnum_ErpCustomerConfiguration => 1060;
    public static int ErpCustomerNavigationEnum_ErpAccountTransactionsInfo => 1070;

    #endregion

    #region View components

    public static string ZoneAfterTirePriceCard => "zone_after_tire_price_card";
    public static string ZoneAfterSpecialPriceCard => "zone_after_special_price_card";
    public static string ErpAdminWidgetZonesOrderDetailsBlock => "admin_erp_order_details_block";

    #endregion

    public static string B2BConvertedQuoteB2BOrderId => "B2BConvertedQuoteB2BOrderId";
    public static string B2CConvertedQuoteB2COrderId => "B2CConvertedQuoteB2COrderId";

    public static string B2BOriginalB2BQuoteOrderIdReference => "B2BOriginalB2BQuoteOrderIdReference";
    public static string B2COriginalB2CQuoteOrderIdReference => "B2COriginalB2CQuoteOrderIdReference";

    public static string CartItemsLivePriceSyncProcessing => "CartItemsLivePriceSyncProcessing";

    public static string B2BCustomerRoleSystemName => "B2BCustomer";
    public static string CustomerLastDateOfDisplayB2BPriceSyncInfo => "LastDateOfDisplayB2BPriceSyncInfo";
    public static string CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo => "LastDateOfDisplayB2BPriceGroupPriceSyncInfo";

    public static string ProvidedB2BCustomerReferenceAsPO => "B2BCustomerReferenceAsPO";
    public static string ProvidedB2BSpecialInstructions => "B2BSpecialInstructions";
    public static string B2CSpecialInstructions => "B2CSpecialInstructions";

    public static string ShippingAddressModifiedIdInCheckoutAttribute => "ShippingAddressModifiedIdInCheckout";
    public static string IsShippingAddressModifiedInCheckoutAttribute => "IsShippingAddressModifiedInCheckout";
    public static string SelectedB2BDeliveryDateAttribute => "SelectedB2BDeliveryDate";

    public static string B2BQouteOrderAttribute => "QouteOrderSelected";
    public static string B2CQouteOrderAttribute => "B2CQouteOrderSelected";
    public static string B2BQuoteAssistantRoleName => "B2B Quote Assistant";
    public static string B2BQuoteAssistantRoleSystemName => "B2BQuoteAssistant";

    public static CacheKey ErpProductInfoSpecificationAttributeOptionIdsByNamesCacheKey => new("ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId-{0}-{1}-{2}", ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId);
    public static string ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId => "ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId.{0}";

    public static CacheKey ErpSpecificationAttributeOptionIdsForSpecialExcludeOptionNamesCacheKey => new("ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames-{0}-{1}", ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames);
    public static string ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames => "ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames.{0}";

    public static CacheKey ErpProductInfoUOMCacheKey => new("ErpProductInfoUOM-{0}-{1}", ErpProductInfoUOM);
    public static string ErpProductInfoUOM => "ErpProductInfoUOM.{0}";

    public static CacheKey ErpProductIdsBySpecialExcludeOptionNamesCacheKey => new("ErpProductIdsBySpecialExcludeOptionNames-{0}-{1}", ErpProductIdsBySpecialExcludeOptionNames);
    public static string ErpProductIdsBySpecialExcludeOptionNames => "ErpProductIdsBySpecialExcludeOptionNames.{0}";

    public static CacheKey ProductsByIdCacheKey => new("NopProductId-{0}", NopProductId);
    public static string NopProductId => "NopProductId.{0}";

    public static CacheKey ErpCustomerAccountErpActivityLogSyncLabelSelectList => new("B2BB2CFeatures.ErpActivityLog.SyncLabelSelectList");
    public static CacheKey ErpProductModelProductPriceCacheKey => new("Erp.ProductPricing.ProductModel.ProductPrice-{0}", ProductPrice);
    public static string ProductPrice => "ProductPrice.{0}";

    #region Erp Order Status

    public static string ErpOrderStatusApproved => "Approved";
    public static string ErpOrderStatusPendingApproval => "Pending Approval";
    public static string ErpOrderStatusProcessing => "Processing";

    #endregion

    public static string B2COrderOnlineSavingsAdjustmentValue => "B2COrderOnlineSavingsAdjustmentValue";
    public static CacheKey ErpUserCurrentYearSavingsByCustomerCacheKey => new("Erp.UserInformation.CurrentYearSavings-{0}", CustomerIdForCurrentYearSavings);
    public static string CustomerIdForCurrentYearSavings => "CurrentYearSavings-{0}";
    public static CacheKey ErpUserAllTimeSavingsByCustomerCacheKey => new("Erp.UserInformation.AllTimeSavings-{0}", CustomerIdForAllTimeSavings);
    public static string CustomerIdForAllTimeSavings => "AllTimeSavings-{0}";

    public static CacheKey B2BCustomerConfigurationPrefixCacheKey => new("B2B.CustomerConfiguration.");
    public static string B2CShippingOptionSystemName => "Shipping.B2CShipping";
    public static string B2CShippingOptionName => "Plugins.Shipping.B2CShipping.Options.OptionName";
    public static string B2CShippingOptionDescription => "Plugins.Shipping.B2CShipping.Options.Description";

    #region Sales org warehouse

    public static string ErpSalesOrgWarehouseInsert => "B2BB2CFeatures.ErpSalesOrgWarehouse.Insert";
    public static string ErpSalesOrgWarehouseUpdate => "B2BB2CFeatures.ErpSalesOrgWarehouse.Update";
    public static string ErpSalesOrgWarehouseDelete => "B2BB2CFeatures.ErpSalesOrgWarehouse.Delete";

    #endregion

    #region Work Flow Message Template

    public static string MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification => "ERPOrderPlaceFailed.SalesRepNotification";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer => "ERPAccountCustomerRegistration.CreatedNotificationToCustomer";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin => "ERPAccountCustomerRegistration.CreatedNotificationToAdmin";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification => "ERPAccountCustomerRegistration.ApprovedNotification";

    #endregion

    public static string ProcessFailedErpOrdersTask => "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ProcessFailedErpOrdersTask";
    public static string ProcessFailedErpOrdersTaskName => "B2B Process Failed ERP Orders";
    public static int DefaultTaskTimeOutPeriod => 360;

}