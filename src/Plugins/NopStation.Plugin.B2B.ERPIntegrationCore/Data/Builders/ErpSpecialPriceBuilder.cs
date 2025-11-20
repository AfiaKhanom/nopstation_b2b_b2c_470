using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpSpecialPriceBuilder : NopEntityBuilder<ErpSpecialPrice>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpSpecialPrice.NopProductId)).AsInt32().ForeignKey<Product>(onDelete:Rule.None)
            .WithColumn(nameof(ErpSpecialPrice.Price)).AsDecimal(18, 4)
            .WithColumn(nameof(ErpSpecialPrice.ListPrice)).AsDecimal(18, 4)
            .WithColumn(nameof(ErpSpecialPrice.PercentageOfAllocatedStock)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(ErpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSpecialPrice.VolumeDiscount)).AsBoolean().Nullable()
            .WithColumn(nameof(ErpSpecialPrice.DiscountPerc)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(ErpSpecialPrice.PricingNote)).AsString().Nullable()
            .WithColumn(nameof(ErpSpecialPrice.ErpAccountId)).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None);
    }

    #endregion
}
