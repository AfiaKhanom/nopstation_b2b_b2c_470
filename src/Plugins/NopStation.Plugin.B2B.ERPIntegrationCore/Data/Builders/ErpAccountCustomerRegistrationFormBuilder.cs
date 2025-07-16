using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpAccountCustomerRegistrationFormBuilder : NopEntityBuilder<ErpAccountCustomerRegistrationForm>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.FullRegisteredName)).AsString(100)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.RegistrationNumber)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.VatNumber)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.TelephoneNumber1)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.TelephoneNumber2)).AsString(50).Nullable()
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.TelefaxNumber)).AsString(50).Nullable()
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.AccountsContactPersonNameSurname)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.AccountsEmail)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.AccountsTelephoneNumber)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.AccountsCellphoneNumber)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.BuyerContactPersonNameSurname)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.BuyerEmail)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.NatureOfBusiness)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.RegisteredOfficeAddressId)).AsInt32()
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.TypeOfBusiness)).AsString(50)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.EstimatePurchasesPerMonthZAR)).AsDecimal(18,4)
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.CreditLimitRequired)).AsBoolean()
            .WithColumn(nameof(ErpAccountCustomerRegistrationForm.IsApproved)).AsBoolean().Nullable();
    }

    #endregion
}
