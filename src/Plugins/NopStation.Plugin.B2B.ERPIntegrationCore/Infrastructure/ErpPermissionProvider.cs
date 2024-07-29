using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure
{
    public class ErpPermissionProvider : IPermissionProvider
    {
        #region Fields

        #region b2b

        public static readonly PermissionRecord DisplayB2BStock = new PermissionRecord { Name = "B2B. Display B2B Stock", SystemName = "DisplayB2BStock", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BPrices = new PermissionRecord { Name = "B2B. Display B2B Prices", SystemName = "DisplayB2BPrices", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BOrders = new PermissionRecord { Name = "B2B. Display B2B Orders", SystemName = "DisplayB2BOrders", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BQuotes = new PermissionRecord { Name = "B2B. Display B2B Quotes", SystemName = "DisplayB2BQuotes", Category = "B2B" };
        public static readonly PermissionRecord PlaceB2BOrder = new PermissionRecord { Name = "B2B. Place B2B Order", SystemName = "PlaceB2BOrder", Category = "B2B" };
        public static readonly PermissionRecord PlaceB2BQuote = new PermissionRecord { Name = "B2B. Place B2B Quote", SystemName = "PlaceB2BQuote", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BAccountCreditInfo = new PermissionRecord { Name = "B2B. Display B2B Account Credit Info", SystemName = "DisplayB2BAccountCreditInfo", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BAccountStatements = new PermissionRecord { Name = "B2B. Display B2B Account Statements", SystemName = "DisplayB2BAccountStatements", Category = "B2B" };
        public static readonly PermissionRecord DisplayB2BFinancialTransactions = new PermissionRecord { Name = "B2B. Display B2B Financial Transactions", SystemName = "DisplayB2BFinancialTransactions", Category = "B2B" };

        #endregion

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
                //PlaceB2COrder,
                //PlaceB2CQuote,
                //DisplayB2CStock,
                //DisplayB2CPrices,
                //DisplayB2COrders,
                //DisplayB2CQuotes,
                //DisplayB2CAccountCreditInfo,
                //DisplayB2CAccountStatements,
                //DisplayB2CFinancialTransactions
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
                        //PlaceB2COrder,
                        //PlaceB2CQuote,
                        //DisplayB2CStock,
                        //DisplayB2CPrices,
                        //DisplayB2COrders,
                        //DisplayB2CQuotes,
                        //DisplayB2CAccountCreditInfo,
                        //DisplayB2CAccountStatements,
                        //DisplayB2CFinancialTransactions,
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
}
