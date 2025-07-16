using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;

public interface ICommonHelperService
{
    Task<bool> HasB2BSalesRepRoleAsync();
    Task ClearUnavailableShoppingCartAndWishlistItemsBeforeImpersonation(Customer customer);
}