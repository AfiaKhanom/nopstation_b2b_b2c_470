using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class QuickOrderTemplateBuilder : NopEntityBuilder<QuickOrderTemplate>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(QuickOrderTemplate.Name)).AsString()
            .WithColumn(nameof(QuickOrderTemplate.CustomerId)).AsInt32()
            .WithColumn(nameof(QuickOrderTemplate.LastOrderDate)).AsDateTime2().Nullable()
            .WithColumn(nameof(QuickOrderTemplate.TotalPriceOfItems)).AsDecimal(18, 4)
            .WithColumn(nameof(QuickOrderTemplate.LastPriceCalculatedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(QuickOrderTemplate.CreatedOnUtc)).AsDateTime2()
            .WithColumn(nameof(QuickOrderTemplate.EditedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(QuickOrderTemplate.Deleted)).AsBoolean().WithDefaultValue(false); 
    }

    #endregion
}
