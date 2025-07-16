using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;

public class ErpPermissionProvider : IPermissionProvider
{
    #region Fields

    public static readonly PermissionRecord DisplayB2BStock = new () { Name = "B2B. Display B2B Stock", SystemName = "DisplayB2BStock", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BPrices = new () { Name = "B2B. Display B2B Prices", SystemName = "DisplayB2BPrices", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BOrders = new () { Name = "B2B. Display B2B Orders", SystemName = "DisplayB2BOrders", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BQuotes = new () { Name = "B2B. Display B2B Quotes", SystemName = "DisplayB2BQuotes", Category = "B2B" };
    public static readonly PermissionRecord PlaceB2BOrder = new () { Name = "B2B. Place B2B Order", SystemName = "PlaceB2BOrder", Category = "B2B" };
    public static readonly PermissionRecord PlaceB2BQuote = new () { Name = "B2B. Place B2B Quote", SystemName = "PlaceB2BQuote", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BAccountCreditInfo = new () { Name = "B2B. Display B2B Account Credit Info", SystemName = "DisplayB2BAccountCreditInfo", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BAccountStatements = new () { Name = "B2B. Display B2B Account Statements", SystemName = "DisplayB2BAccountStatements", Category = "B2B" };
    public static readonly PermissionRecord DisplayB2BFinancialTransactions = new() { Name = "B2B. Display B2B Financial Transactions", SystemName = "DisplayB2BFinancialTransactions", Category = "B2B" };

    #endregion

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            PlaceB2BOrder,
            PlaceB2BQuote,
            DisplayB2BStock,
            DisplayB2BPrices,
            DisplayB2BOrders,
            DisplayB2BQuotes,
            DisplayB2BAccountCreditInfo,
            DisplayB2BAccountStatements,
            DisplayB2BFinancialTransactions
        };
    }

    public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new HashSet<(string, PermissionRecord[])>
        {
            (
                NopCustomerDefaults.AdministratorsRoleName,
                new[]
                {
                    PlaceB2BOrder,
                    PlaceB2BQuote,
                    DisplayB2BStock,
                    DisplayB2BPrices,
                    DisplayB2BOrders,
                    DisplayB2BQuotes,
                    DisplayB2BAccountCreditInfo,
                    DisplayB2BAccountStatements,
                    DisplayB2BFinancialTransactions
                }
            ),
            (
                ERPIntegrationCoreDefaults.B2BCustomerRole,
                new[]
                {
                    PlaceB2BOrder,
                    PlaceB2BQuote,
                    DisplayB2BStock,
                    DisplayB2BPrices,
                    DisplayB2BOrders,
                    DisplayB2BQuotes,
                    DisplayB2BAccountCreditInfo,
                    DisplayB2BFinancialTransactions,
                    StandardPermissionProvider.DisplayPrices,
                    StandardPermissionProvider.EnableWishlist,
                    StandardPermissionProvider.AccessClosedStore,
                    StandardPermissionProvider.EnableShoppingCart,
                    StandardPermissionProvider.PublicStoreAllowNavigation
                }
            ),
            (
                ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName,
                new[]
                {
                    PlaceB2BOrder,
                    DisplayB2BStock,
                    DisplayB2BPrices,
                    DisplayB2BOrders,
                    DisplayB2BQuotes,
                    StandardPermissionProvider.DisplayPrices,
                    StandardPermissionProvider.EnableWishlist,
                    StandardPermissionProvider.EnableShoppingCart,
                    StandardPermissionProvider.PublicStoreAllowNavigation
                }
            ),
            (
                ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName,
                new[]
                {
                    PlaceB2BQuote,
                    DisplayB2BStock,
                    DisplayB2BPrices,
                    DisplayB2BOrders,
                    DisplayB2BQuotes,
                    StandardPermissionProvider.DisplayPrices,
                    StandardPermissionProvider.EnableWishlist,
                    StandardPermissionProvider.EnableShoppingCart,
                    StandardPermissionProvider.PublicStoreAllowNavigation
                }
            ),
            (
                ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRoleSystemName,
                new[]
                {
                    DisplayB2BOrders,
                    DisplayB2BQuotes,
                    DisplayB2BAccountCreditInfo,
                    DisplayB2BAccountStatements,
                    DisplayB2BFinancialTransactions,
                    StandardPermissionProvider.PublicStoreAllowNavigation
                }
            ),
            (
                ERPIntegrationCoreDefaults.B2CCustomerRole,
                new[]
                {
                    StandardPermissionProvider.DisplayPrices,
                    StandardPermissionProvider.EnableWishlist,
                    StandardPermissionProvider.AccessClosedStore,
                    StandardPermissionProvider.EnableShoppingCart,
                    StandardPermissionProvider.PublicStoreAllowNavigation
                }
            ),
        };
    }
}
