using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers
{
    public class CommonHelperService : ICommonHelperService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CommonHelperService(ICustomerService customerService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<bool> HasB2BSalesRepRoleAsync()
        {
            var salesRepRoles = await _customerService.GetCustomerRolesAsync(await _workContext.GetCurrentCustomerAsync());
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