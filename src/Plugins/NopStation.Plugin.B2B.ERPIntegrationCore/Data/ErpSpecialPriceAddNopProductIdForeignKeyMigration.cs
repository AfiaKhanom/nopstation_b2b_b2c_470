using System.Data;
using FluentMigrator;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopSchemaMigration("2025/11/19 02:30:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpSpecialPrice Add NopProductId foreign key to Product table", MigrationProcessType.Update)]
public class ErpSpecialPriceAddNopProductIdForeignKeyMigration : AutoReversingMigration
{
    public override void Up()
    {
        var erpSpecialPriceTableName = NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice));
        var productTableName = NameCompatibilityManager.GetTableName(typeof(Product));
        var foreignKeyName = $"FK_{erpSpecialPriceTableName}_{productTableName}_NopProductId";
        var indexName = $"IX_{erpSpecialPriceTableName}_NopProductId";

        if (Schema.Table(erpSpecialPriceTableName).Exists() &&
            Schema.Table(erpSpecialPriceTableName).Column(nameof(ErpSpecialPrice.NopProductId)).Exists() &&
            !Schema.Table(erpSpecialPriceTableName).Constraint(foreignKeyName).Exists())
        {
            if (!Schema.Table(erpSpecialPriceTableName).Index(indexName).Exists())
            {
                Create.Index(indexName)
                    .OnTable(erpSpecialPriceTableName)
                    .OnColumn(nameof(ErpSpecialPrice.NopProductId)).Ascending()
                    .WithOptions().NonClustered();
            }
            Create.ForeignKey(foreignKeyName)
                .FromTable(erpSpecialPriceTableName).ForeignColumn(nameof(ErpSpecialPrice.NopProductId))
                .ToTable(productTableName).PrimaryColumn(nameof(Product.Id))
                .OnDelete(Rule.None);
        }
    }
}

