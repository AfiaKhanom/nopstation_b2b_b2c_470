using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpSalesRepErpAccountMapBuilder : NopEntityBuilder<ErpSalesRepErpAccountMap>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpSalesRepErpAccountMap.ErpSalesRepId)).AsInt32().ForeignKey<ErpSalesRep>(onDelete: Rule.None)
            .WithColumn(nameof(ErpSalesRepErpAccountMap.ErpAccountId)).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None);
    }

    #endregion
}
