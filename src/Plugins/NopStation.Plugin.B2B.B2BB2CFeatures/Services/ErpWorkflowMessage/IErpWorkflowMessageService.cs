using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage
{
    public interface IErpWorkflowMessageService
    {
        Task<int> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress);
        Task<int> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
        Task<int> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
    }
}