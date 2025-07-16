using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpShiptoAddressErpAccountMapBuilder : NopEntityBuilder<ErpShiptoAddressErpAccountMap>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpShiptoAddressErpAccountMap.ErpAccountId)).AsInt32().NotNullable().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(ErpShiptoAddressErpAccountMap.ErpShiptoAddressId)).AsInt32().NotNullable().ForeignKey<ErpShipToAddress>(onDelete: Rule.None)
            .WithColumn(nameof(ErpShiptoAddressErpAccountMap.ErpShipToAddressCreatedByTypeId)).AsInt32().Nullable();
    }

    #endregion
}
