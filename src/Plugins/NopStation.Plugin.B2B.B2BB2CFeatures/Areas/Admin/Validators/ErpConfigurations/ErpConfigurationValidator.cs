using System.Net.Mail;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.ErpConfigurations;
public partial class ErpConfigurationValidator:BaseNopValidator<ConfigurationModel>
{
    public ErpConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.StockDisplayFormatId)
            .NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.Configuration.Fields.ValidationMsg.StockDisplayFormatId"));
        
    }

    
}
