using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    #region Ctor

    public MapperConfiguration()
    {
        CreateMap<ERPIntegrationCoreSettings, ConfigurationModel>();
        CreateMap<ConfigurationModel, ERPIntegrationCoreSettings>();
    }

    #endregion

    #region Properties

    public int Order => 0;

    #endregion
}