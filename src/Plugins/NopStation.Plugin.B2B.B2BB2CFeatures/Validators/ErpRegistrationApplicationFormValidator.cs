using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators
{
    public partial class ErpRegistrationApplicationFormValidator : BaseNopValidator<ErpAccountCustomerRegistrationFormModel>
    {
        public ErpRegistrationApplicationFormValidator(ILocalizationService localizationService,
            AddressSettings addressSettings)
        {
            RuleFor(x => x.FullRegisteredName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.FullRegisteredName"));

            RuleFor(x => x.RegistrationNumber)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.RegistrationNumber"));
             
            RuleFor(x => x.VatNumber)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.VatNumber"));

            RuleFor(x => x.TelephoneNumber1)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.TelephoneNumber1"));

            RuleFor(x => x.AccountsContactPersonNameSurname)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.AccountsContactPersonNameSurname"));

            RuleFor(x => x.AccountsEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.AccountsEmail"));

            RuleFor(x => x.BuyerContactPersonNameSurname)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.BuyerContactPersonNameSurname"));

            RuleFor(x => x.BuyerEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.BuyerEmail"));
                        
            //from address settings
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.RegisteredOfficeAddress.Company_ROA).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.Company"));
            }

            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.RegisteredOfficeAddress.Address1_ROA).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.Address1"));
            }

            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.RegisteredOfficeAddress.Address2_ROA).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.Address2"));
            }

            if (addressSettings.CountyEnabled && addressSettings.CountyRequired)
            {
                RuleFor(x => x.RegisteredOfficeAddress.County_ROA).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.County"));
            }

            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.RegisteredOfficeAddress.FaxNumber_ROA).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.FaxNumber"));
            }

            SetDatabaseValidationRules<ErpAccountCustomerRegistrationForm>();
        }
    }
}