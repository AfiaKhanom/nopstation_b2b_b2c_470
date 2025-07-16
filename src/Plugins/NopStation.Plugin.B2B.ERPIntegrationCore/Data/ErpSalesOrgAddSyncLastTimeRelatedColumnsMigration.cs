using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/02/01 12:42:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpSalesOrg Last sync time related columns addition", MigrationProcessType.Update)]
public class ErpSalesOrgAddSyncLastTimeRelatedColumnsMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        var erpSalesOrgTableName = TableName<ErpSalesOrg>();

        if (Schema.Table(erpSalesOrgTableName).Exists())
        {
            if(!Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.LastErpAccountSyncTimeOnUtc)).Exists())
            {
                Create.Column(nameof(ErpSalesOrg.LastErpAccountSyncTimeOnUtc))
                    .OnTable(erpSalesOrgTableName)
                    .AsDateTime2()
                    .Nullable();
            }
            if (!Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.LastErpGroupPriceSyncTimeOnUtc)).Exists())
            {
                Create.Column(nameof(ErpSalesOrg.LastErpGroupPriceSyncTimeOnUtc))
                    .OnTable(erpSalesOrgTableName)
                    .AsDateTime2()
                    .Nullable();
            }
            if (!Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.LastErpShipToAddressSyncTimeOnUtc)).Exists())
            {
                Create.Column(nameof(ErpSalesOrg.LastErpShipToAddressSyncTimeOnUtc))
                    .OnTable(erpSalesOrgTableName)
                    .AsDateTime2()
                    .Nullable();
            }
            if (!Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.LastErpProductSyncTimeOnUtc)).Exists())
            {
                Create.Column(nameof(ErpSalesOrg.LastErpProductSyncTimeOnUtc))
                    .OnTable(erpSalesOrgTableName)
                    .AsDateTime2()
                    .Nullable();
            }
            if (!Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.LastErpStockSyncTimeOnUtc)).Exists())
            {
                Create.Column(nameof(ErpSalesOrg.LastErpStockSyncTimeOnUtc))
                    .OnTable(erpSalesOrgTableName)
                    .AsDateTime2()
                    .Nullable();
            }
        }    
    }
}
