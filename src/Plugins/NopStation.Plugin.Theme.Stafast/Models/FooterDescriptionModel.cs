using Nop.Web.Models.Media;

namespace NopStation.Plugin.Theme.Stafast.Models
{
    public class FooterDescriptionModel
    {
        public FooterDescriptionModel()
        {
            Description1 = new FooterDescriptionBoxModel();
            Description2 = new FooterDescriptionBoxModel();
            Description3 = new FooterDescriptionBoxModel();
            Description4 = new FooterDescriptionBoxModel();
        }

        public FooterDescriptionBoxModel Description1 { get; set; }
        public FooterDescriptionBoxModel Description2 { get; set; }
        public FooterDescriptionBoxModel Description3 { get; set; }
        public FooterDescriptionBoxModel Description4 { get; set; }

        public class FooterDescriptionBoxModel
        {
            public FooterDescriptionBoxModel()
            {
                Picture = new PictureModel();
            }

            public bool Enabled { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public string Url { get; set; }
            public PictureModel Picture { get; set; }
        }
    }
}
