using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpGroupPriceBuilder : NopEntityBuilder<ErpGroupPrice>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpGroupPrice.ErpNopGroupPriceCodeId)).AsInt32().ForeignKey<ErpGroupPriceCode>(onDelete: Rule.None)
            .WithColumn(nameof(ErpGroupPrice.NopProductId)).AsInt32().ForeignKey<Product>(onDelete: Rule.None)
            .WithColumn(nameof(ErpGroupPrice.Price)).AsDecimal(18, 4); 
    }

    #endregion
}
