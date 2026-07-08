using System;
using Nop.Core;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents an invoice template
    /// </summary>
    public class InvoiceTemplate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template HTML
        /// </summary>
        public string TemplateHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default template
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the render mode identifier
        /// </summary>
        public int RenderModeId { get; set; }

        /// <summary>
        /// Gets or sets the render mode
        /// </summary>
        public RenderMode RenderMode
        {
            get => (RenderMode)RenderModeId;
            set => RenderModeId = (int)value;
        }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }
}
