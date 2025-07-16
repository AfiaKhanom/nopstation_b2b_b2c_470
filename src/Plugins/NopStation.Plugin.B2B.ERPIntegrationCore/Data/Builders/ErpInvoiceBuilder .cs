using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpInvoiceBuilder : NopEntityBuilder<ErpInvoice>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpInvoice.PostingDateUtc)).AsDateTime2()
            .WithColumn(nameof(ErpInvoice.ErpDocumentNumber)).AsString()
            .WithColumn(nameof(ErpInvoice.Description)).AsString()
            .WithColumn(nameof(ErpInvoice.AmountInclVat)).AsDecimal(18, 4)
            .WithColumn(nameof(ErpInvoice.AmountExclVat)).AsDecimal(18, 4)
            .WithColumn(nameof(ErpInvoice.ErpAccountId)).AsInt32()
            .WithColumn(nameof(ErpInvoice.CurrencyCode)).AsString()
            .WithColumn(nameof(ErpInvoice.DocumentTypeId)).AsInt32()
            .WithColumn(nameof(ErpInvoice.DocumentDisplayName)).AsString()
            .WithColumn(nameof(ErpInvoice.DueDateUtc)).AsDateTime2()
            .WithColumn(nameof(ErpInvoice.RelatedDocumentNo)).AsString()
            .WithColumn(nameof(ErpInvoice.ShipmentDateUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpInvoice.ItemCount)).AsInt32()
            .WithColumn(nameof(ErpInvoice.DocumentDateUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpInvoice.PODSignedById)).AsInt32()
            .WithColumn(nameof(ErpInvoice.PODSignedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpInvoice.ErpOrderNumber)).AsString();
    }

    #endregion
}
