using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpAccountCustomerRegistrationPhysicalTradingAddressBuilder : NopEntityBuilder<ErpAccountCustomerRegistrationPhysicalTradingAddress>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpAccountCustomerRegistrationPhysicalTradingAddress.FormId)).AsInt32().ForeignKey<ErpAccountCustomerRegistrationForm>(onDelete: Rule.None)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPhysicalTradingAddress.FullName)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPhysicalTradingAddress.Surname)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPhysicalTradingAddress.PhysicalTradingAddressId)).AsInt32();
    }

    #endregion
}
