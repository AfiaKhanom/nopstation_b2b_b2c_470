using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality
{
    public class ErpCustomerFunctionalityService : IErpCustomerFunctionalityService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

        #endregion

        #region Ctor

        public ErpCustomerFunctionalityService(ICustomerService customerService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpAccountService erpAccountService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderService orderService,
            IErpNopUserService erpNopUserService,
            IStaticCacheManager staticCacheManager,
            B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
        {
            _customerService = customerService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpAccountService = erpAccountService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderService = orderService;
            _erpNopUserService = erpNopUserService;
            _staticCacheManager = staticCacheManager;
            _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        }

        #endregion

        #region Methods

        public async void ClearGenericAttributeOfB2BQuoteOrder()
        {
            var currStore = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync<int?>(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, null, currStore.Id);
        }

        public async void ClearGenericAttributeOfB2CQuoteOrder()
        {
            var currStore = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync<int?>(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId, null, currStore.Id);
        }

        public async Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(int erpOrderId, IList<ShoppingCartItem> currentShoppingCartItems)
        {
            var b2BOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderId);

            return await CheckAndUpdateGenericAttributeOfB2BQuoteOrder(b2BOrderPerAccount, currentShoppingCartItems);
        }

        public async Task<bool> CheckAndUpdateGenericAttributeOfB2CQuoteOrder(int erpOrderId)
        {
            var b2COrderPerUser = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderId);
            return await CheckAndUpdateGenericAttributeOfB2CQuoteOrder(b2COrderPerUser.Id);
        }

        public async Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(ErpOrderAdditionalData b2BOrderPerAccount, IList<ShoppingCartItem> shoppingCartItems)
        {
            if (!await _erpOrderAdditionalDataService.CheckQuoteOrderStatusAsync(b2BOrderPerAccount))
            {
                ClearGenericAttributeOfB2BQuoteOrder();
                return false;
            }

            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            if (shoppingCartItems == null || !shoppingCartItems.Any())
            {
                ClearGenericAttributeOfB2BQuoteOrder();
                return false;
            }

            var orderItems = await _orderService.GetOrderItemsAsync(b2BOrderPerAccount.NopOrderId);
            if (orderItems == null || !orderItems.Any())
            {
                ClearGenericAttributeOfB2BQuoteOrder();
                return false;
            }

            var isQuoteItemExist = shoppingCartItems.Any(s => orderItems.Any(o => o.ProductId == s.ProductId));
            if (!isQuoteItemExist)
            {
                ClearGenericAttributeOfB2BQuoteOrder();
                return false;
            }

            return true;
        }

        public async Task<bool> IsCustomerInB2BCustomerRole(Customer customer)
        {
            return await _customerService.IsInCustomerRoleAsync(customer, B2BB2CFeaturesDefaults.B2BCustomerRoleSystemName);
        }

        public async Task<bool> IsCurrentCustomerInB2BQuoteAssistantRole()
        {
            return await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2BQuoteAssistantRoleSystemName);
        }

        public async Task<bool> IsCustomerInB2BQuoteAssistantRole(Customer customer)
        {
            return await _customerService.IsInCustomerRoleAsync(customer, B2BB2CFeaturesDefaults.B2BQuoteAssistantRoleSystemName);
        }

        public async Task<ErpAccount> GetActiveErpAccountOfCurrentCustomer()
        {
            return await GetActiveErpAccountByCustomerAsync(await _workContext.GetCurrentCustomerAsync());
        }

        public async Task<ErpAccount> GetActiveErpAccountByCustomerIdAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return null;

            return await GetActiveErpAccountByCustomerAsync(customer);
        }

        public async Task<ErpAccount> GetActiveErpAccountByCustomerAsync(Customer customer)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpAccountByCustomerCacheKey, customer.Id, string.Join(",", await _customerService.GetCustomerRoleIdsAsync(customer)));

            return await _staticCacheManager.Get(key, async () =>
            {
                var erpNopUser = await GetActiveErpNopUserByCustomerAsync(customer);
                if (erpNopUser != null && !erpNopUser.IsDeleted && erpNopUser.IsActive)
                {
                    var erpAccount = await _erpAccountService.GetErpAccountByIdWithActiveAsync(erpNopUser.ErpAccountId);
                    if (erpAccount != null)
                        return erpAccount;
                }
                return null;
            });
        }

        public async Task<bool> IsErpAccountBlockSalesOrderAsync(Customer customer)
        {
            if (await IsCustomerInB2BCustomerRole(customer))
            {
                var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
                if (b2bAccount == null || b2bAccount.ErpAccountStatusType == ErpAccountStatusType.BlockOrder)
                    return true;
            }

            return false;
        }

        public async Task<ErpNopUser> GetActiveErpNopUserByCustomerAsync(Customer customer)
        {
            if (customer == null)
                return null;

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
            if (erpNopUser != null && !erpNopUser.IsDeleted && erpNopUser.IsActive)
            {
                return erpNopUser;
            }
            return null;
        }

        public async Task<bool> IsConsideredAsB2BOrderByB2BUserInformation(ErpNopUser b2BUser)
        {
            if (b2BUser == null || !b2BUser.IsActive)
            {
                return false;
            }
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(b2BUser.ErpAccountId);
            if (erpAccount == null || erpAccount.IsDeleted || !erpAccount.IsActive)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> IsConsideredAsB2COrderByB2CUser(ErpNopUser b2CUser)
        {
            if (b2CUser == null || !b2CUser.IsActive)
                return false;

            var b2BAccount = await _erpAccountService.GetErpAccountByIdWithActiveAsync(b2CUser.ErpAccountId);
            if (b2BAccount == null)
                return false;

            return true;
        }

        public async Task<bool> IsSalesOrderInvalidForCurrentCustomerAsync()
        {
            bool isQuoteOrder;
            var currCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var nopUser = await GetActiveErpNopUserByCustomerAsync(currCustomer);
            var b2bUser = nopUser?.ErpUserType == ErpUserType.B2BUser ? nopUser : null;
            if (b2bUser != null && b2bUser.Id > 0)
            {
                isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currCustomer, B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, store.Id);
                if (isQuoteOrder)
                    return false;
            }

            var b2CUser = nopUser?.ErpUserType == ErpUserType.B2CUser ? nopUser : null;
            if (b2CUser != null && b2CUser.Id > 0)
            {
                isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currCustomer, B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, store.Id);
                if (isQuoteOrder)
                    return false;
            }

            return await IsErpAccountBlockSalesOrderAsync(currCustomer);
        }

        public async Task<bool> IsCurrentCustomerInErpSalesRepRoleAsync()
        {
            return await IsCustomerInB2BSalesRepRoleAsync(await _b2BB2CWorkContext.GetCurrentCustomerAsync());
        }

        public async Task<bool> IsCustomerInB2BSalesRepRoleAsync(Customer customer)
        {
            return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
        }

        public async Task<bool> IsCurrentCustomerInAdministratorRoleAsync()
        {
            return await IsCurrentCustomerInAdministratorRoleAsync(await _b2BB2CWorkContext.GetCurrentCustomerAsync());
        }

        public async Task<bool> IsCurrentCustomerInAdministratorRoleAsync(Customer customer)
        {
            return await _customerService.IsAdminAsync(customer);
        }

        public async Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrder)
        {
            if (erpOrder == null || erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder)
                return false;

            if (erpOrder.QuoteExpiryDate == null || string.IsNullOrEmpty(erpOrder.ERPOrderStatus))
                return false;

            if (erpOrder.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date)
                return false;

            // a quote can be placed only once (so if any order placed already with this quote order then it is false)
            if (erpOrder.QuoteSalesOrderId.HasValue && erpOrder.QuoteSalesOrderId.Value > 0)
                return false;

            return (erpOrder.ERPOrderStatus == B2BB2CFeaturesDefaults.ErpOrderStatusApproved || erpOrder.ERPOrderStatus == B2BB2CFeaturesDefaults.ErpOrderStatusPendingApproval) ? true : false;
        }

        public async Task<bool> CheckAllowAddressEdit(ErpAccount b2BAccount)
        {
            if (b2BAccount == null)
            {
                return false;
            }

            // if Override address edit Config Setting, then we take result from b2BAccount, otherwise from configuration settings
            return b2BAccount.OverrideAddressEditOnCheckoutConfigSetting ? b2BAccount.AllowAccountsAddressEditOnCheckout :
            _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
        }

        public async Task<(DateTime, DateTime)> GetMinimumAndMaximumDeliveryDateForShippingAddress()
        {
            var minDeliveryDate = DateTime.Now.AddDays(_b2BB2CFeaturesSettings.DeliveryDays);
            if (minDeliveryDate.Hour > _b2BB2CFeaturesSettings.CutoffTime)
                minDeliveryDate = minDeliveryDate.AddDays(1);
            var maxDeliveryDate = DateTime.Now.AddMonths(6);

            return (minDeliveryDate, maxDeliveryDate);
        }

        public async Task ClearCurrentCustomerYearlySavingsCacheAsync(int customerId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpUserCurrentYearSavingsByCustomerCacheKey, customerId);

            await _staticCacheManager.RemoveAsync(cacheKey);
        }

        public async Task ClearCurrentCustomerAllTimeSavingsCacheAsync(int customerId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpUserAllTimeSavingsByCustomerCacheKey, customerId);

            await _staticCacheManager.RemoveAsync(cacheKey);
        }

        #endregion
    }
}