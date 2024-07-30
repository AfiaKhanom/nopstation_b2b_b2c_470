using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers
{
    public class CommonHelperService : ICommonHelperService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;

        #endregion

        #region Ctor

        public CommonHelperService(ICustomerService customerService,
            IB2BB2CWorkContext b2BB2CWorkContext)
        {
            _customerService = customerService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
        }

        #endregion

        #region Methods

        public async Task<bool> HasB2BSalesRepRoleAsync()
        {
            var salesRepRoles = await _customerService.GetCustomerRolesAsync((await _b2BB2CWorkContext.GetCurrentERPCustomerAsync()).Customer);
            if (!salesRepRoles.Any())
            {
                return false;
            }
            var salesRepRole = salesRepRoles.Where(r => r.SystemName == ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName).FirstOrDefault();

            if (salesRepRole == null)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}