using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Data
{
    /// <summary>
    /// Represents an invoice template entity builder
    /// </summary>
    public class InvoiceTemplateBuilder : NopEntityBuilder<InvoiceTemplate>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(InvoiceTemplate.StoreId)).AsInt32().NotNullable()
                .WithColumn(nameof(InvoiceTemplate.LanguageId)).AsInt32().NotNullable()
                .WithColumn(nameof(InvoiceTemplate.Name)).AsString(255).NotNullable()
                .WithColumn(nameof(InvoiceTemplate.TemplateHtml)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(InvoiceTemplate.IsDefault)).AsBoolean().NotNullable()
                .WithColumn(nameof(InvoiceTemplate.RenderModeId)).AsInt32().NotNullable()
                .WithColumn(nameof(InvoiceTemplate.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(InvoiceTemplate.UpdatedOnUtc)).AsDateTime2().NotNullable();
        }

        #endregion
    }
}
