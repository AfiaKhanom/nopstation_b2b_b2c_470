using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

public interface IErpCustomerFunctionalityService
{
    void ClearGenericAttributeOfB2BQuoteOrder();
    void ClearGenericAttributeOfB2CQuoteOrder();
    Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(int erpOrderId, IList<ShoppingCartItem> currentShoppingCartItems);
    Task<bool> CheckAndUpdateGenericAttributeOfERPQuoteOrder(ErpOrderAdditionalData b2BOrderPerAccount, IList<ShoppingCartItem> shoppingCartItems);
    Task<bool> CheckAndUpdateGenericAttributeOfB2CQuoteOrder(int erpOrderId, IList<ShoppingCartItem> currentShoppingCartItems);
    Task<bool> IsErpAccountBlockSalesOrderAsync(Customer customer);
    Task<ErpNopUser> GetActiveErpNopUserByCustomerAsync(Customer customer);
    Task<bool> IsConsideredAsB2BOrderByB2BUser(ErpNopUser b2BUser);
    Task<bool> IsConsideredAsB2COrderByB2CUser(ErpNopUser b2CUser);
    Task<ErpAccount> GetActiveErpAccountByCustomerAsync(Customer customer);
    Task<bool> IsSalesOrderInvalidForCurrentCustomerAsync();
    Task<bool> IsCurrentCustomerInErpSalesRepRoleAsync();
    Task<bool> IsCurrentCustomerInAdministratorRoleAsync();
    Task<bool> IsCurrentCustomerInAdministratorRoleAsync(Customer customer);
    Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrder);
    Task<bool> CheckAllowAddressEdit(ErpAccount b2BAccount);
    Task<(DateTime, DateTime)> GetMinimumAndMaximumDeliveryDateForShippingAddress();
    Task ClearCurrentCustomerYearlySavingsCacheAsync(int customerId);
    Task ClearCurrentCustomerAllTimeSavingsCacheAsync(int customerId);
    Task<bool> IsCustomerInB2BCustomerRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2BCustomerRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2BCustomerRoleAsync();
    Task<bool> IsCustomerInB2CCustomerRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2CCustomerRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2CCustomerRoleAsync();
    Task<bool> IsCustomerInB2BQuoteAssistantRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2BQuoteAssistantRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2BQuoteAssistantRoleAsync();
    Task<bool> IsCustomerInB2BOrderAssistantRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2BOrderAssistantRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2BOrderAssistantRoleAsync();
    Task<bool> IsCustomerInB2BB2CAdminRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2BB2CAdminRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2BB2CAdminRoleAsync();
    Task<bool> IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(Customer customer);
    Task<bool> IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInB2BCustomerAccountingPersonnelRoleAsync();
    Task<bool> IsCustomerInB2BSalesRepRoleAsync(int customerId = 0);
    Task<bool> IsCustomerInB2BSalesRepRoleAsync(Customer customer);
    Task<bool> IsCurrentCustomerInB2BSalesRepRoleAsync();
    Task<bool> IsCustomerInQuickOrderUserRoleAsync(Customer customer);
    Task<bool> IsCustomerInQuickOrderUserRoleAsync(int customerId = 0);
    Task<bool> IsCurrentCustomerInQuickOrderUserRoleAsync();
}