using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality
{
    public interface IErpCustomerFunctionalityService
    {
        void ClearGenericAttributeOfB2BQuoteOrder();
        void ClearGenericAttributeOfB2CQuoteOrder();
        Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(int erpOrderId, IList<ShoppingCartItem> currentShoppingCartItems);
        Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(ErpOrderAdditionalData b2BOrderPerAccount, IList<ShoppingCartItem> shoppingCartItems);
        Task<bool> CheckAndUpdateGenericAttributeOfB2CQuoteOrder(int erpOrderId);
        Task<bool> IsCustomerInB2BCustomerRole(Customer customer);
        Task<bool> IsErpAccountBlockSalesOrderAsync(Customer customer);
        Task<ErpNopUser> GetActiveErpNopUserByCustomerAsync(Customer customer);
        Task<bool> IsCurrentCustomerInB2BQuoteAssistantRole();
        Task<bool> IsCustomerInB2BQuoteAssistantRole(Customer customer);
        Task<bool> IsConsideredAsB2BOrderByB2BUserInformation(ErpNopUser b2BUser);
        Task<bool> IsConsideredAsB2COrderByB2CUser(ErpNopUser b2CUser);

        Task<ErpAccount> GetActiveErpAccountByCustomerAsync(Customer customer);

        Task<bool> IsSalesOrderInvalidForCurrentCustomerAsync();
        Task<bool> IsCurrentCustomerInErpSalesRepRoleAsync();
        Task<bool> IsCustomerInB2BSalesRepRoleAsync(Customer customer);
        Task<bool> IsCurrentCustomerInAdministratorRoleAsync();
        Task<bool> IsCurrentCustomerInAdministratorRoleAsync(Customer customer);
        Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrder);
        Task<bool> CheckAllowAddressEdit(ErpAccount b2BAccount);
        Task<(DateTime, DateTime)> GetMinimumAndMaximumDeliveryDateForShippingAddress();
        Task ClearCurrentCustomerYearlySavingsCacheAsync(int customerId);
        Task ClearCurrentCustomerAllTimeSavingsCacheAsync(int customerId);
    }
}