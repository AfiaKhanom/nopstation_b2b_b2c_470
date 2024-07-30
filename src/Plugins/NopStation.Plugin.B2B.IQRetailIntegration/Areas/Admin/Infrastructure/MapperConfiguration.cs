using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.B2B.IQRetailIntegration.Areas.Admin.Model;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            #region Configuration

            CreateMap<IQRetailIntegrationSettings, ConfigurationModel>()
                .ForMember(model => model.BaseUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UserName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Password_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Location_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.B2cPriceCode_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultLimit_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ErpCallTimeOut_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultCustomerId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TerminalNumber_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CompanyId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice1_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice2_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice3_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice4_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice5_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice6_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice7_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice8_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice9_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SellPrice10_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallMaxRetries_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallRestTimeInMinutes_OverrideForStore, options => options.Ignore());
            CreateMap<ConfigurationModel, IQRetailIntegrationSettings>();

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
