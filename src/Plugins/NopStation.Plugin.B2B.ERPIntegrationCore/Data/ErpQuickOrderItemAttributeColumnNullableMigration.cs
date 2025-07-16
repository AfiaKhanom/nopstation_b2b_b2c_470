using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/10/30 12:12:12", "NopStation.Plugin.B2B.ERPIntegrationCore Erp Quick Order Item column Attribute_XML made nullable", MigrationProcessType.Update)]
public class ErpQuickOrderItemAttributeColumnNullableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(QuickOrderItem)))
            .Column(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.AttributesXml))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(QuickOrderItem)))
            .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.AttributesXml)))
            .AsString(int.MaxValue)
            .Nullable();
        }
    }

    #endregion
}
