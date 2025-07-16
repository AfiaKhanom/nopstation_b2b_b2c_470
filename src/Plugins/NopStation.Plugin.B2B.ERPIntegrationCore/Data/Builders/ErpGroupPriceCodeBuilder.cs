using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpGroupPriceCodeBuilder : NopEntityBuilder<ErpGroupPriceCode>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpGroupPriceCode.Code)).AsString(50)
            .WithColumn(nameof(ErpGroupPriceCode.LastUpdateTime)).AsDateTime2();
    }

    #endregion
}
