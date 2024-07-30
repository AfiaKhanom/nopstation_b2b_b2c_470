using System;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public record ApplicationFormModel : BaseNopEntityModel
    {
        #region ctor

        public ApplicationFormModel()
        {
            RegisteredOfficeAddress = new AddressModel();
            PhysicalTradingAddressModel = new PhysicalTradingAddressModel();
            BankingDetailsModel = new BankingDetailsModel();
            TradeReferencesModel = new TradeReferencesModel();
            PremisesModel = new PremisesModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.FullRegisteredName")]
        public string FullRegisteredName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.VatNumber")]
        public string VatNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.TelephoneNumber1")]
        public string TelephoneNumber1 { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.TelephoneNumber2")]
        public string TelephoneNumber2 { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.TelefaxNumber")]
        public string TelefaxNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.AccountsContactPersonNameSurname")]
        public string AccountsContactPersonNameSurname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.AccountsEmail")]
        public string AccountsEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.AccountsTelephoneNumber")]
        public string AccountsTelephoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.AccountsCellphoneNumber")]
        public string AccountsCellphoneNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.BuyerContactPersonNameSurname")]
        public string BuyerContactPersonNameSurname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.BuyerEmail")]
        public string BuyerEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.NatureOfBusiness")]
        public string NatureOfBusiness { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.RegisteredOfficeAddress")]
        public AddressModel RegisteredOfficeAddress { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.TypeOfBusiness")]
        public string TypeOfBusiness { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.EstimatePurchasesPerMonthZAR")]
        public decimal EstimatePurchasesPerMonthZAR { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.CreditLimitRequired")]
        public bool CreditLimitRequired { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.CreatedOnUtc")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.CreatedBy")]
        public string CreatedBy { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ApplicationForm.Fields.UpdatedBy")]
        public string UpdatedBy { get; set; }


        public PhysicalTradingAddressModel PhysicalTradingAddressModel { get; set; }
        public BankingDetailsModel BankingDetailsModel { get; set; }
        public TradeReferencesModel TradeReferencesModel { get; set; }
        public PremisesModel PremisesModel { get; set; }

        #endregion
    }
}
