# Order PDF Designer Plugin for NopCommerce 4.70

## Overview

The Order PDF Designer plugin provides a fully-editable, single global PDF template for order invoices in NopCommerce. Administrators can design custom PDF layouts using rich-text HTML editors with token support, preview generated PDFs, and download them for orders.

## Features

### Core Functionality
- **Single Global Template**: One template for all orders, stored in NopCommerce settings
- **Modular HTML Sections**: Edit 8 separate sections independently:
  - Server Section Header
  - Header Details
  - Address Section (Billing & Shipping)
  - Product Section
  - Note Section
  - Order Summary
  - Footer
  - Footer Description

### Token Support
- **Order Tokens**: Order number, date, totals, payment method, shipping method
- **Address Tokens**: Billing and shipping address details
- **Store Tokens**: Store name, URL, email
- **Click-to-Insert**: Token helper buttons for easy insertion
- Compatible with NopCommerce message template tokens

### Preview & Generation
- **Live Preview**: AJAX-based HTML preview of rendered templates
- **PDF Generation**: Download PDFs for specific orders
- **Configurable Options**: Paper size (A4, Letter, Legal, etc.), margins (top, bottom, left, right)
- **Cache Busting**: Template version tracking for cache invalidation

### Admin UI
- **Rich-Text Editors**: TinyMCE integration for WYSIWYG editing
- **Token Helper**: Collapsible token panels for each section
- **Permission Protected**: ManageOrderPdfDesigner permission
- **Integrated Menu**: Appears in NopStation plugin menu

### Automatic Override of Default NopCommerce PDF Service
- **Seamless Integration**: Automatically replaces NopCommerce's default order PDF generation
- **System-Wide Override**: All order PDF generation throughout NopCommerce uses your custom templates
- **Fallback Protection**: If custom PDF generation fails, automatically falls back to default NopCommerce PDF
- **Compatible Areas**: Overrides PDF generation in:
  - Admin order details (Print Invoice)
  - Order management (Export to PDF)
  - Customer portal (Download Invoice)
  - Email attachments (if configured)
  - Bulk PDF exports

## Installation

1. Copy the plugin folder to `/Plugins/Nop.Plugin.NopStation.OrderPdfDesigner`
2. Build the solution or the plugin project
3. Navigate to Admin → Configuration → Local Plugins
4. Find "Order PDF Designer" and click Install
5. Configure permissions for admin users if needed

## Configuration

1. Go to **Admin → NopStation → Order PDF Designer → Configuration**
2. Edit each HTML section using the rich-text editor
3. Click the "Available Tokens" button to view and insert tokens
4. Configure PDF settings (paper size, margins)
5. Click **Save** to persist changes

## Testing & Preview

1. Enter an **Order ID** in the preview section
2. Click **Preview Order** to view rendered HTML in a modal
3. Click **Generate PDF** to download the PDF file

## How the Override Works

Once installed, the plugin automatically intercepts NopCommerce's default PDF service:

### Automatic System-Wide Override

The plugin uses the **Decorator Pattern** to wrap NopCommerce's `IPdfService`:

1. **During Plugin Startup**: The plugin registers `CustomPdfService` which decorates the default `PdfService`
2. **Service Resolution**: When any part of NopCommerce requests `IPdfService`, it gets our `CustomPdfService` instead
3. **PDF Generation**: All calls to `PrintOrderToPdfAsync()` are routed through our custom templates
4. **Fallback Safety**: If custom PDF generation fails, the call automatically falls back to NopCommerce's default implementation

### What Gets Overridden

✅ **Order PDFs** - Uses custom templates
- Admin → Order Details → Print Invoice
- Admin → Orders → Export to PDF
- Customer portal → Order History → Download Invoice
- Email attachments (when configured)

✅ **Bulk Order Exports** - Each order uses custom template
- Admin → Orders → Export selected orders

❌ **Packaging Slips** - Uses default NopCommerce implementation
❌ **Product Catalogs** - Uses default NopCommerce implementation

### Testing the Override

1. Install and configure the plugin with custom templates
2. Navigate to **Admin → Sales → Orders**
3. Select any order and click **Print Invoice** or **PDF Invoice**
4. The generated PDF will use your custom template
5. No additional configuration needed - it works automatically!

## PDF Rendering Integration

The plugin includes a placeholder `PdfRendererService` that returns HTML with a note about PDF library integration. For production use, integrate one of these HTML-to-PDF libraries:

### Recommended Options

1. **QuestPDF** (Recommended for .NET 8)
   - Pure .NET solution
   - Excellent WYSIWYG rendering
   - No external dependencies
   - NuGet: `QuestPDF`

2. **Puppeteer Sharp**
   - Uses Chromium for rendering
   - Excellent HTML/CSS support
   - Larger footprint
   - NuGet: `PuppeteerSharp`

