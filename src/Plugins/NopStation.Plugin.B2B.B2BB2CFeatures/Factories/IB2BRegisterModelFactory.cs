using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public interface IB2BRegisterModelFactory
    {
        Task<IList<CustomerAttributeModel>> PrepareCustomCustomerAttributesAsync(Customer customer, string overrideAttributesXml = "");
        Task<B2BRegisterModel> PrepareB2BRegisterModelAsync(B2BRegisterModel model, bool excludeProperties, string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false);
        Task<ErpAccountCustomerRegistrationFormModel> PrepareErpAccountCustomerRegistrationFormModelAsync(ErpAccountCustomerRegistrationFormModel model, bool setDefaultValues = false);
    }
}