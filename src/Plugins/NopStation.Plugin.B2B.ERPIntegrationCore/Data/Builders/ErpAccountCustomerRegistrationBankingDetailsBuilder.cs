using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpAccountCustomerRegistrationBankingDetailsBuilder : NopEntityBuilder<ErpAccountCustomerRegistrationBankingDetails>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.FormId)).AsInt32().ForeignKey<ErpAccountCustomerRegistrationForm>(onDelete: Rule.None)
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.NameOfBanker)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.AccountName)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.AccountNumber)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.BranchCode)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationBankingDetails.Branch)).AsString(50);
    }

    #endregion
}
