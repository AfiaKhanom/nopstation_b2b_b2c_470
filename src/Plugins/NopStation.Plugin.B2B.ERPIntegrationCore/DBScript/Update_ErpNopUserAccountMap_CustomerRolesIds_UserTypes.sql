/****** Card:  clients/groceryexpress/bugfix/2963-2804-bugs-on-nopstation-b2b-plugin-for-47    Script Date: 05-Nov-2025 ******/


-- Update ErpUserTypeId with ErpNopUserType ids
UPDATE Erp_Nop_User_Account_Map 
SET [ErpUserTypeId] = ( 
    SELECT [ErpUserTypeId]  
    FROM [dbo].[Erp_Nop_User] ENU 
    WHERE ENU.[Id] = Erp_Nop_User_Account_Map.[ErpUserId]
)
WHERE EXISTS (
    SELECT 1 
    FROM [dbo].[Erp_Nop_User] ENU 
    WHERE ENU.[Id] = Erp_Nop_User_Account_Map.[ErpUserId] 
)
GO


-- Update CustomerRolesIds with actual customer role IDs
UPDATE Erp_Nop_User_Account_Map 
SET [CustomerRolesIds] = (
    SELECT STRING_AGG(CAST(CCM.[CustomerRole_Id] AS VARCHAR), ',')
    FROM [dbo].[Customer_CustomerRole_Mapping] CCM 
    INNER JOIN [dbo].[Erp_Nop_User] ENU ON CCM.[Customer_Id] = ENU.[NopCustomerId]
    WHERE ENU.[Id] = Erp_Nop_User_Account_Map.[ErpUserId] AND CCM.[CustomerRole_Id] != 3
)
WHERE EXISTS (
    SELECT 1 
    FROM [dbo].[Erp_Nop_User] ENU 
    WHERE ENU.[Id] = Erp_Nop_User_Account_Map.[ErpUserId] 
    AND ENU.[NopCustomerId] IS NOT NULL
)
GO