3. **wkhtmltopdf**
   - Requires external binary
   - Good rendering quality
   - Wrapper: `DinkToPdf` or `Wkhtmltopdf.NetCore`

4. **IronPDF** (Commercial)
   - Commercial license required
   - Excellent features and support
   - NuGet: `IronPdf`

5. **SelectPdf** (Commercial)
   - Commercial license required
   - Good rendering quality
   - NuGet: `Select.HtmlToPdf`

### Integration Steps

1. Install your chosen PDF library via NuGet
2. Update `Services/PdfRendererService.cs`
3. Implement `ConvertHtmlToPdfAsync` method
4. Test with sample orders

## Security Considerations

### Admin Trust Model
- The plugin allows administrators (trusted users) to create HTML templates
- Admins can insert arbitrary HTML/CSS
- This is by design for template flexibility
- Only users with `ManageOrderPdfDesigner` permission can access

### Token Safety
- Tokens are processed server-side using NopCommerce's `IMessageTokenProvider`
- User-supplied data in tokens is handled by NopCommerce's token system
- No client-side token processing

### Permission Requirements
- Users must have `ManageOrderPdfDesigner` permission
- Default: Granted to Administrators role only

## Architecture

### Services
- **IOrderPdfDesignerService**: Main service interface
- **OrderPdfDesignerService**: Template rendering and token replacement
- **IPdfRendererService**: PDF generation abstraction
- **PdfRendererService**: PDF renderer implementation (placeholder)
- **CustomPdfService**: Decorator that overrides NopCommerce's default `IPdfService`
  - Intercepts all `PrintOrderToPdfAsync()` calls system-wide
  - Routes order PDF generation through custom templates
  - Falls back to default implementation on error
  - Preserves default behavior for packaging slips and product catalogs

### Controllers
- **OrderPdfDesignerAdminController**: Admin UI controller
  - `Configure()`: Display/save settings
  - `Preview(orderId)`: AJAX preview
  - `GeneratePdf(orderId)`: PDF download

### Models
- **OrderPdfDesignerSettings**: Template and configuration storage
- **ConfigurationModel**: Admin UI model
- **PreviewModel**: Preview request model

### Infrastructure
- **PluginNopStartup**: Service registration
- **RouteProvider**: Admin route configuration
- **OrderPdfDesignerPermissionProvider**: Permission definitions

## Token Reference

### Order Tokens
- `%Order.OrderNumber%` - Order number
- `%Order.OrderId%` - Order ID
- `%Order.CustomerFullName%` - Customer name
- `%Order.CustomerEmail%` - Customer email
- `%Order.OrderDate%` - Order date
- `%Order.OrderTotal%` - Total amount
- `%Order.OrderSubTotal%` - Subtotal
- `%Order.OrderShipping%` - Shipping cost
- `%Order.OrderTax%` - Tax amount
- `%Order.OrderDiscount%` - Discount amount
- `%Order.PaymentMethod%` - Payment method name
- `%Order.ShippingMethod%` - Shipping method name

### Billing Address Tokens
- `%Order.BillingFirstName%` - First name
- `%Order.BillingLastName%` - Last name
- `%Order.BillingAddress1%` - Address line 1
- `%Order.BillingAddress2%` - Address line 2
- `%Order.BillingCity%` - City
- `%Order.BillingStateProvince%` - State/Province
- `%Order.BillingZipPostalCode%` - Zip/Postal code
- `%Order.BillingCountry%` - Country

### Shipping Address Tokens
- `%Order.ShippingFirstName%` - First name
- `%Order.ShippingLastName%` - Last name
- `%Order.ShippingAddress1%` - Address line 1
- `%Order.ShippingAddress2%` - Address line 2
- `%Order.ShippingCity%` - City
- `%Order.ShippingStateProvince%` - State/Province
- `%Order.ShippingZipPostalCode%` - Zip/Postal code
- `%Order.ShippingCountry%` - Country

### Store Tokens
- `%Store.Name%` - Store name
- `%Store.URL%` - Store URL
- `%Store.Email%` - Store email

## Troubleshooting

### Plugin Not Appearing in Menu
- Verify installation completed successfully
- Check that user has `ManageOrderPdfDesigner` permission
- Clear NopCommerce cache

### Preview Not Loading
- Check browser console for JavaScript errors
- Verify order ID exists
- Check application logs for server errors

### PDF Generation Returns HTML
- This is expected behavior with the default implementation
- Integrate a PDF library as documented above

### Tokens Not Resolving
- Verify token syntax matches exactly (case-sensitive)
- Check that order has the required data
- Review application logs for token provider errors

## Version History

### Version 4.70.1.0
- Initial release
- Support for NopCommerce 4.70
- Modular template sections
- Token support
- Preview functionality
- Placeholder PDF renderer

## Support

For issues, questions, or feature requests, please contact Nop-Station support.

## License

Copyright © Nop-Station Team. All rights reserved.
