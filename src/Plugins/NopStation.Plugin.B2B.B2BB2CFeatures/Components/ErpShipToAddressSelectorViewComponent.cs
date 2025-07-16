using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class ErpShipToAddressSelectorViewComponent : NopViewComponent
{
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IWorkContext _workContext;
    private readonly IAddressService _addressService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ICountryService _countryService;
    private readonly IErpAccountService _erpAccountService;

    public ErpShipToAddressSelectorViewComponent(ErpShipToAddressService erpShipToAddressService,
        IErpNopUserService erpNopUserService,
        IWorkContext workContext,
        IAddressService addressService,
        IStateProvinceService stateProvinceService,
        ICountryService countryService,
        IErpAccountService erpAccountService)
    {
        _erpShipToAddressService = erpShipToAddressService;
        _erpNopUserService = erpNopUserService;
        _workContext = workContext;
        _addressService = addressService;
        _stateProvinceService = stateProvinceService;
        _countryService = countryService;
        _erpAccountService = erpAccountService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RouteValueDictionary data)
    {
        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync())?.Id ?? 0);

        if (EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.User.Identity.IsAuthenticated && erpUser != null)
        {
            var erpShipToAddresses = await _erpShipToAddressService.GetAllErpShipToAddressesAsync(erpAccountId: erpUser.ErpAccountId, showHidden: false);
            var model = new ErpShipToAddressSelectorModel();

            foreach (var shipto in erpShipToAddresses)
            {
                var nopAddress = await _addressService.GetAddressByIdAsync(shipto.AddressId);
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress?.StateProvinceId ?? 0);
                var country = await _countryService.GetCountryByIdAsync(nopAddress?.CountryId ?? 0);

                model.AvailableShipToAddresses.Add(new ErpShipToAddressModel
                {
                    Id = shipto.Id,
                    HouseNumber = nopAddress?.Address1 ?? string.Empty,
                    Street = nopAddress?.Address2 ?? string.Empty,
                    Suburb = shipto.Suburb,
                    City = nopAddress?.City ?? string.Empty,
                    StateProvince = stateProvince?.Name ?? string.Empty,
                    StateProvinceCode = stateProvince?.Abbreviation ?? string.Empty,
                    PostalCode = nopAddress?.ZipPostalCode ?? string.Empty,
                    Country = country?.Name ?? string.Empty,
                    IsSelected = erpUser.ErpShipToAddressId == shipto.Id ? 1 : 0
                });

                if (erpUser.ErpShipToAddressId == shipto.Id)
                {
                    model.DefaultAddressChanged = 1;
                }
            }

            model.ErpNopUserId = erpUser.Id;
            model.SelectedErpShipToAddressId = erpUser.ErpShipToAddressId;
            model.AvailableShipToAddresses = model.AvailableShipToAddresses.OrderByDescending(x => x.IsSelected).ThenBy(x => x.CreatedOnUtc).ToList();
            model.NextDefaultErpShipToAddressId = model.AvailableShipToAddresses?.Count >= 2 ? model.AvailableShipToAddresses[1].Id : 0;

            var defaultShipToAddressErpAccountMapping = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpUser.ErpShipToAddressId);
            var erpAccountOfDefaultShipToAddress = await _erpAccountService.GetErpAccountByIdAsync(defaultShipToAddressErpAccountMapping?.ErpAccountId ?? 0);

            var nextDefaultShipToAddressErpAccountMapping = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(model.NextDefaultErpShipToAddressId);
            var erpAccountOfNextDefaultShipToAddress = await _erpAccountService.GetErpAccountByIdAsync(nextDefaultShipToAddressErpAccountMapping?.ErpAccountId ?? 0);

            model.IsSalesOrgDifferent = erpAccountOfDefaultShipToAddress?.ErpSalesOrgId != erpAccountOfNextDefaultShipToAddress?.ErpSalesOrgId;

            return View(model);
        }

        return Content(string.Empty);
    }
}
