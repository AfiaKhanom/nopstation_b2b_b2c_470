using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class QuickOrderItemBuilder : NopEntityBuilder<QuickOrderItem>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(QuickOrderItem.ProductSku)).AsString()
            .WithColumn(nameof(QuickOrderItem.Quantity)).AsInt32()
            .WithColumn(nameof(QuickOrderItem.AttributesXml)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(QuickOrderItem.QuickOrderTemplateId)).AsInt32().ForeignKey<QuickOrderTemplate>(onDelete: Rule.None);         
    }

    #endregion
}