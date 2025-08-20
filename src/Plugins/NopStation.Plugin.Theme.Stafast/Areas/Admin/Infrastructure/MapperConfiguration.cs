using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Theme.Stafast.Areas.Admin.Models;

namespace NopStation.Plugin.Theme.Stafast.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<StafastSettings, ConfigurationModel>()
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, StafastSettings>();
        }
    }
}
