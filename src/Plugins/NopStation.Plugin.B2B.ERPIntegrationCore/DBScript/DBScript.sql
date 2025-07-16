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


/****** Card:  clients/groceryexpress/bugfix/2804-bugs-on-nopstation-b2b-plugin-for-47    Script Date: 17-September-24 ******/

 ALTER TABLE [dbo].[Erp_Order_Item_Additional_Data]
 ADD [WareHouse] [nvarchar](255) NULL;


-- =============================================
-- SQL Script to Remove Underscores from Column Names
-- =============================================
-- This script identifies all columns in the current database that:
-- 1. Contain underscores in their names
-- 2. Belong to tables that have 'erp' in their name
-- Then renames them by removing the underscores.
-- For example: In table 'ErpCustomers', column 'User_Id' becomes 'UserId'
-- =============================================

-- Create a temporary table to store information about columns that need to be renamed
DECLARE @ColumnRename TABLE (
    SchemaName NVARCHAR(128),    -- Schema name (e.g., 'dbo')
    TableName NVARCHAR(128),     -- Table name
    ColumnName NVARCHAR(128),    -- Original column name with underscores
    NewColumnName NVARCHAR(128)  -- New column name without underscores
)

-- Create a table to store generated SQL commands
DECLARE @GeneratedSQL TABLE (
    ID INT IDENTITY(1,1),
    CommandType NVARCHAR(50),
    SQLCommand NVARCHAR(MAX)
)

-- Find all columns in the current database that contain underscores
-- AND belong to tables with 'erp' in their name (case-insensitive)
INSERT INTO @ColumnRename (SchemaName, TableName, ColumnName, NewColumnName)
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    REPLACE(c.name, '_', '') AS NewColumnName
FROM 
    sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    c.name LIKE '%[_]%'
    AND t.name LIKE '%erp%'  -- Filter for tables containing 'erp'

-- Declare variables needed for processing each column
DECLARE @SchemaName NVARCHAR(128)
DECLARE @TableName NVARCHAR(128)
DECLARE @ColumnName NVARCHAR(128)
DECLARE @NewColumnName NVARCHAR(128)
DECLARE @SQL NVARCHAR(MAX)
DECLARE @DataType NVARCHAR(MAX)
DECLARE @IsNullable BIT
DECLARE @MaxLength INT
DECLARE @Precision INT
DECLARE @Scale INT

-- Create a cursor to iterate through each column that needs to be renamed
DECLARE column_cursor CURSOR FOR
SELECT SchemaName, TableName, ColumnName, NewColumnName FROM @ColumnRename

-- Open the cursor and get the first row
OPEN column_cursor
FETCH NEXT FROM column_cursor INTO @SchemaName, @TableName, @ColumnName, @NewColumnName

-- Loop through each column that needs to be renamed
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Get the data type and properties of the current column
    SELECT 
        @DataType = tp.name,
        @IsNullable = c.is_nullable,
        @MaxLength = c.max_length,
        @Precision = c.precision,
        @Scale = c.scale
    FROM 
        sys.columns c
        INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id
    WHERE 
        c.object_id = OBJECT_ID(QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
        AND c.name = @ColumnName

    -- Build the complete data type definition string
    DECLARE @DataTypeDefinition NVARCHAR(MAX)
    SET @DataTypeDefinition = @DataType

    -- Add appropriate size/length specifications based on the data type
    IF @DataType IN ('varchar', 'nvarchar', 'char', 'nchar')
    BEGIN
        IF @MaxLength = -1
            SET @DataTypeDefinition = @DataTypeDefinition + '(MAX)'
        ELSE IF @DataType IN ('varchar', 'char')
            SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@MaxLength AS NVARCHAR) + ')'
        ELSE
            SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@MaxLength/2 AS NVARCHAR) + ')'
    END
    ELSE IF @DataType IN ('decimal', 'numeric')
    BEGIN
        SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@Precision AS NVARCHAR) + ',' + CAST(@Scale AS NVARCHAR) + ')'
    END

    -- Add NULL or NOT NULL constraint
    IF @IsNullable = 1
        SET @DataTypeDefinition = @DataTypeDefinition + ' NULL'
    ELSE
        SET @DataTypeDefinition = @DataTypeDefinition + ' NOT NULL'

    -- Step 1: ALTER COLUMN statement
    SET @SQL = 'ALTER TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + 
               ' ALTER COLUMN ' + QUOTENAME(@ColumnName) + ' ' + @DataTypeDefinition
    
    -- Store the ALTER command in our results table
    INSERT INTO @GeneratedSQL (CommandType, SQLCommand)
    VALUES ('ALTER', @SQL)
    
    -- Execute the ALTER command
    PRINT 'Executing: ' + @SQL
    EXEC sp_executesql @SQL

    -- Step 2: RENAME statement
    SET @SQL = 'EXEC sp_rename ''' + 
               QUOTENAME(@SchemaName) + '.' + 
               QUOTENAME(@TableName) + '.' + 
               QUOTENAME(@ColumnName) + ''', ''' + 
               @NewColumnName + ''', ''COLUMN'''
    
    -- Store the RENAME command in our results table
    INSERT INTO @GeneratedSQL (CommandType, SQLCommand)
    VALUES ('RENAME', @SQL)
    
    -- Execute the RENAME command
    PRINT 'Executing: ' + @SQL
    EXEC sp_executesql @SQL
    
    -- Get the next column to process
    FETCH NEXT FROM column_cursor INTO @SchemaName, @TableName, @ColumnName, @NewColumnName
