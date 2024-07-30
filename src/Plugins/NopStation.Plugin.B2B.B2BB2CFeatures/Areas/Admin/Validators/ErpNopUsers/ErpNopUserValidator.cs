using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpSalesOrgs
{
    public partial class ErpNopUserAccountMapValidator : BaseNopValidator<ErpNopUserModel>
    {
        public ErpNopUserAccountMapValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.NopCustomerId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.NopCustomerId"));

            RuleFor(x => x.ErpAccountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpAccountId"));

            RuleFor(x => x.ErpShipToAddressId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpShipToAddressId"));

            //RuleFor(x => x.BillingErpShipToAddressId)
            //    .GreaterThan(0)
            //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.BillingErpShipToAddressId"));

            //RuleFor(x => x.ShippingErpShipToAddressId)
            //    .GreaterThan(0)
            //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ShippingErpShipToAddressId"));

            RuleFor(x => x.SelectedCustomerRoleIds)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpUserRoleId"));

            RuleFor(x => x.ErpUserTypeId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpUserTypeId"));

            SetDatabaseValidationRules<ErpNopUser>();
        }
    }
}