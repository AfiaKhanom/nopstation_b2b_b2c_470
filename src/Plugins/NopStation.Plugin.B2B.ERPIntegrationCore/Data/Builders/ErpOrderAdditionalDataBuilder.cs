using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpOrderAdditionalDataBuilder : NopEntityBuilder<ErpOrderAdditionalData>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderAdditionalData), nameof(ErpOrderAdditionalData.NopOrderId))).AsInt32() 
                .WithColumn(nameof( ErpOrderAdditionalData.ErpOrderNumber)).AsString()
                .WithColumn(nameof( ErpOrderAdditionalData.ErpOrderOriginTypeId)).AsInt32()
                .WithColumn(nameof( ErpOrderAdditionalData.ErpOrderTypeId)).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderAdditionalData), nameof(ErpOrderAdditionalData.OrderPlacedByNopCustomerId))).AsInt32()
                .WithColumn(nameof( ErpOrderAdditionalData.QuoteExpiryDate)).AsDateTime2().Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderAdditionalData), nameof(ErpOrderAdditionalData.QuoteSalesOrderId))).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderAdditionalData), nameof(ErpOrderAdditionalData.ErpAccountId))).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpOrderAdditionalData), nameof(ErpOrderAdditionalData.ErpShipToAddressId))).AsInt32().Nullable().ForeignKey<ErpShipToAddress>(onDelete: Rule.None)
                .WithColumn(nameof( ErpOrderAdditionalData.SpecialInstructions)).AsString().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.CustomerReference)).AsString().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.ERPOrderStatus)).AsString()
                .WithColumn(nameof( ErpOrderAdditionalData.DeliveryDate)).AsDateTime2().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.IntegrationStatusTypeId)).AsInt32()
                .WithColumn(nameof( ErpOrderAdditionalData.IntegrationError)).AsString().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.IntegrationRetries)).AsInt32().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.IntegrationErrorDateTimeUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.LastERPUpdateUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.IsShippingAddressModified)).AsBoolean()
                .WithColumn(nameof( ErpOrderAdditionalData.IsOrderPlaceNotificationSent)).AsBoolean().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.ErpOrderPlaceByCustomerTypeId)).AsInt32()
                .WithColumn(nameof( ErpOrderAdditionalData.ChangedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof( ErpOrderAdditionalData.ChangedById)).AsInt32(); 

        }
        #endregion
    }
}
