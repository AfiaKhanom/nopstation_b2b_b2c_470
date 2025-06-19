using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.B2B.D365BCIntegration.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.D365BCIntegration.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<D365BCIntegrationSettings, ConfigurationModel>()
                .ForMember(model => model.ClientId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ClientSecret_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BaseApiUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CompanyName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultCustomerId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ErpCallTimeOut_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallMaxRetries_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallRestTimeInMinutes_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomerSyncLimit_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductSyncLimit_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.StockSyncLimit_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TenantId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Environment_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.OrderSyncLimit_OverrideForStore, options => options.Ignore());

            CreateMap<ConfigurationModel, D365BCIntegrationSettings>();
        }

        public int Order => 1;
    }
}