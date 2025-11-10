using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Web.Areas.Admin.Models.Customers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IOverridenCustomerModelFactory
{
    IAsyncEnumerable<CustomerModel> PrepareCustomerModelsAsync(IPagedList<Customer> customers);
}
