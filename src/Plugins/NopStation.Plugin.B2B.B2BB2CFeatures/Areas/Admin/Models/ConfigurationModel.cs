using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStockDisplayFormats = new List<SelectListItem>();
            AvailableSalesOrganizations = new List<SelectListItem>();
            AvailableSpecificationAttributes = new List<SelectListItem>();
            AvailableProductAvailabilityRanges_DefaultValue = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableWarehouse")]
        public bool EnableWarehouse { get; set; }
        public bool EnableWarehouse_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.IsActive")]
        public bool IsActive { get; set; }
        public bool IsActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.PlaceB2BOrder")]
        public bool PlaceB2BOrder { get; set; }
        public bool PlaceB2BOrder_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.PlaceB2COrder")]
        public bool PlaceB2COrder { get; set; }
        public bool PlaceB2COrder_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseNopProductPrice")]
        public bool UseNopProductPrice { get; set; }
        public bool UseNopProductPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseProductGroupPrice")]
        public bool UseProductGroupPrice { get; set; }
        public bool UseProductGroupPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseProductSpecialPrice")]
        public bool UseProductSpecialPrice { get; set; }
        public bool UseProductSpecialPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseProductCombinedPrice")]
        public bool UseProductCombinedPrice { get; set; }
        public bool UseProductCombinedPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.AllowBackOrderingForAll")]
        public bool AllowBackOrderingForAll { get; set; }
        public bool AllowBackOrderingForAll_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.IsB2BUserRegisterAllowed")]
        public bool IsB2BUserRegisterAllowed { get; set; }
        public bool IsB2BUserRegisterAllowed_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.IsB2CUserRegisterAllowed")]
        public bool IsB2CUserRegisterAllowed { get; set; }
        public bool IsB2CUserRegisterAllowed_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.IsShowLoginForPrice")]
        public bool IsShowLoginForPrice { get; set; }
        public bool IsShowLoginForPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.LastDateTimeOfTCUpdate")]
        public DateTime LastDateTimeOfTCUpdate { get; set; }
        public bool LastDateTimeOfTCUpdate_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }
        public bool UpdatedOnUtc_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UpdatedById")]
        public int UpdatedById { get; set; }
        public bool UpdatedById_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.ERPTimeOutMilliseconds")]
        public int ERPTimeOutMilliseconds { get; set; }
        public bool ERPTimeOutMilliseconds_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.MaxERPIntegrationOrderPlaceReties")]
        public int MaxERPIntegrationOrderPlaceReties { get; set; }
        public bool MaxERPIntegrationOrderPlaceReties_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableLogOnErpCall")]
        public bool EnableLogOnErpCall { get; set; }
        public bool EnableLogOnErpCall_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableLiveStockChecks")]
        public bool EnableLiveStockChecks { get; set; }
        public bool EnableLiveStockChecks_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableLivePriceChecks")]
        public bool EnableLivePriceChecks { get; set; }
        public bool EnableLivePriceChecks_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.StockDisplayFormat")]
        public int StockDisplayFormatId { get; set; }
        public bool StockDisplayFormat_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.MaintainUniqueCustomerReference")]
        public bool MaintainUniqueCustomerReference { get; set; }
        public bool MaintainUniqueCustomerReference_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableQuoteFunctionality")]
        public bool EnableQuoteFunctionality { get; set; }
        public bool EnableQuoteFunctionality_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.CustomerRefrenceCheckoutAttributeId")]
        public int CustomerRefrenceCheckoutAttributeId { get; set; }
        public bool CustomerRefrenceCheckoutAttributeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.AdminArea.Configuration.PreFilterFacetSpecificationAttribute")]
        public int PreFilterFacetSpecificationAttributeId { get; set; }
        public bool PreFilterFacetSpecificationAttributeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DisplayAddToQuickListFavouriteButton")]
        public bool DisplayAddToQuickListFavouriteButton { get; set; }
        public bool DisplayAddToQuickListFavouriteButton_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.AllowAddressEditOnCheckoutForAll")]
        public bool AllowAddressEditOnCheckoutForAll { get; set; }
        public bool AllowAddressEditOnCheckoutForAll_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DeliveryDays")]
        public int DeliveryDays { get; set; }
        public bool DeliveryDays_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.CutoffTime")]
        public int CutoffTime { get; set; }
        public bool CutoffTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.ERPToDetermineDate")]
        public bool ERPToDetermineDate { get; set; }
        public bool ERPToDetermineDate_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.OverSpendWarningText")]
        public string OverSpendWarningText { get; set; }
        public bool OverSpendWarningText_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DefaultB2COrganizationId")]
        public string DefaultB2COrganizationId { get; set; }
        public bool DefaultB2COrganizationId_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.AllowOverspend")]
        public bool AllowOverspend { get; set; }
        public bool Override_AllowOverspend { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.TrackInventoryMethodId")]
        public int TrackInventoryMethodId { get; set; }
        public bool Override_TrackInventoryMethodId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.LowStockActivityId_DefaultValue")]
        public int LowStockActivityId_DefaultValue { get; set; }
        public bool Override_LowStockActivityId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.BackorderModeId_DefaultValue")]
        public int BackorderModeId_DefaultValue { get; set; }
        public bool Override_BackorderModeId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.AllowBackInStockSubscriptions_DefaultValue")]
        public bool AllowBackInStockSubscriptions_DefaultValue { get; set; }
        public bool Override_AllowBackInStockSubscriptions { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.ProductAvailabilityRangeId_DefaultValue")]
        public int ProductAvailabilityRangeId_DefaultValue { get; set; }
        public bool Override_ProductAvailabilityRangeId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.AvailableForPreOrder_DefaultValue")]
        public bool AvailableForPreOrder_DefaultValue { get; set; }
        public bool Override_AvailableForPreOrder { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DisplayStockAvailability_DefaultValue")]
        public bool DisplayStockAvailability_DefaultValue { get; set; }
        public bool Override_DisplayStockAvailability { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DisplayStockQuantity_DefaultValue")]
        public bool DisplayStockQuantity_DefaultValue { get; set; }
        public bool Override_DisplayStockQuantity { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.OrderMaximumQuantity")]
        public int OrderMaximumQuantity { get; set; }
        public bool OrderMaximumQuantity_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DefaultCountryId")]
        public int DefaultCountryId { get; set; }
        public bool DefaultCountryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.EnableAccountPayment")]
        public bool EnableAccountPayment { get; set; }
        public bool EnableAccountPayment_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UsePrefilterFacet")]
        public bool UsePrefilterFacet { get; set; }
        public bool UsePrefilterFacet_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseDefaultAccountForB2CUser")]
        public bool UseDefaultAccountForB2CUser { get; set; }
        public bool UseDefaultAccountForB2CUser_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DefaultB2CErpAccountId")]
        public int DefaultB2CErpAccountId { get; set; }
        public bool DefaultB2CErpAccountId_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseERPIntegration")]
        public bool UseERPIntegration { get; set; }
        public bool UseERPIntegration_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.DownloadInvoicesPath")]
        public string DownloadInvoicesPath { get; set; }
        public bool DownloadInvoicesPath_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.FtpUserName")]
        public string FtpUserName { get; set; }
        public bool FtpUserName_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.FtpPassword")]
        public string FtpPassword { get; set; }
        public bool FtpPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UseMultiSalesOrg")]
        public bool UseMultiSalesOrg { get; set; }
        public bool UseMultiSalesOrg_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.UsePercentageOfAllocatedStock")]
        public bool UsePercentageOfAllocatedStock { get; set; }
        public bool UsePercentageOfAllocatedStock_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.PercentageOfStockAllowed")]
        public int PercentageOfStockAllowed { get; set; }
        public bool PercentageOfStockAllowed_OverrideForStore { get; set; }


        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.IsErpAccountCustomerRegisterAllowed")]
        public bool IsErpAccountCustomerRegisterAllowed { get; set; }
        public bool IsErpAccountCustomerRegisterAllowed_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.GoogleMapsApiKey")]
        public string GoogleMapsApiKey { get; set; }
        public bool GoogleMapsApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.Latitude")]
        public double Latitude { get; set; }
        public bool Latitude_OverrideForStore { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Configuration.Fields.Longitude")]
        public double Longitude { get; set; }
        public bool Longitude_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStockDisplayFormats { get; set; }
        public IList<SelectListItem> AvailableSalesOrganizations { get; set; }
        public IList<SelectListItem> AvailableSpecificationAttributes { get; set; }
        public IList<SelectListItem> AvailableProductAvailabilityRanges_DefaultValue { get; set; }

    }
}
