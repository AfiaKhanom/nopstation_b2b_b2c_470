using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders
{
    public class ErpNopUserBuilder : NopEntityBuilder<ErpNopUser>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUser), nameof(ErpNopUser.NopCustomerId))).AsInt32() 
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUser), nameof(ErpNopUser.ErpAccountId))).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUser), nameof(ErpNopUser.ErpShipToAddressId))).AsInt32().ForeignKey<ErpShipToAddress>(onDelete: Rule.None)
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUser), nameof(ErpNopUser.BillingErpShipToAddressId))).AsInt32()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpNopUser), nameof(ErpNopUser.ShippingErpShipToAddressId))).AsInt32()
                .WithColumn(nameof(ErpNopUser.ErpUserTypeId)).AsInt32(); 

        }
        #endregion
    }
}
