using Nop.Web.Framework.Models;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Models
{
    /// <summary>
    /// Represents an invoice template list model
    /// </summary>
    public record InvoiceTemplateListModel : BasePagedListModel<InvoiceTemplateModel>
    {
    }
}
