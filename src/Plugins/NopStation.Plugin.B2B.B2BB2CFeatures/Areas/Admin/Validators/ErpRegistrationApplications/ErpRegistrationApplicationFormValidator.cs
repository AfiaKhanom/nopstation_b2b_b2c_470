using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpRegistrationApplication;

public partial class ErpRegistrationApplicationFormValidator : BaseNopValidator<ApplicationFormModel>
{
    public ErpRegistrationApplicationFormValidator(ILocalizationService localizationService,
        AddressSettings addressSettings)
    {
        RuleFor(x => x.FullRegisteredName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg.FullRegisteredName"));

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpRegistrationApplication.ApplicationForm.RequiredErrMsg."));
         
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
            RuleFor(x => x.RegisteredOfficeAddress.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Company"));
        }

        if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
        {
            RuleFor(x => x.RegisteredOfficeAddress.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Address1"));
        }

        if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
        {
            RuleFor(x => x.RegisteredOfficeAddress.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Address2"));
        }

        if (addressSettings.CountyEnabled && addressSettings.CountyRequired)
        {
            RuleFor(x => x.RegisteredOfficeAddress.County).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.County"));
        }

        if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
        {
            RuleFor(x => x.RegisteredOfficeAddress.FaxNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.FaxNumber"));
        }

        SetDatabaseValidationRules<ErpAccountCustomerRegistrationForm>();
    }
}