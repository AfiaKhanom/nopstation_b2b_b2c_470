using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Payments.B2BAccount.Models;

namespace NopStation.Plugin.Payments.B2BAccount.Validators;

public partial class PaymentInfoValidator : BaseNopValidator<B2BAccountPaymentInfoModel>
{
    public PaymentInfoValidator(ILocalizationService localizationService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        if (b2BB2CFeaturesSettings.IsCustomerReferenceRequiredDuringPayment)
        {
            RuleFor(x => x.CustomerReferenceAsPO)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.CustomerReferenceAsPORequired"));

            if (b2BB2CFeaturesSettings.PreventSpecialCharactersInCustomerReference)
            {
                var specialCharactersToPrevent = b2BB2CFeaturesSettings.SpecialCharactersToPreventInCustomerReference.Trim();
                if (string.IsNullOrEmpty(specialCharactersToPrevent) || specialCharactersToPrevent.ToLower() == "all")
                {
                    RuleFor(x => x.CustomerReferenceAsPO)
                        .Matches("^[a-zA-Z0-9\x20]+$")
                        .WithMessage(string.Format(localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.Invalid").Result, specialCharactersToPrevent));
                }
                else
                {
                    specialCharactersToPrevent = specialCharactersToPrevent.Replace("\"", "/\\\"");
                    RuleFor(x => x.CustomerReferenceAsPO)
                        .Matches("^[^" + specialCharactersToPrevent + "\r\n]+$")
                        .WithMessage(string.Format(localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.Invalid").Result, specialCharactersToPrevent));
                }
            }
            RuleFor(x => x.CustomerReferenceAsPO).MustAwait(async (x, context) =>
            {
                if (b2BB2CFeaturesSettings.MaintainUniqueCustomerReference)
                {
                    return !await erpOrderAdditionalDataService.IfCustomerReferenceExistWithThisErpAccount(x.CustomerReferenceAsPO, x.ErpAccountId);
                }
                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.AlreadyExist"));
        }
    }
}