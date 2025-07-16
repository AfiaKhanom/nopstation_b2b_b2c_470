using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpWarehouseAdditionalDataBuilder : NopEntityBuilder<ErpWarehouseAdditionalData>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpWarehouseAdditionalData.Code)).AsString()
            .WithColumn(nameof(ErpWarehouseAdditionalData.LastUpdateTime)).AsDateTime2()
            .WithColumn(nameof(ErpWarehouseAdditionalData.NopProductId)).AsInt32().Nullable();
    }

    #endregion
}
