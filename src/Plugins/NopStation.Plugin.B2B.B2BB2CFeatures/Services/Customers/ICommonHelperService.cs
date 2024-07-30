using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers
{
    public interface ICommonHelperService
    {
        Task<bool> HasB2BSalesRepRoleAsync();
    }
}