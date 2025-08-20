using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Theme.Stafast.Models
{
    public partial record StafastSocialModel : BaseNopModel
    {
        public string LinkedInLink { get; set; }
        public string PinterestLink { get; set; }
    }
}