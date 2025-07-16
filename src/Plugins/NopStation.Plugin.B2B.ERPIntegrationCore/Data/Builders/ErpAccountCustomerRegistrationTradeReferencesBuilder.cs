using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpAccountCustomerRegistrationTradeReferencesBuilder : NopEntityBuilder<ErpAccountCustomerRegistrationTradeReferences>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.FormId)).AsInt32().ForeignKey<ErpAccountCustomerRegistrationForm>(onDelete: Rule.None)
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.Name)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.Telephone)).AsString(20)
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.Amount)).AsDecimal(18, 4)
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.Terms)).AsString(300)
            .WithColumn(nameof(ErpAccountCustomerRegistrationTradeReferences.HowLong)).AsString(300);
    }

    #endregion
}
