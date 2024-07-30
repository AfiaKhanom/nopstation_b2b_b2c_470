using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Mapping.Builder
{
    /// <summary>
    /// Represents a task entity builder
    /// </summary>
    public partial class SyncTaskBuilder : NopEntityBuilder<SyncTask>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
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
                .WithColumn(nameof(SyncTask.DayTimeSlots)).AsString(int.MaxValue).Nullable();
        }

        #endregion
    }
}
