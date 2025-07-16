using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;

public class ERPCustomer
{
    public Customer Customer { get; set; }
    public Customer OriginalCustomer { get; set; }
    public ErpAccount ErpAccount { get; set; }
    public ErpNopUser ErpNopUser { get; set; }
    public string CustomerRolesIds { get; set; }
}
