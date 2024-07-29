using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpProductService : IErpProductService
    {
        #region Fields

        private readonly IRepository<Product> _erpProductRepository;

        #endregion

        #region ctor

        public ErpProductService(IRepository<Product> erpProductRepository)
        {
            _erpProductRepository = erpProductRepository;
        }

        #endregion

        #region Methods

        public async Task UnpublishAllOldProduct(DateTime syncStartTime)
        {
            if (syncStartTime == DateTime.MinValue)
                return;
            var products = await _erpProductRepository.GetAllAsync(query =>
            {
                query = query.Where(product => product.UpdatedOnUtc < syncStartTime);
                return query;
            });
            foreach (var product in products)
            {
                product.Published = false;
            }
            await _erpProductRepository.UpdateAsync(products);
        }

        #endregion
    }
}