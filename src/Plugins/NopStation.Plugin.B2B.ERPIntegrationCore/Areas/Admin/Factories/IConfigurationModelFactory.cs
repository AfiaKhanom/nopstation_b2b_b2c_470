using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Factories;

public interface IConfigurationModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();
}
