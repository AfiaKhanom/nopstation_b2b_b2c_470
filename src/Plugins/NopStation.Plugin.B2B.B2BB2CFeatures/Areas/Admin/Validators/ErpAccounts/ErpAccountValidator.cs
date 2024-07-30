using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpAccounts
{
    public partial class ErpAccountValidator : BaseNopValidator<ErpAccountModel>
    {
        public ErpAccountValidator(ILocalizationService localizationService,
            AddressSettings addressSettings)
        {
            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.AccountNumber"));

            RuleFor(x => x.AccountName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.AccountName"));

            RuleFor(x => x.ErpSalesOrgId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.ErpSalesOrgId"));
             
            RuleFor(x => x.BillingAddress.FirstName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.FirstName"));

            RuleFor(x => x.BillingAddress.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Email"));

            RuleFor(x => x.BillingAddress.ZipPostalCode)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.ZipPostalCode"));

            RuleFor(x => x.BillingAddress.City)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.City"));

            RuleFor(x => x.BillingAddress.CountryId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.CountryId"));

            RuleFor(x => x.BillingAddress.PhoneNumber)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.PhoneNumber"));

            //from address settings
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.BillingAddress.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Company"));
            }

            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.BillingAddress.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Address1"));
            }

            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.BillingAddress.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.Address2"));
            }

            if (addressSettings.CountyEnabled && addressSettings.CountyRequired)
            {
                RuleFor(x => x.BillingAddress.County).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.County"));
            }

            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.BillingAddress.FaxNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.RequiredErrMsg.FaxNumber"));
            }

            SetDatabaseValidationRules<ErpAccount>();
        }
    }
}