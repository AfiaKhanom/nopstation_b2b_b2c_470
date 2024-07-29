using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpNopUserAccountMapBuilder : NopEntityBuilder<ErpNopUserAccountMap>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUserAccountMap), nameof(ErpNopUserAccountMap.ErpAccountId))).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUserAccountMap), nameof(ErpNopUserAccountMap.ErpUserId))).AsInt32().ForeignKey<ErpNopUser>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUserAccountMap), nameof(ErpNopUserAccountMap.CustomerRolesIds))).AsString();
             
        }
    }
    #endregion
}

