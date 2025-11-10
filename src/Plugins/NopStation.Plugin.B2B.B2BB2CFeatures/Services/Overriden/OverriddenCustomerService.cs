using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public partial class OverriddenCustomerService : CustomerService
{
    #region Fields

    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpNopUserService _erpNopUserService;

    #endregion

    #region Ctor

    public OverriddenCustomerService(CustomerSettings customerSettings,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<CustomerRole> customerRoleRepository,
        IRepository<ForumPost> forumPostRepository,
        IRepository<ForumTopic> forumTopicRepository,
        IRepository<GenericAttribute> gaRepository,
        IRepository<NewsComment> newsCommentRepository,
        IRepository<Order> orderRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<PollVotingRecord> pollVotingRecordRepository,
        IRepository<ShoppingCartItem> shoppingCartRepository,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpNopUserService erpNopUserService) : base(customerSettings,
            eventPublisher,
            genericAttributeService,
            dataProvider,
            customerAddressRepository,
            blogCommentRepository,
            customerRepository,
            customerAddressMappingRepository,
            customerCustomerRoleMappingRepository,
            customerPasswordRepository,
            customerRoleRepository,
            forumPostRepository,
            forumTopicRepository,
            gaRepository,
            newsCommentRepository,
            orderRepository,
            productReviewRepository,
            productReviewHelpfulnessRepository,
            pollVotingRecordRepository,
            shoppingCartRepository,
            shortTermCacheManager,
            staticCacheManager,
            storeContext,
            shoppingCartSettings,
            taxSettings)
    {
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpNopUserService = erpNopUserService;
    }

    #endregion

    #region Methods

    #region Customer roles

    /// <summary>
    /// Gets list of customer roles
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="showHidden">A value indicating whether to load hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public override async Task<IList<CustomerRole>> GetCustomerRolesAsync(Customer customer, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var allRolesById = await GetAllCustomerRolesDictionaryAsync();

        var mappings = await _shortTermCacheManager.GetAsync(
            async () => await _customerCustomerRoleMappingRepository.GetAllAsync(query => query.Where(crm => crm.CustomerId == customer.Id)), NopCustomerServicesDefaults.CustomerRolesCacheKey, customer);

        var nopCustomerRoles = mappings
            .Select(mapping => allRolesById.TryGetValue(mapping.CustomerRoleId, out var role) ? role : null)
            .Where(cr => cr != null && (showHidden || cr.Active))
            .ToList();

        #region B2B

        var currentErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);

        if (currentErpUser == null)
            return nopCustomerRoles;

        var erpAccountNopUserMap = 
            await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(currentErpUser.ErpAccountId, currentErpUser.Id);

        var erpUserCustomerRoleIds = new List<int>();

        if (erpAccountNopUserMap != null && !string.IsNullOrWhiteSpace(erpAccountNopUserMap.CustomerRolesIds))
        {
            erpUserCustomerRoleIds = erpAccountNopUserMap.CustomerRolesIds
                .Split(',')
                .Select(y => int.TryParse(y, out var roleId) ? roleId : (int?)null)
                .Where(roleId => roleId.HasValue)
                .Select(roleId => roleId.Value)
                .ToList();

            var erpAccountNopUserMappedCustomerRoles =
                await _customerRoleRepository.GetAllAsync(query => query.Where(cr => erpUserCustomerRoleIds.Contains(cr.Id)));

            nopCustomerRoles.AddRange(erpAccountNopUserMappedCustomerRoles);
            nopCustomerRoles = nopCustomerRoles.DistinctBy(cr => cr.Id).ToList();
        }

        #endregion

        return nopCustomerRoles;
    }

    #endregion

    #endregion
}