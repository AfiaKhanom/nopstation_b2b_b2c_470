using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality
{
    public class ErpPriceSyncFunctionalityService : IErpPriceSyncFunctionalityService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
        private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ErpPriceSyncFunctionalityService(ICustomerService customerService,
            IErpAccountService erpAccountService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
            IErpGroupPriceCodeService erpGroupPriceCodeService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerService = customerService;
            _erpAccountService = erpAccountService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
            _erpGroupPriceCodeService = erpGroupPriceCodeService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        private async Task<bool> CheckPriceSyncRequired(Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var lastDateOfDisplayB2BPriceSyncInfo = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer,
                B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceSyncInfo, store.Id, defaultValue: null);

            // we will display price update notification only once in a day
            if (lastDateOfDisplayB2BPriceSyncInfo.HasValue && lastDateOfDisplayB2BPriceSyncInfo.Value.Date == DateTime.Now.Date)
                return false;

            var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
            if (b2BAccount == null)
                return false;

            //Checking for B2BPriceGroupProduct Pricing applicable for the B2BAccount
            if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
            {
                if (!b2BAccount.B2BPriceGroupCodeId.HasValue)
                    return false;

                var b2BPriceGroupCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(b2BAccount.B2BPriceGroupCodeId.Value);

                if (b2BPriceGroupCode == null)
                    return false;

                var lastDateOfDisplayB2BPriceGroupPriceSyncInfo = await _genericAttributeService.GetAttributeAsync<DateTime?>(b2BPriceGroupCode,
                B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo, store.Id, defaultValue: null);

                if (lastDateOfDisplayB2BPriceGroupPriceSyncInfo.HasValue && lastDateOfDisplayB2BPriceGroupPriceSyncInfo.Value.Date == DateTime.Now.Date)
                {
                    return false;
                }

                var b2BPriceGroupLastPriceRefreshDate = await _dateTimeHelper.ConvertToUserTimeAsync(b2BPriceGroupCode.LastUpdateTime, DateTimeKind.Utc);

                if (b2BPriceGroupLastPriceRefreshDate < DateTime.Now.Date)
                {
                    await _genericAttributeService.SaveAttributeAsync(b2BPriceGroupCode, B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo,
                        DateTime.Now.Date, store.Id);
                    return true;
                }

                return false;
            }
            else
            {
                if (!b2BAccount.LastPriceRefresh.HasValue)
                {
                    return true;
                }

                //convert dates to the user time
                var lastPriceRefreshDate = await _dateTimeHelper.ConvertToUserTimeAsync(b2BAccount.LastPriceRefresh.Value, DateTimeKind.Utc);
                if (lastPriceRefreshDate < DateTime.Now.Date)
                {
                    return true;
                }
            }

            return false;
        }

        public async void ExecuteAllProductsLivePriceSync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsB2BPriceSyncRequiredAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return false;

            var isRequired = await CheckPriceSyncRequired(customer);
            if (isRequired)
            {
                // we will display price update notification only once in a day
                await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceSyncInfo,
                        DateTime.Now.Date, store.Id);
            }

            return isRequired;
        }

        public async Task<bool> IsCartProductB2BPriceSyncRequiredAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}