END

-- Clean up the cursor
CLOSE column_cursor
DEALLOCATE column_cursor

-- Output a summary
DECLARE @RenamedCount INT
SELECT @RenamedCount = COUNT(*) FROM @ColumnRename
PRINT 'Successfully renamed ' + CAST(@RenamedCount AS NVARCHAR) + ' column(s) by removing underscores in tables containing ''erp''.'

-- Output all the generated SQL commands for review
SELECT ID,
    CommandType,
    SQLCommand
FROM 
    @GeneratedSQL
ORDER BY 
    ID

-- Output a summary table of all the renamed columns
SELECT 
    SchemaName,
    TableName,
    ColumnName AS OriginalColumnName,
    NewColumnName
FROM 
    @ColumnRename
ORDER BY 
    SchemaName,
    TableName,
    ColumnName


-- Make the ErpNopUserAccountMap CustomerRoleIds column nullable
ALTER TABLE [Erp_Nop_User_Account_Map]
ALTER COLUMN [CustomerRolesIds] NVARCHAR(MAX) NULL;


-- Rename the B2B related customer roles to the latest names and system names
Update[dbo].[CustomerRole] Set[Name] = 'B2B Customer', [SystemName] = 'B2BCustomer' Where[SystemName] = 'B2BUser';
Update[dbo].[CustomerRole] Set[Name] = 'B2C Customer', [SystemName] = 'B2CCustomer' Where[SystemName] = 'B2CUser';
Update[dbo].[CustomerRole] Set[Name] = 'B2B Sales Rep', [SystemName] = 'B2BSalesRep' Where[SystemName] = 'ERPSalesRep';
Update[dbo].[CustomerRole] Set[Name] = 'Quick Order User', [SystemName] = 'QuickOrderUser' Where[SystemName] = 'QuickOrderUser';
Update[dbo].[CustomerRole] Set[Name] = 'B2B-B2C Admin', [SystemName] = 'B2BB2CAdmin' Where[SystemName] = 'B2BB2CAdmin';
Update[dbo].[CustomerRole] Set[Name] = 'B2B Order Assistant', [SystemName] = 'B2BOrderAssistant' Where[SystemName] = 'B2BOrderAssistant';
Update[dbo].[CustomerRole] Set[Name] = 'B2B Quote Assistant', [SystemName] = 'B2BQuoteAssistant' Where[SystemName] = 'B2BQuoteAssistant';
Update[dbo].[CustomerRole] Set[Name] = 'B2B Customer Accounting Personnel', [SystemName] = 'B2BCustomerAccountingPersonnel' Where[SystemName] = 'B2BCustomerAccountingPersonnel';

-- Show the duplicate customer roles that needed to be deleted
WITH DuplicateCTEShow AS (
    SELECT Id, Name, SystemName,
           ROW_NUMBER() OVER (PARTITION BY [Name], [SystemName] ORDER BY Id ASC) as Records
    FROM [dbo].[CustomerRole]
)
SELECT * FROM DuplicateCTEShow 
WHERE Records > 1
ORDER BY Name, Id;

-- Delete the duplicate customer roles
WITH DuplicateCTEDelete AS (
    SELECT Id, Name, SystemName,
           ROW_NUMBER() OVER (PARTITION BY [Name], [SystemName] ORDER BY Id ASC) as Records
    FROM [dbo].[CustomerRole]
)
DELETE FROM DuplicateCTEDelete 
WHERE Records > 1