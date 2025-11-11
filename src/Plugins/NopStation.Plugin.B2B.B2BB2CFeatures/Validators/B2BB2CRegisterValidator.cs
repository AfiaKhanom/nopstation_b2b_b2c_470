using System.Linq;
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;

public partial class B2BB2CRegisterValidator : BaseNopValidator<B2BRegisterModel>
{
    public B2BB2CRegisterValidator(ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        CustomerSettings customerSettings)
    {
        //for b2b user
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .When(x => x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountNumber.Required"));
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .When(x => x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountName.Required"));

        //for b2c user
        RuleFor(x => x.B2CIdentificationNumber)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.B2CIdentificationNumber.Required"));
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountName.Required"));
        RuleFor(x => x.ZipPostalCode)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ZipPostalCodeRequired"));
        RuleFor(x => x.Phone)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.PhoneRequired"));
        RuleFor(x => x.StreetAddress)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.StreetAddressRequired"));
        RuleFor(x => x.StreetAddress2)
             .NotEmpty()
             .When(x => !x.IsB2BUser && customerSettings.StreetAddress2Required)
             .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.StreetAddress2Required"));
        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Country.Required"));
        RuleFor(x => x.City)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));

        RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.Required"));
        RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ValidEmailRequired"));

        if (customerSettings.FirstNameEnabled && customerSettings.FirstNameRequired)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.FirstName.Required"));
        }
        if (customerSettings.LastNameEnabled && customerSettings.LastNameRequired)
        {
            RuleFor(x => x.LastName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.LastName.Required"));
        }
        if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
        {
            //entered?
            RuleFor(x => x.DateOfBirthDay).Must((x, context) =>
            {
                var dateOfBirth = x.ParseDateOfBirth();
                if (!dateOfBirth.HasValue)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.DateOfBirthDay.Required"));
        }

        //Password rule
        RuleFor(x => x.Password).IsPassword(localizationService, customerSettings);
        RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.ConfirmPassword.Required"));
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Password.EnteredPasswordsDoNotMatch"));

        //form fields
        if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
        {
            RuleFor(x => x.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Company.Required"));
        }

        if (customerSettings.CountryEnabled &&
            customerSettings.StateProvinceEnabled &&
            customerSettings.StateProvinceRequired)
        {
            RuleFor(x => x.StateProvinceId).MustAwait(async (x, context) =>
            {
                //does selected country have states?
                var hasStates = (await stateProvinceService.GetStateProvincesByCountryIdAsync(x.CountryId)).Any();
                if (hasStates)
                {
                    //if yes, then ensure that a state is selected
                    if (x.StateProvinceId == 0)
                        return false;
                }

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StateProvince.Required"));
        }
        if (customerSettings.CountyRequired && customerSettings.CountyEnabled)
        {
            RuleFor(x => x.County)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.County.Required"));
        }
    }
}