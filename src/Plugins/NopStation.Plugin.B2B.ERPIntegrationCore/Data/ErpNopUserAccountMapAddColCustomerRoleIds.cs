using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/11/15 02:10:00", "ErpNopUserAccountMap Table Add column CustomerRoleIds", MigrationProcessType.Update)]
public class ErpNopUserAccountMapAddColCustomerRoleIds : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpNopUserAccountMap)))
            .Column(nameof(ErpNopUserAccountMap.CustomerRolesIds)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpNopUserAccountMap)))
                .AddColumn(nameof(ErpNopUserAccountMap.CustomerRolesIds))
                .AsString(int.MaxValue)
                .Nullable()
                .WithDefaultValue(string.Empty);
        }
    }
}