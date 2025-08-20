using Nop.Core.Caching;

namespace NopStation.Plugin.Theme.Stafast.Infrastructure.Cache
{
    public class ModelCacheKey
    {
        public static CacheKey TopMenuModelKey => new CacheKey("Nopstation.Theme.Stafast.topmenu-{0}-{1}-{2}", TopMenuModelPattern);
        public static string TopMenuModelPattern => "Nopstation.Theme.Stafast.topmenu";

        public static CacheKey FooterDescriptionModelKey => new CacheKey("Nopstation.Theme.Stafast.footer.description-{0}-{1}-{2}", FooterDescriptionModelPattern);
        public static string FooterDescriptionModelPattern => "Nopstation.Theme.Stafast.footer.description";

        public static CacheKey SocialModelKey => new CacheKey("Nopstation.Theme.Stafast.social-{0}-{1}-{2}", SocialModelPattern);
        public static string SocialModelPattern => "Nopstation.Theme.Stafast.social";

    }
}
