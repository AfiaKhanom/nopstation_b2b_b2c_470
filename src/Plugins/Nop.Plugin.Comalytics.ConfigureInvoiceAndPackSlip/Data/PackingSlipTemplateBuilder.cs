using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Data
{
    /// <summary>
    /// Represents a packing slip template entity builder
    /// </summary>
    public class PackingSlipTemplateBuilder : NopEntityBuilder<PackingSlipTemplate>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PackingSlipTemplate.StoreId)).AsInt32().NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.LanguageId)).AsInt32().NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.Name)).AsString(255).NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.TemplateHtml)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(PackingSlipTemplate.IsDefault)).AsBoolean().NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.RenderModeId)).AsInt32().NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(PackingSlipTemplate.UpdatedOnUtc)).AsDateTime2().NotNullable();
        }

        #endregion
    }
}
