using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;

public record ErpSalesRepModel : ErpBaseEntityModel
{
    public ErpSalesRepModel()
    {
        SalesOrgIds = new List<int>();
        AvailableCustomers = new List<SelectListItem>();
        AvailableSalesRepType = new List<SelectListItem>();
        AvailableSalesOrgs = new List<SelectListItem>();
        ErpAccountSearchModel = new ErpAccountSearchModel();
    }
    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.NopCustomerId")]
    public int NopCustomerId { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.NopCustomerId")]
    public string NopCustomer { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesRepTypeId")]
    public int SalesRepTypeId { get; set; }

    public int SalesRepId {  get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesRepTypeId")]
    public string SalesRepType { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesOrgIds")]
    public IList<int> SalesOrgIds { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesOrgIds")]
    public string CommaSeparatedOrgNames { get; set; } 
    
    //customer roles
    [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.CustomerRoles")]
    public string CustomerRoleNames { get; set; }

    public ErpAccountSearchModel ErpAccountSearchModel { get; set; }

    public IList<SelectListItem> AvailableCustomers { get; set; }
    public IList<SelectListItem> AvailableSalesRepType { get; set; }
    public IList<SelectListItem> AvailableSalesOrgs { get; set; }
}
