using System.Collections.Generic;
using System.IO;
using System;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
public class OverriddenLocalizationService : LocalizationService
{
    public OverriddenLocalizationService(ILanguageService languageService, ILocalizedEntityService localizedEntityService, ILogger logger, IRepository<LocaleStringResource> lsrRepository, ISettingService settingService, IStaticCacheManager staticCacheManager, IWorkContext workContext, LocalizationSettings localizationSettings) : base(languageService, localizedEntityService, logger, lsrRepository, settingService, staticCacheManager, workContext, localizationSettings)
    {
    }
    public override async Task ImportResourcesFromXmlAsync(Language language, StreamReader xmlStreamReader, bool updateExistingResources = true)
    {
        ArgumentNullException.ThrowIfNull(language);

        if (xmlStreamReader.EndOfStream)
            return;

        var lsNamesList = new Dictionary<string, LocaleStringResource>();

        foreach (var localeStringResource in _lsrRepository.Table.Where(lsr => lsr.LanguageId == language.Id)
                     .OrderBy(lsr => lsr.Id))
            lsNamesList[localeStringResource.ResourceName.ToLowerInvariant()] = localeStringResource;

        var lrsToUpdateList = new List<LocaleStringResource>();
        var lrsToInsertList = new Dictionary<string, LocaleStringResource>();

        foreach (var (name, value) in LoadLocaleResourcesFromStream(xmlStreamReader, language.Name))
        {
            if (lsNamesList.TryGetValue(name, out var localString))
            {
                if (!updateExistingResources)
                    continue;

                var lsr = localString;
                lsr.ResourceValue = value;
                lrsToUpdateList.Add(lsr);
            }
            else
            {
                var lsr = new LocaleStringResource { LanguageId = language.Id, ResourceName = name, ResourceValue = value };
                lrsToInsertList[name] = lsr;
            }
        }
        lrsToUpdateList = lrsToUpdateList
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .ToList();
        await _lsrRepository.UpdateAsync(lrsToUpdateList, false);
        await _lsrRepository.InsertAsync(lrsToInsertList.Values.ToList(), false);

        //clear cache
        await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<LocaleStringResource>.Prefix);
    }

}
