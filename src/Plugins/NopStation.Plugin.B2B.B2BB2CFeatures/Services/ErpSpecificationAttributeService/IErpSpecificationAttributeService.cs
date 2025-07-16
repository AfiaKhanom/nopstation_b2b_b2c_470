using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;

public interface IErpSpecificationAttributeService
{
    Task<IList<int>> GetSpecificationAttributeOptionIdsByNames(int specificationAttributeId, string preFilterFacets, int b2BAccountId);
    Task<IList<int>> GetSpecificationAttributeOptionIdsForExcludeByNames(int specificationAttributeId, string specialExcludeOption);

    Task<string> GetProductUOMByProductIdAndSpecificationAttributeId(int productId, int specificationAttributeId);
    Task<IList<int>> GetProductIdBySpecificationAttributeOptionNames(int specificationAttributeId, string specialExcludes, int b2BAccountId);
}