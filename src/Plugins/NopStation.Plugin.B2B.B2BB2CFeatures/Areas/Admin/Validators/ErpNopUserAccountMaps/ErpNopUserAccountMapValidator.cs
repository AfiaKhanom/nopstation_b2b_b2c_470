using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpNopUserAccountMaps
{
    public partial class ErpNopUserAccountMapValidator : BaseNopValidator<ErpNopUserAccountMapModel>
    {
        public ErpNopUserAccountMapValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ErpAccountId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpAccountId"));

            RuleFor(x => x.SelectedCustomerRoleIds)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.RequiredErrMsg.ErpUserRoleId"));

            SetDatabaseValidationRules<ErpNopUserAccountMap>();
        }
    }
}