using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace Nop.Plugin.Misc.QuickOrder.Data
{
    public class QuickOrderTemplateBuilder : NopEntityBuilder<QuickOrderTemplate>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(QuickOrderTemplate.Name)).AsString()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.CustomerId))).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.LastOrderDate))).AsDateTime2().Nullable()
                .WithColumn(nameof(QuickOrderTemplate.TotalPriceOfItems)).AsDecimal(18, 4)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.LastPriceCalculatedOnUtc))).AsDateTime2().Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.CreatedOnUtc))).AsDateTime2()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.EditedOnUtc))).AsDateTime2().Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(QuickOrderTemplate), nameof(QuickOrderTemplate.Deleted))).AsBoolean().WithDefaultValue(false); 
        }

        #endregion
    }
}
