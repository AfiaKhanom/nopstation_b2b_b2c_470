using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Data
{
    /// <summary>
    /// Represents a PDF job entity builder
    /// </summary>
    public class PdfJobBuilder : NopEntityBuilder<PdfJob>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PdfJob.JobTypeId)).AsInt32().NotNullable()
                .WithColumn(nameof(PdfJob.StatusId)).AsInt32().NotNullable()
                .WithColumn(nameof(PdfJob.RequestedByCustomerId)).AsInt32().NotNullable()
                .WithColumn(nameof(PdfJob.ParametersJson)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(PdfJob.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(PdfJob.CompletedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(PdfJob.ErrorMessage)).AsString(int.MaxValue).Nullable();
        }

        #endregion
    }
}
