using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountCustomerRegistrationPremisesService
    {
        Task InsertErpAccountCustomerRegistrationPremisesAsync(ErpAccountCustomerRegistrationPremises erpAccountCustomerRegistrationPremises);

        Task UpdateErpAccountCustomerRegistrationPremisesAsync(ErpAccountCustomerRegistrationPremises erpAccountCustomerRegistrationPremises);

        Task DeleteErpAccountCustomerRegistrationPremisesByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationPremises> GetErpAccountCustomerRegistrationPremisesByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationPremises> GetErpAccountCustomerRegistrationPremisesByIdWithActiveAsync(int id);

        Task<IPagedList<ErpAccountCustomerRegistrationPremises>> GetAllErpAccountCustomerRegistrationPremisesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string nameOfLandlord = null, string emailOfLandlord = null);

        Task<IList<ErpAccountCustomerRegistrationPremises>> GetErpAccountCustomerRegistrationPremisesByFormIdAsync(int formId);
    }
}

