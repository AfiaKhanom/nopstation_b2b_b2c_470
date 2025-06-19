# NopStation.Plugin.B2B.D365BCIntegration

## Overview

**NopStation.Plugin.B2B.D365BCIntegration** is a nopCommerce plugin designed to integrate nopCommerce with Microsoft Dynamics 365 Business Central (D365BC). It provides synchronization and management of accounts, products, orders, and stock between nopCommerce and D365BC, supporting B2B scenarios.

---

## Project Structure

```
NopStation.Plugin.B2B.D365BCIntegration/
│
├── D365BCIntegrationDefaults.cs
├── D365BCIntegrationPlugin.cs
├── D365BCIntegrationSettings.cs
├── logo.png
├── NopStation.Plugin.B2B.D365BCIntegration.csproj
├── plugin.json
├── readme.md
│
├── Areas/
│   └── Admin/
│       └── Controllers/
│           └── D365BCIntegrationController.cs
│
├── D365BCImplementation/
│   ├── D365BCIntegrationAccountService.cs
│   ├── D365BCIntegrationOrderService.cs
│   ├── D365BCIntegrationProductService.cs
│   ├── D365BCIntegrationStockService.cs
│   ├── ID365BCIntegrationAccountService.cs
│   ├── ID365BCIntegrationOrderService.cs
│   └── ...
│
├── Infrastructure/
│   └── NopStartup.cs
│
├── Localization/
├── Models/
├── Services/
│   ├── D365BCService.cs
│   ├── D365BCHttpClient.cs
│   ├── D365BCHttpService.cs
│   └── ErpNopMapperService.cs
└── ...
```

---

## Key Components

### 1. Plugin Entry Point

#### `D365BCIntegrationPlugin.cs`

- Inherits: `BasePlugin`, implements `IAdminMenuPlugin`, `IErpIntegrationPlugin`, `IMiscPlugin`, `INopStationPlugin`
- Responsibilities:
  - Registers plugin settings and localization resources on install.
  - Removes settings and resources on uninstall.
  - Provides configuration page URL.
  - Manages admin menu and plugin resources.

#### Key Methods:
- `InstallAsync()`: Registers settings and localization resources.
- `UninstallAsync()`: Cleans up settings and resources.
- `GetConfigurationPageUrl()`: Returns the admin configuration page URL.
- `InsertLocalStringResourcesAsync()`: Adds/upgrades locale resources for plugin fields.

---

### 2. Configuration & Settings

#### `D365BCIntegrationSettings.cs`

- Holds configuration such as API URLs, credentials, company name, environment, tenant ID, and sync limits.
- Used throughout the plugin for API calls and validation.

---

### 3. Dependency Injection & Startup

#### `Infrastructure/NopStartup.cs`

- Registers all plugin services with the DI container.
- Adds view location expander for plugin views.

#### Key Registrations:
- `ID365BCService` → `D365BCService`
- `ID365BCIntegrationAccountService` → `D365BCIntegrationAccountService`
- `ID365BCIntegrationProductService` → `D365BCIntegrationProductService`
- `ID365BCIntegrationStockService` → `D365BCIntegrationStockService`
- `ID365BCIntegrationOrderService` → `D365BCIntegrationOrderService`
- `IErpNopMapperService` → `ErpNopMapperService`
- `ID365BCAuthService` → `D365BCAuthService`
- `D365BCHttpClient` (as HTTP client)

---

### 4. Admin UI

#### `Areas/Admin/Controllers/D365BCIntegrationController.cs`

- Inherits: `NopStationAdminController`
- Responsibilities:
  - Renders and processes the configuration page.
  - Prepares customer lists for configuration.
  - Handles permissions and notifications.

#### Key Methods:
- `Configure()`: GET/POST actions for plugin configuration.
- `PrepareAvailableCustomersAsync()`: Loads available customers for selection.

---

### 5. Business Logic Services

#### Account, Product, Order, Stock Services

- Location: `D365BCImplementation/`
- Classes:
  - `D365BCIntegrationAccountService`
  - `D365BCIntegrationProductService`
  - `D365BCIntegrationOrderService`
  - `D365BCIntegrationStockService`

**Responsibilities:**
- Each service implements its respective interface (e.g., `ID365BCIntegrationAccountService`).
- Each service depends on:
  - `ID365BCService`: Core D365BC API logic.
  - `IErpLogsService`: Logging.
  - `D365BCIntegrationSettings`: Plugin configuration.
  - (Some) `IWorkContext`: nopCommerce context.

**Example:**  
`D365BCIntegrationOrderService`  
- Validates settings.
- Calls `ID365BCService` methods to fetch or create orders in D365BC.

---

### 6. Core D365BC API Logic

#### `Services/D365BCService.cs`

- Implements `ID365BCService`.
- Handles:
  - API URL construction (`PrepareODataUrl`)
  - Validation of settings (`IsValidD365BCIntegrationSettings`)
  - Mapping between D365BC and ERP models.
  - Calls to HTTP service/client for actual API communication.

#### `Services/D365BCHttpClient.cs` & `Services/D365BCHttpService.cs`

- Handle HTTP requests, authentication, and serialization for D365BC API.

---

### 7. Model Mapping

#### `Services/ErpNopMapperService.cs`

- Implements `IErpNopMapperService`.
- Maps between nopCommerce and ERP (D365BC) models for accounts, products, orders, etc.

---

## Localization

- Locale resources are managed in `InsertLocalStringResourcesAsync()` and removed in `UninstallAsync()` in `D365BCIntegrationPlugin`.
- Resources cover all configuration fields and admin UI strings.

---

## Extensibility

- All major services are registered via interfaces, supporting easy customization or replacement.
- Plugin settings are managed via nopCommerce's `ISettingService`.
- Admin UI is extensible via standard nopCommerce MVC patterns.

---

## How to Extend

1. **Add New ERP Entity Sync:**
   - Define interface and implementation in `D365BCImplementation/`.
   - Register in `Infrastructure/NopStartup.cs`.
   - Add admin UI as needed.

2. **Customize API Calls:**
   - Extend or override methods in `D365BCService`.
   - Update HTTP client/service as needed.

3. **Add/Update Localization:**
   - Add new keys in `InsertLocalStringResourcesAsync()`.

---

## Key Files Reference

- `D365BCIntegrationPlugin.cs`
- `D365BCIntegrationSettings.cs`
- `Infrastructure/NopStartup.cs`
- `Areas/Admin/Controllers/D365BCIntegrationController.cs`
- `D365BCImplementation/D365BCIntegrationAccountService.cs`
- `D365BCImplementation/D365BCIntegrationOrderService.cs`
- `D365BCImplementation/D365BCIntegrationProductService.cs`
- `D365BCImplementation/D365BCIntegrationStockService.cs`
- `Services/D365BCService.cs`
- `Services/D365BCHttpClient.cs`
- `Services/D365BCHttpService.cs`
- `Services/ErpNopMapperService.cs`

---

## Summary

This plugin is a robust, extensible integration layer between nopCommerce and Microsoft Dynamics 365 Business Central, following best practices for dependency injection, configuration, localization, and modular service design. For further details, refer to the code files listed above.
