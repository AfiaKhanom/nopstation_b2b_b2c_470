using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace Nop.Plugin.Misc.QuickOrder.Data
{
    public class QuickOrderItemBuilder : NopEntityBuilder<QuickOrderItem>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.ProductSku))).AsString()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.Quantity))).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.AttributesXml))).AsString(int.MaxValue).WithDefaultValue("")
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderItem), nameof(QuickOrderItem.QuickOrderTemplateId))).AsInt32().ForeignKey<QuickOrderTemplate>(onDelete: Rule.None);
             
        }

        #endregion
    }
}