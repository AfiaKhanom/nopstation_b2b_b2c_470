using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/10/07 12:12:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpOrderItemAdditionalData Added column Warehouse", MigrationProcessType.Update)]
public class ErpOrderItemAdditionalDataSchemaMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderItemAdditionalData)))
            .Column(NameCompatibilityManager.GetColumnName(typeof(ErpOrderItemAdditionalData), nameof(ErpOrderItemAdditionalData.WareHouse))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderItemAdditionalData)))
            .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderItemAdditionalData), nameof(ErpOrderItemAdditionalData.WareHouse)))
            .AsString()
            .Nullable();
        }
    }

    #endregion
}