using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpOrderItemAdditionalDataBuilder : NopEntityBuilder<ErpOrderItemAdditionalData>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpOrderItemAdditionalData.NopOrderItemId)).AsInt32().ForeignKey<OrderItem>(onDelete: Rule.None)
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpOrderId)).AsInt32().ForeignKey<ErpOrderAdditionalData>(onDelete: Rule.None)
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpOrderLineNumber)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpSalesUoM)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpOrderLineStatus)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpDateRequired)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpDateExpected)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpDeliveryMethod)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpInvoiceNumber)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ErpOrderLineNotes)).AsString()
            .WithColumn(nameof(ErpOrderItemAdditionalData.LastErpUpdateUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ChangedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpOrderItemAdditionalData.ChangedBy)).AsInt32()
            .WithColumn(nameof(ErpOrderItemAdditionalData.WareHouse)).AsString().Nullable();
    }

    #endregion
}
