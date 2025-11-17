using System.Data;
using FluentMigrator;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopSchemaMigration("2025/11/18 02:30:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpNopUser Add NopCustomerId foreign key to Customer table", MigrationProcessType.Update)]
public class ErpNopUserAddNopCustomerIdForeignKeyMigration : AutoReversingMigration
{
    public override void Up()
    {
        var erpNopUserTableName = NameCompatibilityManager.GetTableName(typeof(ErpNopUser));
        var customerTableName = NameCompatibilityManager.GetTableName(typeof(Customer));
        var foreignKeyName = $"FK_{erpNopUserTableName}_{customerTableName}_NopCustomerId";
        var indexName = $"IX_{erpNopUserTableName}_NopCustomerId";

        if (Schema.Table(erpNopUserTableName).Exists() &&
            Schema.Table(erpNopUserTableName).Column(nameof(ErpNopUser.NopCustomerId)).Exists() &&
            !Schema.Table(erpNopUserTableName).Constraint(foreignKeyName).Exists())
        {
            if (!Schema.Table(erpNopUserTableName).Index(indexName).Exists())
            {
                Create.Index(indexName)
                    .OnTable(erpNopUserTableName)
                    .OnColumn(nameof(ErpNopUser.NopCustomerId)).Ascending()
                    .WithOptions().NonClustered();
            }
            Create.ForeignKey(foreignKeyName)
                .FromTable(erpNopUserTableName).ForeignColumn(nameof(ErpNopUser.NopCustomerId))
                .ToTable(customerTableName).PrimaryColumn(nameof(Customer.Id))
                .OnDelete(Rule.None);
        }
    }
}

