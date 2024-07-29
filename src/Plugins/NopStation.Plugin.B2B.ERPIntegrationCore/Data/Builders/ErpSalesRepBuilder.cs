using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpSalesRepBuilder : NopEntityBuilder<ErpSalesRep>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSalesRep), nameof(ErpSalesRep.NopCustomerId))).AsInt32()
                .WithColumn(nameof(ErpSalesRep.SalesRepTypeId)).AsInt32();
        }
        #endregion
    }
}
