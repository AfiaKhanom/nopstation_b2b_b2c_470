using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpAccountCustomerRegistrationPremisesBuilder : NopEntityBuilder<ErpAccountCustomerRegistrationPremises>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.FormId)).AsInt32().ForeignKey<ErpAccountCustomerRegistrationForm>(onDelete: Rule.None)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.OwnedOrLeased)).AsBoolean()
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.NameOfLandlord)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.AddressOfLandlord)).AsString(300)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.EmailOfLandlord)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationPremises.TelephoneNumberOfLandlord)).AsString(20);
    }

    #endregion
}
