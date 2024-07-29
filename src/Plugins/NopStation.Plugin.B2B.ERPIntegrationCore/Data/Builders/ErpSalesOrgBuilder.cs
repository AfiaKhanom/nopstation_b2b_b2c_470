using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpSalesOrgBuilder : NopEntityBuilder<ErpSalesOrg>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ErpSalesOrg.Code)).AsString()
                .WithColumn(nameof(ErpSalesOrg.Name)).AsString()
                .WithColumn(nameof(ErpSalesOrg.Email)).AsString()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSalesOrg), nameof(ErpSalesOrg.AddressId))).AsInt32()
                .WithColumn(nameof(ErpSalesOrg.IntegrationClientId)).AsString().Nullable()
                .WithColumn(nameof(ErpSalesOrg.AuthenticationKey)).AsString().Nullable();
        }
        #endregion
    }
}
