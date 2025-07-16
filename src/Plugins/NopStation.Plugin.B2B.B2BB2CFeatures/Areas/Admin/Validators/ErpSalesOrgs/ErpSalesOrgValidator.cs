using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpSalesOrgs;

public partial class ErpSalesOrgValidator : BaseNopValidator<ErpSalesOrgModel>
{
    public ErpSalesOrgValidator(ILocalizationService localizationService,
        AddressSettings addressSettings)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Name"));

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Code"));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Email"));

        RuleFor(x => x.Address.FirstName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.FirstName"));

        RuleFor(x => x.Address.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Email"));

        RuleFor(x => x.Address.CountryId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.CountryId"));

        RuleFor(x => x.Address.ZipPostalCode)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ZipPostalCode"));

        RuleFor(x => x.Address.City)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.City"));

        RuleFor(x => x.Address.PhoneNumber)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.PhoneNumber"));

        //from address settings
        if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
        {
            RuleFor(x => x.Address.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Company"));
        }
        if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
        {
            RuleFor(x => x.Address.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Address1"));
        }
        if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
        {
            RuleFor(x => x.Address.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.Address2"));
        }
        if (addressSettings.CountyEnabled && addressSettings.CountyRequired)
        {
            RuleFor(x => x.Address.County).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.County"));
        }
        if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
        {
            RuleFor(x => x.Address.FaxNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.FaxNumber"));
        }

        SetDatabaseValidationRules<ErpSalesOrg>();
    }
}