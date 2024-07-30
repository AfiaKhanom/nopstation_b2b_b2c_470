using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record ErpAccountCustomerRegistrationFormModel
    {
        public ErpAccountCustomerRegistrationFormModel()
        {
            RegisteredOfficeAddress = new RegisteredOfficeAddressModel();
            PhysicalTradingAddressModel = new ErpAccountCustomerRegistrationPhysicalTradingAddressModel();
            BankingDetailsModel = new ErpAccountCustomerRegistrationBankingDetailsModel();
            TradeReferencesModel = new ErpAccountCustomerRegistrationTradeReferencesModel();
            PremisesModel = new ErpAccountCustomerRegistrationPremisesModel();
        }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.FullRegisteredName")]
        public string FullRegisteredName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.VatNumber")]
        public string VatNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.TelephoneNumber1")]
        public string TelephoneNumber1 { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.TelephoneNumber2")]
        public string TelephoneNumber2 { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.TelefaxNumber")]
        public string TelefaxNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.AccountsContactPersonNameSurname")]
        public string AccountsContactPersonNameSurname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.AccountsEmail")]
        public string AccountsEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.AccountsTelephoneNumber")]
        public string AccountsTelephoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.AccountsCellphoneNumber")]
        public string AccountsCellphoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.BuyerContactPersonNameSurname")]
        public string BuyerContactPersonNameSurname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.BuyerEmail")]
        public string BuyerEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.NatureOfBusiness")]
        public string NatureOfBusiness { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.RegisteredOfficeAddress")]
        public RegisteredOfficeAddressModel RegisteredOfficeAddress { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.TypeOfBusiness")]
        public string TypeOfBusiness { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.EstimatePurchasesPerMonthZAR")]
        public decimal EstimatePurchasesPerMonthZAR { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Fields.CreditLimitRequired")]
        public bool CreditLimitRequired { get; set; }


        public ErpAccountCustomerRegistrationPhysicalTradingAddressModel PhysicalTradingAddressModel { get; set; }
        public ErpAccountCustomerRegistrationBankingDetailsModel BankingDetailsModel { get; set; }
        public ErpAccountCustomerRegistrationTradeReferencesModel TradeReferencesModel { get; set; }
        public ErpAccountCustomerRegistrationPremisesModel PremisesModel { get; set; }
    }
}
