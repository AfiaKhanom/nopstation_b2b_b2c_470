using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;

public interface IErpWorkflowMessageService
{
    Task<IList<int>> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress);
    Task<IList<int>> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
    Task<IList<int>> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
    Task<IList<int>> SendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId);
    Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId);
    Task<IList<int>> SendOrderPlacedAffiliateNotificationAsync(Order order, int languageId);
    Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId, string attachmentFilePath = null, string attachmentFileName = null);
}