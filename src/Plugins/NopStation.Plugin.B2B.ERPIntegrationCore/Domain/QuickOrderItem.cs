using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class QuickOrderItem : BaseEntity
{
    public string ProductSku { get; set; }

    public int Quantity { get; set; }

    public int QuickOrderTemplateId { get; set; }

    public string AttributesXml { get; set; }

    public virtual QuickOrderTemplate QuickOrderTemplate { get; set; }
}
