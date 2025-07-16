using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpSalesOrgs;

public partial class ErpShipToAddressValidator : BaseNopValidator<ErpShipToAddressModel>
{
    public ErpShipToAddressValidator(AddressSettings addressSettings,
        ILocalizationService localizationService,
        IStateProvinceService stateProvinceService)
    {
        RuleFor(x => x.ShipToCode)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.ShipToCode"));

        RuleFor(x => x.ShipToName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.ShipToName"));
         
        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.ErpAccountId"));

        RuleFor(x => x.RepNumber)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.RepNumber"));

        //for address model, these are fixed from factory
        RuleFor(x => x.AddressModel.FirstName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.FirstName"));

        RuleFor(x => x.AddressModel.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.Email"));

        RuleFor(x => x.AddressModel.CountryId)
           .GreaterThan(0)
           .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.CountryId"));

        RuleFor(x => x.AddressModel.City)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.City"));

        RuleFor(x => x.AddressModel.ZipPostalCode)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.ZipPostalCode"));

        RuleFor(x => x.AddressModel.PhoneNumber)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpShipToAddress.RequiredErrMsg.ValueCanNotBeEmpty.PhoneNumber"));

        //depending on address settings
        if (addressSettings.CountyEnabled && addressSettings.CountyRequired)
        {
            RuleFor(x => x.AddressModel.County).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.County"));
        }
        if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
        {
            RuleFor(x => x.AddressModel.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Company.Required"));
        }
        if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
        {
            RuleFor(x => x.AddressModel.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required"));
        }
        if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
        {
            RuleFor(x => x.AddressModel.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress2.Required"));
        }
        if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
        {
            RuleFor(x => x.AddressModel.FaxNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Customers.Customers.Fields.Fax.Required"));
        }

        SetDatabaseValidationRules<ErpShipToAddress>();
    }
}