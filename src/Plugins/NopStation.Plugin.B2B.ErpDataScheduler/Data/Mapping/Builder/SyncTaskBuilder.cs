using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Mapping.Builder;

public partial class SyncTaskBuilder : NopEntityBuilder<SyncTask>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SyncTask.Name)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(SyncTask.Type)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(SyncTask.Seconds)).AsInt32().NotNullable()
            .WithColumn(nameof(SyncTask.LastEnabledUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(SyncTask.Enabled)).AsBoolean().Nullable()
            .WithColumn(nameof(SyncTask.StopOnError)).AsBoolean().Nullable()
            .WithColumn(nameof(SyncTask.LastStartUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(SyncTask.LastEndUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(SyncTask.LastSuccessUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(SyncTask.DayTimeSlots)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(SyncTask.QuartzJobName)).AsString().Nullable()
            .WithColumn(nameof(SyncTask.IsIncremental)).AsBoolean()
            .WithColumn(nameof(SyncTask.IsRunning)).AsBoolean();
    }

    #endregion
}
