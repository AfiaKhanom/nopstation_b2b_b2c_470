using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpSalesRepService
    {
        Task InsertErpSalesRepAsync(ErpSalesRep erpSalesRep);

        Task UpdateErpSalesRepAsync(ErpSalesRep erpSalesRep);

        Task DeleteErpSalesRepByIdAsync(int id);

        Task<ErpSalesRep> GetErpSalesRepByIdAsync(int id);
        Task<IList<ErpSalesRep>> GetErpSalesRepByIdsAsync(int[] erpSalesRepIds);

        Task<ErpSalesRep> GetErpSalesRepByIdWithActiveAsync(int id);

        Task<IPagedList<ErpSalesRep>> GetAllErpSalesRepAsync(int nopCustomerId = 0, int salesRepTypeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false,
            int[] erpSalesOrgIds = null);

        Task<IList<ErpSalesRep>> GetErpSalesRepsByNopCustomerIdAsync(int nopCustomerId, bool showHidden = false);
        Task<IPagedList<ErpNopUser>> GetAllSalesRepUsersAsync(int salesRepId = 0, string erpAccontNo = null,
            string accountName = null, string email = null, string fullName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,
            bool getOnlyTotalCount = false);

        Task<IPagedList<ErpNopUser>> GetAllSalesRepUsersBySalesRepIdAsync(int salesRepId = 0, string erpAccontNo = null,
            string accountName = null, string email = null, string fullName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,
            bool getOnlyTotalCount = false);
        Task<IPagedList<ErpSalesOrg>> GetSalesRepOrgsPagedAsync(int salesRepId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false);
        Task<IList<ErpSalesOrg>> GetSalesRepOrgsAsync(int salesRepId, bool showHidden = false);
        Task DeleteErpSalesRepsAsync(IList<ErpSalesRep> erpSalesReps);
        Task<IPagedList<Customer>> GetAllSalesRepCustomersAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        Task<IPagedList<Customer>> GetAllCustomersNotYetSalesRepAsync(int includeSalesRepCustomerId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
    }
}

