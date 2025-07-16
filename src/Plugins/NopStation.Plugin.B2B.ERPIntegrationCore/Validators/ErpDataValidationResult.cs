namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public record ErpDataValidationResult
{
    public string Property { get; set; }

    public string Error { get; set; }
}
