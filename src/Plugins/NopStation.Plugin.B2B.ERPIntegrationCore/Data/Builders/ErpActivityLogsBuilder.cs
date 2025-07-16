using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpActivityLogsBuilder : NopEntityBuilder<ErpActivityLogs>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpActivityLogs.ErpActivityLogTypeId)).AsInt32()
            .WithColumn(nameof(ErpActivityLogs.EntityId)).AsInt32().Nullable()
            .WithColumn(nameof(ErpActivityLogs.EntityName)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ErpActivityLogs.Comment)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ErpActivityLogs.IpAddress)).AsString()
            .WithColumn(nameof(ErpActivityLogs.CustomerId)).AsInt32().Nullable()
            .WithColumn(nameof(ErpActivityLogs.CreatedOnUtc)).AsDateTime2();
    }

    #endregion
}