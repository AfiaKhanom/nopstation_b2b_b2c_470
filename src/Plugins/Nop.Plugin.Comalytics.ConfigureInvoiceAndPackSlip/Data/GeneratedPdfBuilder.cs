using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Data
{
    /// <summary>
    /// Represents a generated PDF entity builder
    /// </summary>
    public class GeneratedPdfBuilder : NopEntityBuilder<GeneratedPdf>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(GeneratedPdf.JobId)).AsInt32().NotNullable()
                .WithColumn(nameof(GeneratedPdf.OrderId)).AsInt32().NotNullable()
                .WithColumn(nameof(GeneratedPdf.FileName)).AsString(255).NotNullable()
                .WithColumn(nameof(GeneratedPdf.FilePath)).AsString(1000).NotNullable()
                .WithColumn(nameof(GeneratedPdf.Size)).AsInt64().NotNullable()
                .WithColumn(nameof(GeneratedPdf.CreatedOnUtc)).AsDateTime2().NotNullable();
        }

        #endregion
    }
}
