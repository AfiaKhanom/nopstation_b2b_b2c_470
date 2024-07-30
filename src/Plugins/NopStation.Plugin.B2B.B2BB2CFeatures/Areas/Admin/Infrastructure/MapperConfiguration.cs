using AutoMapper;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            #region Configuration

            CreateMap<B2BB2CFeaturesSettings, ConfigurationModel>()
                .ForMember(model => model.IsActive_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableWarehouse_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PlaceB2BOrder_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PlaceB2COrder_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseNopProductPrice_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseProductGroupPrice_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseProductSpecialPrice_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseProductCombinedPrice_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowBackOrderingForAll_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.IsB2BUserRegisterAllowed_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.IsShowLoginForPrice_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LastDateTimeOfTCUpdate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UpdatedOnUtc_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UpdatedById_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ERPTimeOutMilliseconds_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaxERPIntegrationOrderPlaceReties_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLogOnErpCall_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLiveStockChecks_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLivePriceChecks_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.StockDisplayFormat_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaintainUniqueCustomerReference_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableQuoteFunctionality_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomerRefrenceCheckoutAttributeId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayAddToQuickListFavouriteButton_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowAddressEditOnCheckoutForAll_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DeliveryDays_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CutoffTime_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ERPToDetermineDate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.OverSpendWarningText_OverrideForStore, options => options.Ignore())

                .ForMember(model => model.OrderMaximumQuantity_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultCountryId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Override_AllowOverspend, options => options.Ignore())
                .ForMember(model => model.Override_TrackInventoryMethodId, options => options.Ignore())
                .ForMember(model => model.Override_LowStockActivityId, options => options.Ignore())
                .ForMember(model => model.Override_BackorderModeId, options => options.Ignore())
                .ForMember(model => model.Override_AllowBackInStockSubscriptions, options => options.Ignore())
                .ForMember(model => model.Override_ProductAvailabilityRangeId, options => options.Ignore())
                .ForMember(model => model.Override_AvailableForPreOrder, options => options.Ignore())
                .ForMember(model => model.Override_DisplayStockAvailability, options => options.Ignore())
                .ForMember(model => model.Override_DisplayStockQuantity, options => options.Ignore())
                .ForMember(model => model.GoogleMapsApiKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Latitude_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Longitude_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, B2BB2CFeaturesSettings>();

            #endregion

            #region ErpShipToAddress

            CreateMap<ErpShipToAddress, ErpShipToAddressModel>()
                .ForMember(model => model.AddressModel, options => options.Ignore());
            CreateMap<ErpShipToAddressModel, ErpShipToAddress>();

            #endregion

            #region ErpAccount

            CreateMap<ErpAccount, ErpAccountModel>();
            CreateMap<ErpAccountModel, ErpAccount>()
                .ForMember(model => model.ErpAccountStatusType, options => options.Ignore())
                .ForMember(model => model.ErpSalesOrg, options => options.Ignore());

            #endregion

            #region ErpSalesOrg

            CreateMap<ErpSalesOrg, ErpSalesOrgModel>();
            CreateMap<ErpSalesOrgModel, ErpSalesOrg>();

            #endregion

            #region ErpNopUser

            CreateMap<ErpNopUser, ErpNopUserModel>();
            CreateMap<ErpNopUserModel, ErpNopUser>();

            CreateMap<ErpSalesRep, ErpSalesRepModel>()
                .ForMember(model => model.AvailableCustomers, options => options.Ignore())
                .ForMember(model => model.AvailableSalesOrgs, options => options.Ignore())
                .ForMember(model => model.SalesOrgIds, options => options.Ignore());
            CreateMap<ErpSalesRepModel, ErpSalesRep>();

            #endregion

            #region ErpInvoice

            CreateMap<ErpInvoice, ErpInvoiceModel>()
                .ForMember(model => model.AvailableDocumentTypes, options => options.Ignore());

            CreateMap<ErpInvoiceModel, ErpInvoice>();

            #endregion

            #region ErpActivityLogs

            CreateMap<ErpLogs, ErpActivityLogModel>();
            CreateMap<ErpActivityLogModel, ErpLogs>()
                .ForMember(model => model.LogLevel, options => options.Ignore())
                .ForMember(model => model.ErpSyncLavel, options => options.Ignore());

            CreateMap<ErpActivityLogs, ErpActivityLogsModel>();
            CreateMap<ErpActivityLogsModel, ErpActivityLogs>();

            CreateMap<ActivityLogType, ErpActivityLogsTypeModel>();
            CreateMap<ErpActivityLogsTypeModel, ActivityLogType>();

            #endregion

            #region RegisteredOfficeAddress
            CreateMap<Address, RegisteredOfficeAddressModel>()
                .ForMember(model => model.AvailableCountries_ROA, options => options.Ignore())
                .ForMember(model => model.AvailableStates_ROA, options => options.Ignore())
                .ForMember(model => model.CountryName_ROA, options => options.Ignore())
                .ForMember(model => model.StateProvinceName_ROA, options => options.Ignore())
                .ForMember(model => model.CityRequired_ROA, options => options.Ignore())
                .ForMember(model => model.CompanyRequired_ROA, options => options.Ignore())
                .ForMember(model => model.CountyRequired_ROA, options => options.Ignore())
                .ForMember(model => model.FaxRequired_ROA, options => options.Ignore())
                .ForMember(model => model.PhoneRequired_ROA, options => options.Ignore())
                .ForMember(model => model.StateProvinceName_ROA, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required_ROA, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired_ROA, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired_ROA, options => options.Ignore());
            CreateMap<RegisteredOfficeAddressModel, Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());
            #endregion

            #region PhysicalTradingAddress
            CreateMap<Address, PhysicalTradingAddressModel>()
                .ForMember(model => model.AvailableCountries_PTA, options => options.Ignore())
                .ForMember(model => model.AvailableStates_PTA, options => options.Ignore())
                .ForMember(model => model.CountryName_PTA, options => options.Ignore())
                .ForMember(model => model.StateProvinceName_PTA, options => options.Ignore())
                .ForMember(model => model.CityRequired_PTA, options => options.Ignore())
                .ForMember(model => model.CompanyRequired_PTA, options => options.Ignore())
                .ForMember(model => model.CountyRequired_PTA, options => options.Ignore())
                .ForMember(model => model.FaxRequired_PTA, options => options.Ignore())
                .ForMember(model => model.PhoneRequired_PTA, options => options.Ignore())
                .ForMember(model => model.StateProvinceName_PTA, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required_PTA, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired_PTA, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired_PTA, options => options.Ignore());
            CreateMap<PhysicalTradingAddressModel, Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());
            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 1;

        #endregion
    }
}
