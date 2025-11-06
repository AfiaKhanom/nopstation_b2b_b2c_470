using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/11/16 02:20:00", "ErpNopUserAccountMap Table Add column ErpUserTypeId", MigrationProcessType.Update)]
public class ErpNopUserAccountMapAddColErpUserTypeId : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpNopUserAccountMap)))
            .Column(nameof(ErpNopUserAccountMap.ErpUserTypeId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpNopUserAccountMap)))
                .AddColumn(nameof(ErpNopUserAccountMap.ErpUserTypeId))
                .AsInt32()
                .Nullable()
                .WithDefaultValue(0);
        }
    }
}