using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AdvanceCart.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<AdvanceCartSettings, ConfigurationModel>()
            .ForMember(s => s.CustomProperties, options => options.Ignore());
        CreateMap<ConfigurationModel, AdvanceCartSettings>();
    }

    public int Order => 1;
}
