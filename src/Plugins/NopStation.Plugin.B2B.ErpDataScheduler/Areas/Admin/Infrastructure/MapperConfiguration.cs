using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            #region Configuration

            CreateMap<ErpDataSchedulerSettings, ConfigurationModel>()
                .ForMember(model => model.SyncFromDate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NeedQuoteOrderCall_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.StartProductSyncAfterLastSyncedProduct_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, ErpDataSchedulerSettings>();

            #endregion

            #region SyncTask

            CreateMap<SyncTask, SyncTaskModel>()
                .ForMember(model => model.DayOfWeekSlots, options => options.Ignore())
                .ForMember(model => model.SyncLogSearchModel, options => options.Ignore());
            CreateMap<SyncTaskModel, SyncTask>()
                .ForMember(entity => entity.DayTimeSlots, options => options.Ignore());

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
