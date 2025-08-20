
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AjaxCart.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AjaxCart.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<AjaxCartSettings, ConfigurationModel>()
            .ForMember(s => s.CustomProperties, options => options.Ignore())
            .ForMember(s => s.EnableAjaxCartPlugin_OverrideForStore, options => options.Ignore());

        CreateMap<ConfigurationModel, AjaxCartSettings>();

    }

    public int Order => 1;
}
