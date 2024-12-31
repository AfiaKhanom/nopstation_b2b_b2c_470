using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;
using Microsoft.AspNetCore.Routing.Constraints;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var lang = GetLanguageRoutePattern();

            //Quick Order
            endpointRouteBuilder.MapControllerRoute(name: "QuickOrder",
              pattern: $"{lang}/Favourites/",
              defaults: new { controller = "QuickOrder", action = "QuickOrderTemplateList" });

            endpointRouteBuilder.MapControllerRoute(name: "QuickOrderDetails",
              pattern: $"{lang}/FavouritesDetails/{{id:min(0)}}",
              defaults: new { controller = "QuickOrder", action = "QuickOrderTemplateDetails" });

            //logout
            endpointRouteBuilder.MapControllerRoute(name: "Logout",
                pattern: $"{lang}/logout/",
                defaults: new { controller = "CustomerImpersonate", action = "Logout" });

            //QuoteOrderList
            endpointRouteBuilder.MapControllerRoute(name: "QuoteDetails",
                pattern: $"{lang}/QuoteDetails/",
                defaults: new { controller = "OverridenOrder", action = "QuoteDetails" });

            //B2BRegister
            endpointRouteBuilder.MapControllerRoute(name: "B2BRegister",
                pattern: $"{lang}/B2BRegister/",
                defaults: new { controller = "B2BB2CCustomer", action = "B2BRegister" });

            //B2BRegister
            endpointRouteBuilder.MapControllerRoute(name: "B2CRegister",
                pattern: $"{lang}/B2CRegister/",
                defaults: new { controller = "B2BB2CCustomer", action = "B2CRegister" });

            //B2BRegister
            endpointRouteBuilder.MapControllerRoute(name: "ErpAccountCustomerRegistrationForm",
                pattern: $"{lang}/ErpAccountCustomerRegistrationApplication/",
                defaults: new { controller = "B2BB2CCustomer", action = "ErpAccountCustomerRegistrationForm" });

            endpointRouteBuilder.MapControllerRoute(name: "ErpAccountInvoices",
                pattern: $"{lang}/MyAccount/AccountTransactions",
                defaults: new { controller = "ErpAccountPublic", action = "ErpAccountInvoices" });

            endpointRouteBuilder.MapControllerRoute(name: "ErpAccountInfo",
                pattern: $"{lang}/MyAccount/ERPAccountInfo",
                defaults: new { controller = "ErpAccountPublic", action = "ErpAccountInfo" });

            endpointRouteBuilder.MapControllerRoute(name: "IsCartItemsLivePriceSyncProcessing",
                pattern: $"{lang}/IsCartItemsLivePriceSyncProcessing/",
                defaults: new { controller = "ErpCheckout", action = "IsCartItemsLivePriceSyncProcessing" });

            endpointRouteBuilder.MapControllerRoute(name: "LoadB2BCartItemData",
                pattern: $"{lang}/LoadB2BCartItemData/",
                defaults: new { controller = "OverridenShoppingCart", action = "LoadB2BCartItemData" });

            endpointRouteBuilder.MapControllerRoute(name: "LoadB2BOrderItemData",
                pattern: $"{lang}/LoadB2BOrderItemData/",
                defaults: new { controller = "ErpCheckout", action = "LoadB2BOrderItemData" });

            endpointRouteBuilder.MapControllerRoute(name: "CheckCartItemQuotePriceChangeWarning",
                pattern: $"{lang}/CheckCartItemQuotePriceChangeWarning/",
                defaults: new { controller = "OverridenShoppingCart", action = "CheckCartItemQuotePriceChangeWarning" });

            endpointRouteBuilder.MapControllerRoute(name: "CurrentCartItemsLivePriceCheck",
                pattern: $"{lang}/CurrentCartItemsLivePriceCheck/",
                defaults: new { controller = "HandleLiveERPCall", action = "CurrentCartItemsLivePriceCheck" });

            endpointRouteBuilder.MapControllerRoute(name: "CurrentCartItemsLiveStockCheck",
                pattern: $"{lang}/CurrentCartItemsLiveStockCheck/",
                defaults: new { controller = "HandleLiveERPCall", action = "CurrentCartItemsLiveStockCheck" });

            endpointRouteBuilder.MapControllerRoute(name: "QuoteOrder",
                pattern: $"{lang}/QuoteOrder/",
                defaults: new { controller = "QuoteOrder", action = "Index" });

            endpointRouteBuilder.MapControllerRoute(name: "ErpQuoteOrderList",
                pattern: $"{lang}/ErpQuoteOrderList/",
                defaults: new { controller = "ErpAccountPublic", action = "ErpAccountQuoteOrders" });

            endpointRouteBuilder.MapControllerRoute(name: "IsItemsInCart",
                pattern: $"{lang}/IsItemsInCart/",
                defaults: new { controller = "OverridenOrder", action = "IsItemsInCart" });

            endpointRouteBuilder.MapControllerRoute(name: "B2BClearCart",
                pattern: $"{lang}/B2BClearCart/",
                defaults: new { controller = "OverridenShoppingCart", action = "ClearCart" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerImpersonateList",
                pattern: $"{lang}/CustomerImpersonate/List",
                defaults: new { controller = "CustomerImpersonate", action = "List" });

            endpointRouteBuilder.MapControllerRoute(name: "ErpAccountOrders",
                pattern: $"{lang}/ERPOrders/History",
                defaults: new { controller = "ErpAccountPublic", action = "ErpAccountOrders" });

            endpointRouteBuilder.MapControllerRoute(name: "GetDeliveryDatesBySuburbOrCity",
                pattern: $"{lang}/GetERPDeliveryDates/",
                defaults: new { controller = "ErpCheckout", action = "GetERPDeliveryDates" });

            endpointRouteBuilder.MapControllerRoute(name: "GetCountryTwoLetterIsoCode",
                pattern: $"GetCountryTwoLetterIsoCode/{{code?}}",
                defaults: new { controller = "B2BB2CCustomer", action = "GetCountryTwoLetterIsoCode" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("GET") });
        }

        #region Properties

        public int Priority => 1;

        #endregion
    }
}