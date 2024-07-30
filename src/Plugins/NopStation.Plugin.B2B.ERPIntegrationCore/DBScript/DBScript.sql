/****** Card:  115-add-last-sync-date-time-for-all-erp-synced-data    Script Date: 01/04/2024 ******/

 ALTER TABLE [dbo].[Erp_ShipToAddress]
 ADD [LastShipToAddressSyncDate] DATETIME2 NULL;

 EXEC sp_rename 'Erp_Account.LastAccountRefresh', 'LastErpAccountSyncDate', 'COLUMN';

/****** Card:  114-ship-to-address-to-erp-account-branch    Script Date: 01/04/2024 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_ShiptoAddress_Erp_Account_Map](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpShiptoAddress_Id] [int] NULL,
	[ErpAccount_Id] [int] NULL,
 CONSTRAINT [PK_Erp_ShiptoAddress_Erp_Account_Map] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



insert into Erp_ShiptoAddress_Erp_Account_Map 
( ErpShiptoAddress_Id, ErpAccount_Id) 
select id, Erpaccount_id from Erp_ShipToAddress 


Alter table Erp_ShipToAddress
DROP COLUMN Erpaccount_id


/****** Card:  feature/116-Individual-logs-for-sync-and-activity-logs    Script Date: 08-Apr-24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Erp_Activity_Logs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ErpActivityLogTypeId] [int] NOT NULL,
	[EntityId] [int] NULL,
	[EntityName] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL,
	[IpAddress] [nvarchar](255) NOT NULL,
	[CustomerId] [int] NULL,
	[CreatedOnUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Erp_Activity_Logs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Inserting into [ActivityLogType] table

INSERT INTO [dbo].[ActivityLogType] ([SystemKeyword], [Name], [Enabled])
VALUES
('Erp_EditSettings', 'Edit setting(s)', 1),
('Erp_DeleteErpActivityLog', 'Delete an Erp Activity Log', 1),
('Erp_DeleteAllErpActivityLogs', 'Delete all Erp Activity Logs', 1),
('Erp_UpdateErpActivityLogsTypes', 'Update Erp Activity Log Types', 1),
('Erp_AddNewB2CCustomer', 'Add a new B2C Customer', 1),
('Erp_AddNewB2BCustomer', 'Add a new B2B Customer', 1),
('Erp_AddNewErpNopUser', 'Add a new Erp Nop User', 1),
('Erp_EditErpNopUser', 'Edit an Erp Nop User', 1),
('Erp_DeleteErpNopUser', 'Delete an Erp Nop User', 1),
('Erp_AddNewErpAccount', 'Add a new Erp Account', 1),
('Erp_EditErpAccount', 'Edit an Erp Account', 1),
('Erp_DeleteErpAccount', 'Delete an Erp Account', 1),
('Erp_ErpOrderPlacement', 'Place Order on ERP', 1),
('Erp_B2COrderPlacement', 'Place a B2C Order', 1),
('Erp_B2BOrderPlacement', 'Place a B2B Order', 1),
('Erp_B2BQuoteOrderPlacement', 'Place a B2B Quote Order', 1),
('Erp_AddNewErpShipToAddress', 'Add a new Erp Ship To Address', 1),
('Erp_EditErpShipToAddress', 'Edit an Erp Ship To Address', 1),
('Erp_DeleteErpShipToAddress', 'Delete an Erp Ship To Address', 1),
('Erp_AddNewErpGroupPriceCode', 'Add a new Erp Group Price Code', 1),
('Erp_EditErpGroupPriceCode', 'Edit an Erp Group Price Code', 1),
('Erp_DeleteErpGroupPriceCode', 'Delete an Erp Group Price Code', 1),
('Erp_CustomerImpersonationStart', 'Erp Customer Impersonation Start', 1),
('Erp_CustomerImpersonationEnd', 'Erp Customer Impersonation End', 1),
('Erp_AddNewErpNopUserAccountMap', 'Add a new Erp Nop User Account Map', 1),
('Erp_DeleteErpNopUserAccountMap', 'Delete an Erp Nop User Account Map', 1),
('Erp_ReprocessErpOrder', 'Reprocess an Erp Order', 1),
('Erp_AddNewSpecialPrice', 'Add a new Erp Special Price', 1),
('Erp_EditSpecialPrice', 'Edit an Erp Special Price', 1),
('Erp_DeleteSpecialPrice', 'Delete an Erp Special Price', 1),
('Erp_AddNewErpGroupPrice', 'Add a new Erp Group Price', 1),
('Erp_EditErpGroupPrice', 'Edit an Erp Group Price', 1),
('Erp_DeleteErpGroupPrice', 'Delete an Erp Group Price', 1),
('Erp_AddNewErpSalesOrg', 'Add a new Erp Sales Org', 1),
('Erp_EditErpSalesOrg', 'Edit an Erp Sales Org', 1),
('Erp_DeleteErpSalesOrg', 'Delete an Erp Sales Org', 1),
('Erp_AddNewErpSalesOrgWarehouse', 'Add a new Erp Sales Org Warehouse', 1),
('Erp_EditErpSalesOrgWarehouse', 'Edit an Erp Sales Org Warehouse', 1),
('Erp_DeleteErpSalesOrgWarehouse', 'Delete an Erp Sales Org Warehouse', 1),
('Erp_AddNewErpSalesRep', 'Add a new Erp Sales Representative', 1),
('Erp_EditErpSalesRep', 'Edit an Erp Sales Representative', 1),
('Erp_DeleteErpSalesRep', 'Delete an Erp Sales Representative', 1),
('Erp_ReOrder', 'ReOrder an Order', 1),
('Erp_ErpCustomerPublicStoreLogin', 'Erp Customer login into Public Store', 1),
('Erp_ErpCustomerPublicStoreLogOut', 'Erp Customer log out from public store', 1),
('Erp_QuoteToOrderConvert', 'Convert Quote to Order', 1),
('Erp_QuickOrderToOrderConvert', 'Convert Quick order to Order', 1),
('Erp_CreateQuickOrder', 'Create a Quick Order', 1),
('Erp_UpdateQuickOrder', 'Update a Quick Order', 1),
('Erp_DeleteQuickOrder', 'Delete a Quick Order', 1),
('Erp_InvoiceDownload', 'Download Invoice', 1),
('Erp_PODDownload', 'Download POD', 1),
('Erp_ErpNopUserAccountSwitch', 'Switch Erp Account for Erp Nop User', 1),
('Erp_AddNewErpAccountForErpSalesRepMap', 'Add new Erp Account for Erp Sales Rep', 1),
('Erp_DeleteErpAccountForErpSalesRepMap', 'Delete Erp Account for Erp Sales Rep', 1);


/****** Card:  bugfix/122-b2b-shipToAddress-error-resolve    Script Date: 15-Apr-24 ******/

  Alter Table [dbo].[Erp_Account]
  Alter Column [BillingSuburb] [nvarchar](max) NULL



/****** Card:  refactor/131-Erp-Data-Scheduler-modification    Script Date: 29-Apr-24 ******/

INSERT INTO [dbo].[ActivityLogType] ([SystemKeyword], [Name], [Enabled])
VALUES
('Erp_EditSyncTask', 'Edit sync task', 1);



/****** Card:  bugfix/132-ErpShipToAddress-issue-on-B2C-account-register    Script Date: 29-Apr-24 ******/
  
Alter Table [dbo].[Erp_ShipToAddress]
Alter Column [Suburb] [nvarchar](max) NULL