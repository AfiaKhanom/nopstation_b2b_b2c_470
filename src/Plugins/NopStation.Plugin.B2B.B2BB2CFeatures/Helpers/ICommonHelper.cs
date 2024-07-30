using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Helpers
{
    public interface ICommonHelper
    {
        Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null, string defaultItemValue = "0", bool selected = false);
        Task<List<SelectListItem>> PrepareDropdownDataFromEnumAsync<TEnum>(bool withSpecialDefaultItem = true, string defaultItemText = null, string defaultItemValue = "0", bool selected = false) where TEnum : Enum;
    }
}