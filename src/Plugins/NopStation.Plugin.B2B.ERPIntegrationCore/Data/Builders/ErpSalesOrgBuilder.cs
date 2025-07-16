using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpSalesOrgBuilder : NopEntityBuilder<ErpSalesOrg>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpSalesOrg.Code)).AsString()
            .WithColumn(nameof(ErpSalesOrg.Name)).AsString()
            .WithColumn(nameof(ErpSalesOrg.Email)).AsString()
            .WithColumn(nameof(ErpSalesOrg.AddressId)).AsInt32()
            .WithColumn(nameof(ErpSalesOrg.IntegrationClientId)).AsString().Nullable()
            .WithColumn(nameof(ErpSalesOrg.AuthenticationKey)).AsString().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpAccountSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpGroupPriceSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpShipToAddressSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpProductSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpStockSyncTimeOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
