using System.Linq;
using FluentValidation.Results;
using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

public static class ErpDataValidationHelper
{
    #region Methods

    public static string PrepareValidationLog(ValidationResult validationResult)
    {
        if (validationResult == null)
        {
            return string.Empty;
        }

        var errorMessages = validationResult.Errors.Select(error => new ErpDataValidationResult
        {
            Property = error.PropertyName,
            Error = error.ErrorMessage
        });

        return JsonConvert.SerializeObject(errorMessages, Formatting.Indented);
    }

    #endregion
}
