namespace NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

public enum IntegrationStatusType
{
    WaitingForPayment = 5,

    Queued = 10,

    Sent = 20,

    Confirmed = 30,

    Failed = 40,

    Processing = 50,

    Cancelled = 60
}