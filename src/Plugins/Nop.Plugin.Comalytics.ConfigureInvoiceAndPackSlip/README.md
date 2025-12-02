# Configure Invoice and Pack Slip Plugin for nopCommerce 4.70

## Overview

This plugin provides comprehensive invoice and packing slip PDF generation capabilities for nopCommerce 4.70. It supports multiple PDF rendering engines (QuestPDF and Chromium), custom templates, token-based content generation, multi-language support, barcode generation, and batch processing.

## Features

- **Dual PDF Rendering Engines**:
  - **QuestPDF**: Native C# PDF generation with excellent performance
  - **Chromium**: HTML-to-PDF rendering for pixel-perfect HTML template fidelity (requires additional setup)

- **Custom Templates**:
  - HTML-based templates with token replacement
  - Per-store and per-language template support
  - Template editor with syntax highlighting
  - Default templates included

- **Token System**:
  - Comprehensive token support for orders, customers, products, shipments
  - Safe HTML escaping by default
  - Barcode generation token support
  - Custom attribute tokens

- **Advanced Features**:
  - RTL (Right-to-Left) language support
  - Custom fonts and font embedding
  - Configurable page sizes and margins
  - Product image embedding
  - Batch PDF generation with job tracking
  - Background processing support

## Installation

1. Copy the plugin folder to `/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/`

2. Build the solution or the plugin project:
   ```bash
   dotnet build
   ```

3. Restart the application

4. Navigate to **Admin > Configuration > Local plugins**

5. Find "Configure Invoice and Pack Slip" and click **Install**

6. Click **Configure** to set up the plugin settings

## Configuration

### PDF Renderer Selection

- **QuestPDF** (Default): Works out of the box, no additional setup required
- **Chromium**: Requires Playwright installation (see below)

### Chromium Setup (Optional)

To use the Chromium renderer for HTML-to-PDF conversion:

1. Add the Playwright package to the project:
   ```xml
   <PackageReference Include="Microsoft.Playwright" Version="1.40.0" />
   ```

2. Install Chromium browser:
   ```bash
   pwsh bin/Debug/net8.0/playwright.ps1 install chromium
   ```

3. Update the ChromiumAdapter implementation with actual Playwright rendering logic

4. Select "Chromium" as the PDF renderer in plugin configuration

## Usage

### Creating Templates

1. Navigate to **Plugins > Invoice & Packing Slips > Invoice Templates**

2. Click **Add New Template**

3. Configure:
   - **Name**: Template identifier
   - **Store**: Select store or 0 for all stores
   - **Language**: Select language or 0 for all languages
   - **Render Mode**: QuestPDF or Chromium
   - **Template HTML**: Enter HTML with tokens
   - **Is Default**: Set as default template for the store/language combination

4. Save the template

### Available Tokens

#### Order Tokens
- `{Order.Number}` - Order number
- `{Order.Date}` - Order date
- `{Order.SubTotal}` - Order subtotal
- `{Order.ShippingTotal}` - Shipping cost
- `{Order.Tax}` - Tax amount
- `{Order.Total}` - Total amount
- `{Order.Items}` - Order items (auto-generated table)

#### Customer Tokens
- `{Customer.Name}` - Customer full name
- `{Customer.Email}` - Customer email
- `{Customer.Phone}` - Customer phone number

#### Address Tokens
- `{Billing.Address}` - Formatted billing address
- `{Shipping.Address}` - Formatted shipping address

#### Payment & Shipping Tokens
- `{Payment.Method}` - Payment method name
- `{Shipping.Method}` - Shipping method name
- `{Shipment.TrackingNumber}` - Tracking number

#### Store Tokens
- `{Store.Name}` - Store name
- `{Store.Url}` - Store URL
- `{Store.Logo}` - Store logo URL

#### Special Tokens
- `{BarCode}` - Generated barcode image (Base64)
- `{Currency}` - Currency code

### Generating PDFs

#### Single Invoice/Packing Slip
1. Navigate to an order in the admin panel
2. Click **Generate Invoice** or **Generate Packing Slip**
3. PDF will be generated and downloaded

#### Batch Generation
1. Navigate to **Plugins > Invoice & Packing Slips > PDF Jobs**
2. Click **Create New Job**
3. Select orders and job type
4. Submit the job
5. Monitor progress in the job list
6. Download completed PDFs

## Database Schema

The plugin creates the following tables:

- `Coma_InvoiceTemplate` - Invoice template definitions
- `Coma_PackingSlipTemplate` - Packing slip template definitions
- `Coma_PdfJob` - PDF generation job records
- `Coma_GeneratedPdf` - Generated PDF file records

## Architecture

### Services

- **ITemplateService**: Template CRUD operations using LINQ2DB
- **IPdfRenderer**: PDF rendering interface with QuestPDF and Chromium implementations
- **ITokenResolverService**: Token resolution from order/shipment data
- **IBarcodeService**: Barcode generation using ZXing.Net

### Migrations

The plugin uses FluentMigrator for database schema management:
- `SchemaMigration`: Initial schema creation
- Entity builders for LINQ2DB mappings

### Admin UI

All admin views follow nopCommerce conventions:
- Located under `Areas/Admin/`
- Controllers suffixed with `AdminController`
- DataTables for list views
- `_CreateOrUpdate` partials for edit forms

## Security Considerations

- Template editing restricted to admin roles
- Token output sanitized by default (HTML escaped)
- External resource fetching disabled in templates
- File storage in configured plugin directory
- Permission-based access control

## Troubleshooting

### QuestPDF License Error
The plugin uses the Community license for QuestPDF. For commercial use, obtain a commercial license from QuestPDF.

### Chromium Not Working
Ensure Playwright and Chromium are installed correctly:
```bash
dotnet add package Microsoft.Playwright
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

### Templates Not Rendering
- Check token syntax is correct
- Verify template is set as default or correctly associated with store/language
- Check error logs in **System > Log**

### Images Not Displaying
- Ensure product images exist and are accessible
- Check MaxPictureWidth and MaxPictureHeight settings
- Verify image URLs are accessible from the server

## Upgrade Notes from 4.20

Key differences from nopCommerce 4.20:

1. **LINQ2DB**: Replaced Entity Framework Core with LINQ2DB
2. **Async Operations**: All service methods are now async
3. **FluentMigrator**: Database migrations use FluentMigrator
4. **Updated Dependencies**: QuestPDF 2024.x, .NET 8.0

Migration steps:
1. Export existing templates and settings
2. Uninstall old plugin
3. Install new plugin
4. Import templates
5. Reconfigure settings

## Support

For issues, questions, or feature requests:
- GitHub Issues: [Repository URL]
- Documentation: [Documentation URL]
- Email: support@comalytics.com

## License

Copyright © 2024 Comalytics Team. All rights reserved.

## Version History

### 4.70.0.1 (2024-12-02)
- Initial release for nopCommerce 4.70
- QuestPDF rendering engine
- Chromium adapter (stub implementation)
- Template management
- Token system
- Barcode generation
- Multi-language support
- Batch processing foundation
