using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpAccountBuilder : NopEntityBuilder<ErpAccount>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ErpAccount.AccountNumber)).AsString(50)
                .WithColumn(nameof(ErpAccount.AccountName)).AsString(100)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpAccount), nameof(ErpAccount.ErpSalesOrgId))).AsInt32().ForeignKey<ErpSalesOrg>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpAccount), nameof(ErpAccount.BillingAddressId))).AsInt32().Nullable().ForeignKey<Address>(onDelete: Rule.None)
                .WithColumn(nameof(ErpAccount.BillingSuburb)).AsString(200).Nullable()
                .WithColumn(nameof(ErpAccount.VatNumber)).AsString(50).Nullable()
                .WithColumn(nameof(ErpAccount.CreditLimit)).AsDecimal(18,4)
                .WithColumn(nameof(ErpAccount.CreditLimitAvailable)).AsDecimal(18,4)
                .WithColumn(nameof(ErpAccount.CurrentBalance)).AsDecimal(18,4)
                .WithColumn(nameof(ErpAccount.LastPaymentAmount)).AsDecimal(18,4).Nullable()
                .WithColumn(nameof(ErpAccount.LastPaymentDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.AllowOverspend)).AsBoolean()
                .WithColumn(nameof(ErpAccount.PreFilterFacets)).AsString(500).Nullable()
                .WithColumn(nameof(ErpAccount.PaymentTypeCode)).AsString(10).Nullable()
                .WithColumn(nameof(ErpAccount.OverrideBackOrderingConfigSetting)).AsBoolean()
                .WithColumn(nameof(ErpAccount.AllowAccountsBackOrdering)).AsBoolean()
                .WithColumn(nameof(ErpAccount.OverrideAddressEditOnCheckoutConfigSetting)).AsBoolean()
                .WithColumn(nameof(ErpAccount.AllowAccountsAddressEditOnCheckout)).AsBoolean()
                .WithColumn(nameof(ErpAccount.OverrideStockDisplayFormatConfigSetting)).AsBoolean()
                .WithColumn(nameof(ErpAccount.ErpAccountStatusTypeId)).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpAccount), nameof(ErpAccount.LastErpAccountSyncDate))).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.LastPriceRefresh)).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.B2BPriceGroupCodeId)).AsInt32().Nullable()
                .WithColumn(nameof(ErpAccount.TotalSavingsForthisYear)).AsDecimal(18,4).Nullable()
                .WithColumn(nameof(ErpAccount.TotalSavingsForAllTime)).AsDecimal(18,4).Nullable()
                .WithColumn(nameof(ErpAccount.TotalSavingsForthisYearUpdatedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.TotalSavingsForAllTimeUpdatedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.LastTimeOrderSyncOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(ErpAccount.IsDefaultPaymentAccount)).AsBoolean()
                .WithColumn(nameof(ErpAccount.StockDisplayFormatTypeId)).AsInt32()
                .WithColumn(nameof(ErpAccount.PercentageOfStockAllowed)).AsDecimal().Nullable(); 

        }
        #endregion
    }
}
