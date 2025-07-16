using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/03/07 20:26:00", "NopStation.Plugin.B2B.ERPIntegrationCore Erp Logs rename column ErpSyncLavelId to ErpSyncLevelId", MigrationProcessType.NoMatter)]
public class ErpLogsRenameErpSyncLavelIdColumnMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpLogs)))
            .Column(NameCompatibilityManager.GetColumnName(typeof(ErpLogs), "ErpSyncLavelId")).Exists())
        {
            Rename
                .Column(NameCompatibilityManager.GetColumnName(typeof(ErpLogs), "ErpSyncLavelId"))
                .OnTable(NameCompatibilityManager.GetTableName(typeof(ErpLogs)))
                .To(nameof(ErpLogs.ErpSyncLevelId));
        }
    }
}
