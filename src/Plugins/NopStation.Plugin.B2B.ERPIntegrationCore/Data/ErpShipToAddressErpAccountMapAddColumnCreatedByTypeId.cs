using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/02/20 08:31:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpShipToAddressErpAccountMap ErpShipToAddressCreatedByTypeId column addition", MigrationProcessType.NoMatter)]
public class ErpShipToAddressErpAccountMapAddColumnCreatedByTypeId : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        var erpShipToAddressErpAccountMaptableName = TableName<ErpShiptoAddressErpAccountMap>();

        if (Schema.Table(erpShipToAddressErpAccountMaptableName).Exists() &&
            !Schema.Table(erpShipToAddressErpAccountMaptableName).Column(nameof(ErpShiptoAddressErpAccountMap.ErpShipToAddressCreatedByTypeId)).Exists())
        {
            Create.Column(nameof(ErpShiptoAddressErpAccountMap.ErpShipToAddressCreatedByTypeId))
                .OnTable(erpShipToAddressErpAccountMaptableName)
                .AsInt32()
                .Nullable();            
        }    
    }
}
