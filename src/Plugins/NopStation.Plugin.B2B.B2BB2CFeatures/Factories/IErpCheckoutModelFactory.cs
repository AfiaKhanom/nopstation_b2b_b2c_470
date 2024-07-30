using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public interface IErpCheckoutModelFactory
    {
        Task PrepareB2BShipToAddressModelAsync(ErpShipToAddressModelForCheckout b2BShipToAddressModel, ErpShipToAddress b2BShipToAddress, ErpAccount b2BAccount, bool loadAvailableAreas = false, bool loadCountriesAndStates = false);
        Task PrepareB2CShipToAddressModelAsync(ErpShipToAddressModelForCheckout b2BShipToAddressModel, ErpShipToAddress b2CShipToAddress, ErpAccount b2BAccount, bool loadAvailableAreas = false, bool loadCountriesAndStates = false);

        Task<CheckoutErpBillingAddressModel> PrepareCheckoutErpBillingAddressModelAsync(IList<ShoppingCartItem> cart, ErpAccount b2BAccount);

        Task<ErpOnePageCheckoutModel> PrepareB2BOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2BUser);
        Task<ErpOnePageCheckoutModel> PrepareB2COnePageCheckoutModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2CUser);

        Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2BShippingAddressModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2BUser, ErpAccount b2BAccount);

        Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2CShippingAddressModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2CUser, ErpAccount b2BAccount);
        Task<ErpCheckoutShippingAddressModel> PrepareShippingAddressModelAsync(ErpNopUser b2BUser, ErpAccount b2BAccount, int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");

        Task<(IList<SelectListItem>, bool)> GetDeliveryDatesBySuburbOrCityAsync(string suburb, string city);
    }
}