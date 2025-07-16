using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpOrderAdditionalDataValidator : BaseNopValidator<ErpOrderAdditionalData>
{
    #region Ctor

    public ErpOrderAdditionalDataValidator()
    {
        // Required integer fields
        RuleFor(x => x.NopOrderId)
            .GreaterThan(0);

        RuleFor(x => x.ErpOrderOriginTypeId)
            .GreaterThan(0);

        RuleFor(x => x.ErpOrderTypeId)
            .GreaterThan(0);

        RuleFor(x => x.OrderPlacedByNopCustomerId)
            .GreaterThan(0);

        RuleFor(x => x.ErpAccountId)
            .GreaterThan(0);

        RuleFor(x => x.IntegrationStatusTypeId)
            .GreaterThan(0);

        RuleFor(x => x.ErpOrderPlaceByCustomerTypeId)
            .GreaterThan(0);

        RuleFor(x => x.ChangedById)
            .GreaterThan(0);

        // Optional integer fields
        RuleFor(x => x.QuoteSalesOrderId)
            .GreaterThan(0)
            .When(x => x.QuoteSalesOrderId.HasValue);

        RuleFor(x => x.ErpShipToAddressId)
            .GreaterThan(0)
            .When(x => x.ErpShipToAddressId.HasValue);

        RuleFor(x => x.IntegrationRetries)
            .GreaterThanOrEqualTo(0)
            .When(x => x.IntegrationRetries.HasValue);

        // Required string fields
        RuleFor(x => x.ErpOrderNumber)
            .NotEmpty();

        RuleFor(x => x.ERPOrderStatus)
            .NotEmpty();

        // Optional string fields
        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(int.MaxValue)
            .When(x => !string.IsNullOrEmpty(x.SpecialInstructions));

        RuleFor(x => x.CustomerReference)
            .MaximumLength(int.MaxValue)
            .When(x => !string.IsNullOrEmpty(x.CustomerReference));

        RuleFor(x => x.IntegrationError)
            .MaximumLength(int.MaxValue)
            .When(x => !string.IsNullOrEmpty(x.IntegrationError));

        // Required boolean fields
        RuleFor(x => x.IsShippingAddressModified)
            .NotNull();

        // Optional boolean fields
        RuleFor(x => x.IsOrderPlaceNotificationSent)
            .Must(x => true)
            .When(x => x.IsOrderPlaceNotificationSent.HasValue);

        // Optional DateTime fields
        RuleFor(x => x.QuoteExpiryDate)
            .Must(x => true)
            .When(x => x.QuoteExpiryDate.HasValue);

        RuleFor(x => x.DeliveryDate)
            .Must(x => true)
            .When(x => x.DeliveryDate.HasValue);

        RuleFor(x => x.IntegrationErrorDateTimeUtc)
            .Must(x => true)
            .When(x => x.IntegrationErrorDateTimeUtc.HasValue);

        RuleFor(x => x.LastERPUpdateUtc)
            .Must(x => true)
            .When(x => x.LastERPUpdateUtc.HasValue);

        RuleFor(x => x.ChangedOnUtc)
            .Must(x => true)
            .When(x => x.ChangedOnUtc.HasValue);
    }

    #endregion
}