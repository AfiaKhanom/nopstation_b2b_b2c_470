# Technical Specification: Comalytics Invoice and Pack Slip Plugin

## Executive Summary

This document provides a comprehensive technical specification for the Comalytics Invoice and Pack Slip Plugin for nopCommerce 4.70. The plugin enables custom PDF generation for invoices and packing slips with support for multiple rendering engines, templates, tokens, and batch processing.

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Admin UI Layer                           │
│  (Controllers, Views, Models - Areas/Admin)                  │
├─────────────────────────────────────────────────────────────┤
│                    Service Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Template   │  │    Token     │  │     PDF      │      │
│  │   Service    │  │   Resolver   │  │   Renderer   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │   Barcode    │  │   PDF Job    │                        │
│  │   Service    │  │   Service    │                        │
│  └──────────────┘  └──────────────┘                        │
├─────────────────────────────────────────────────────────────┤
│                    Data Layer                                │
│  (LINQ2DB Repositories, Entity Builders, Migrations)        │
├─────────────────────────────────────────────────────────────┤
│                    Domain Layer                              │
│  (Entities, Enums, Value Objects)                          │
└─────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

#### 1. Domain Layer
- **Entities**: InvoiceTemplate, PackingSlipTemplate, PdfJob, GeneratedPdf
- **Enums**: RenderMode, JobType, JobStatus
- **Responsibility**: Define data structures and business rules

#### 2. Data Layer
- **Entity Builders**: Map entities to database schema using FluentMigrator
- **Migrations**: Schema versioning and updates
- **Responsibility**: Database access and persistence

#### 3. Service Layer
- **TemplateService**: CRUD operations for templates
- **TokenResolverService**: Convert order/shipment data to render models
- **PdfRenderer**: Generate PDFs from render models
- **BarcodeService**: Generate barcode images
- **PdfJobService**: Manage background PDF jobs (to be implemented)
- **Responsibility**: Business logic and orchestration

#### 4. Admin UI Layer
- **Controllers**: Handle HTTP requests, validate input, return views
- **Models**: View models for data binding and validation
- **Views**: Razor views for user interface
- **Responsibility**: User interaction and data presentation

## Database Schema Design

### Entity Relationship Diagram

```
┌──────────────────────┐
│  InvoiceTemplate     │
├──────────────────────┤
│ Id (PK)              │
│ StoreId              │
│ LanguageId           │
│ Name                 │
│ TemplateHtml         │
│ IsDefault            │
│ RenderModeId         │
│ CreatedOnUtc         │
│ UpdatedOnUtc         │
└──────────────────────┘

┌──────────────────────┐
│ PackingSlipTemplate  │
├──────────────────────┤
│ Id (PK)              │
│ StoreId              │
│ LanguageId           │
│ Name                 │
│ TemplateHtml         │
│ IsDefault            │
│ RenderModeId         │
│ CreatedOnUtc         │
│ UpdatedOnUtc         │
└──────────────────────┘

┌──────────────────────┐       ┌──────────────────────┐
│      PdfJob          │       │   GeneratedPdf       │
├──────────────────────┤       ├──────────────────────┤
│ Id (PK)              │◄──────┤ JobId (FK)           │
│ JobTypeId            │       │ OrderId              │
│ StatusId             │       │ FileName             │
│ RequestedByCustomerId│       │ FilePath             │
│ ParametersJson       │       │ Size                 │
│ CreatedOnUtc         │       │ CreatedOnUtc         │
│ CompletedOnUtc       │       └──────────────────────┘
│ ErrorMessage         │
└──────────────────────┘
```

### Indexing Strategy

Recommended indexes (to be added in future migration):

```sql
-- Invoice Template lookups
CREATE INDEX IX_InvoiceTemplate_StoreLanguage 
ON Coma_InvoiceTemplate(StoreId, LanguageId, IsDefault);

-- Packing Slip Template lookups
CREATE INDEX IX_PackingSlipTemplate_StoreLanguage 
ON Coma_PackingSlipTemplate(StoreId, LanguageId, IsDefault);

-- Job status queries
CREATE INDEX IX_PdfJob_Status 
ON Coma_PdfJob(StatusId, CreatedOnUtc DESC);

-- Generated PDF lookups
CREATE INDEX IX_GeneratedPdf_Order 
ON Coma_GeneratedPdf(OrderId, CreatedOnUtc DESC);

CREATE INDEX IX_GeneratedPdf_Job 
ON Coma_GeneratedPdf(JobId);
```

## Service Layer Details

### 1. ITemplateService

**Purpose**: Manage invoice and packing slip templates

**Methods**:
- `GetInvoiceTemplateByIdAsync(int id)`: Retrieve specific template
- `GetAllInvoiceTemplatesAsync(int storeId, int languageId)`: List templates with filtering
- `InsertInvoiceTemplateAsync(InvoiceTemplate)`: Create new template
- `UpdateInvoiceTemplateAsync(InvoiceTemplate)`: Update existing template
- `DeleteInvoiceTemplateAsync(InvoiceTemplate)`: Remove template
- Similar methods for PackingSlipTemplate

**Implementation Notes**:
- Uses LINQ2DB for all operations
- Auto-sets timestamps (CreatedOnUtc, UpdatedOnUtc)
- Supports store and language filtering (0 = all)
- Results ordered by StoreId, LanguageId, Name

### 2. ITokenResolverService

**Purpose**: Convert order/shipment data into render models

**Methods**:
- `ResolveInvoiceTokensAsync(Order order)`: Create InvoiceRenderModel from order
- `ResolvePackingSlipTokensAsync(Shipment shipment)`: Create PackingSlipRenderModel from shipment

**Token Resolution Process**:
```
Order/Shipment Data
    ↓
Load Related Entities
(Customer, Addresses, Items, Products, Pictures)
    ↓
Format Values
(Currency, Dates, Addresses)
    ↓
Generate Barcodes
(Order number as CODE_128)
    ↓
HTML Escape
(Product names, addresses)
    ↓
RenderModel
```

**Supported Tokens**:
- Order: number, date, subtotal, shipping, tax, total, items
- Customer: name, email, full name
- Address: formatted multi-line address with state/country lookup
- Payment: method name
- Shipping: method name, tracking number
- Store: name, URL
- Barcode: Base64 PNG image
- Currency: code
- Products: SKU, name, quantity, prices, pictures

### 3. IPdfRenderer

**Purpose**: Generate PDF bytes from render models

**Interface**:
```csharp
public interface IPdfRenderer
{
    Task<byte[]> RenderInvoiceToPdfAsync(InvoiceRenderModel model, CancellationToken cancellationToken = default);
    Task<byte[]> RenderPackingSlipToPdfAsync(PackingSlipRenderModel model, CancellationToken cancellationToken = default);
}
```

**Implementations**:

#### QuestPdfAdapter
- **Library**: QuestPDF 2024.7.3
- **License**: Community (set in constructor)
- **Features**:
  - Native C# PDF generation
  - Fluent API for document composition
  - Support for tables, images, headers, footers
  - Pagination with page numbers
  - RTL text support
  - Font embedding
  - Fast performance

**Invoice Layout**:
```
┌─────────────────────────────────────────┐
│ Header                                  │
│  Store Name              INVOICE        │
│  Store URL               #Order-123     │
│                          2024-12-02     │
├─────────────────────────────────────────┤
│ Billing & Shipping Addresses            │
├─────────────────────────────────────────┤
│ Items Table                             │
│ SKU | Product | Qty | Price | Total     │
│ ... | ....... | ... | ..... | .....     │
├─────────────────────────────────────────┤
│ Totals (aligned right)                  │
│                      Subtotal: $100.00  │
│                      Shipping: $10.00   │
│                           Tax: $9.00    │
│                         Total: $119.00  │
├─────────────────────────────────────────┤
│ Payment Method: Credit Card             │
├─────────────────────────────────────────┤
│ Footer                   Page 1 of 1    │
└─────────────────────────────────────────┘
```

#### ChromiumAdapter (Stub)
- **Library**: Microsoft.Playwright (to be added)
- **Purpose**: HTML to PDF with pixel-perfect rendering
- **Status**: Stub implementation with TODO comments
- **Use Case**: When exact HTML template fidelity is required

**Implementation Guide**:
```csharp
// 1. Install Playwright
// dotnet add package Microsoft.Playwright

// 2. Install Chromium
// pwsh bin/Debug/net8.0/playwright.ps1 install chromium

// 3. Implement rendering
public async Task<byte[]> RenderInvoiceToPdfAsync(InvoiceRenderModel model, ...)
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();
    
    // Generate HTML from template + tokens
    var html = GenerateHtml(model);
    await page.SetContentAsync(html);
    
    // Generate PDF
    var pdfBytes = await page.PdfAsync(new()
    {
        Format = "A4",
        PrintBackground = true,
        Margin = new() { Top = "20mm", Bottom = "20mm", Left = "20mm", Right = "20mm" }
    });
    
    return pdfBytes;
}
```

### 4. IBarcodeService

**Purpose**: Generate barcode images

**Implementation**: Uses ZXing.Net
- **Format**: CODE_128
- **Output**: PNG image as Base64 or bytes
- **Configurable**: Width, height, margin
- **Default**: 250x100 with 10px margin

**Usage**:
```csharp
var base64 = await _barcodeService.GenerateBarcodeBase64Async("ORDER-12345");
var bytes = await _barcodeService.GenerateBarcodeBytesAsync("ORDER-12345", 300, 120);
```

## Configuration System

### Settings Class

**ConfigureInvoiceAndPackSlipSettings** implements `ISettings`:

```csharp
public class ConfigureInvoiceAndPackSlipSettings : ISettings
{
    // Renderer
    public string PdfRendererProvider { get; set; } = "QuestPdf";
    
    // Page Layout
    public string PageOrientation { get; set; } = "Portrait";
    public string PageSize { get; set; } = "A4";
    public int TopMargin { get; set; } = 20;
    public int BottomMargin { get; set; } = 20;
    public int LeftMargin { get; set; } = 20;
    public int RightMargin { get; set; } = 20;
    
    // Typography
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 10;
    public bool EnableRtl { get; set; } = false;
    public bool EmbedFonts { get; set; } = true;
    
    // Images
    public int MaxPictureWidth { get; set; } = 200;
    public int MaxPictureHeight { get; set; } = 200;
    
    // Storage
    public string FileStoragePath { get; set; } = 
        "~/App_Data/Plugins/Comalytics.ConfigureInvoiceAndPackSlip/Pdfs";
    
    // Processing
    public int NumberOfCopies { get; set; } = 1;
    public bool EnableBackgroundProcessing { get; set; } = true;
}
```

### Dependency Injection

**Renderer Factory Pattern**:
```csharp
services.AddScoped<IPdfRenderer>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<ConfigureInvoiceAndPackSlipSettings>();
    
    return settings.PdfRendererProvider?.ToLowerInvariant() switch
    {
        "chromium" => serviceProvider.GetRequiredService<ChromiumAdapter>(),
        _ => serviceProvider.GetRequiredService<QuestPdfAdapter>()
    };
});
```

**Service Registration**:
```csharp
services.AddScoped<ITemplateService, TemplateService>();
services.AddScoped<IBarcodeService, BarcodeService>();
services.AddScoped<ITokenResolverService, TokenResolverService>();
services.AddScoped<QuestPdfAdapter>();
services.AddScoped<ChromiumAdapter>();
```

## Security Model

### Permission-Based Access Control

**Permissions**:
1. **ManageInvoiceTemplates**: CRUD operations on invoice templates
2. **ManagePackingSlipTemplates**: CRUD operations on packing slip templates
3. **ManagePdfJobs**: View and manage PDF generation jobs
4. **GeneratePdfs**: Generate PDFs for orders

**Default Grants**: All permissions to Administrators role

### Data Security

**Token Output Sanitization**:
- All string tokens are HTML-escaped using `HttpUtility.HtmlEncode()`
- Product names, customer names, addresses are sanitized
- Prevents XSS in PDF content

**Template Security**:
- Templates restricted to admin users only
- No arbitrary code execution (HTML only)
- External resource fetching disabled/proxied

**File Storage**:
- PDFs stored in configured plugin directory
- Access controlled via nopCommerce permissions
- File paths validated to prevent directory traversal

## Performance Considerations

### Optimization Strategies

1. **Async Operations**: All database and file I/O is async
2. **Lazy Loading**: Template HTML only loaded when needed
3. **Caching**: Settings cached via nopCommerce ISettingService
4. **Batch Processing**: Background jobs for multiple PDFs
5. **Image Optimization**: Configurable max dimensions

### Scalability

**Horizontal Scaling**:
- Stateless services
- No in-memory state
- Database-backed job queue

**Vertical Scaling**:
- QuestPDF is CPU-bound but efficient
- Memory usage proportional to PDF complexity
- Chromium requires more resources (1 instance per render)

**Performance Metrics** (estimated):
- QuestPDF: ~100-200ms per invoice (simple)
- QuestPDF: ~500-1000ms per invoice (complex with images)
- Chromium: ~1-3 seconds per invoice (includes browser launch)

## Error Handling

### Exception Strategy

**Service Layer**:
- `ArgumentNullException`: For null required parameters
- `InvalidOperationException`: For invalid state
- `NotImplementedException`: For stub methods (Chromium)

**Controller Layer**:
- Try-catch with user-friendly error messages
- Notification service for success/error feedback
- Logging via nopCommerce logging infrastructure

**Job Processing**:
- Job status updated to Failed on exception
- Error message stored in PdfJob.ErrorMessage
- Retry logic (to be implemented)

## Testing Strategy

### Unit Tests

**Recommended Framework**: xUnit

**Test Coverage**:
1. **Domain Entities**: Enum conversions, property validation
2. **Services**: 
   - TemplateService CRUD operations (mock IRepository)
   - TokenResolverService data transformation (mock dependencies)
   - BarcodeService image generation
3. **Controllers**: Action results, model validation

**Example**:
```csharp
[Fact]
public async Task TokenResolver_ShouldEscapeProductName()
{
    // Arrange
    var order = CreateOrderWithHtmlInProductName();
    var resolver = CreateTokenResolverService();
    
    // Act
    var model = await resolver.ResolveInvoiceTokensAsync(order);
    
    // Assert
    Assert.DoesNotContain("<script>", model.Items[0].Name);
    Assert.Contains("&lt;script&gt;", model.Items[0].Name);
}
```

### Integration Tests

**Test Scenarios**:
1. End-to-end PDF generation
2. Database migrations
3. Template CRUD operations
4. Settings persistence

### Manual Testing

**Checklist**:
- [ ] Plugin installation
- [ ] Settings configuration
- [ ] Template creation
- [ ] PDF generation from order
- [ ] Barcode rendering
- [ ] Multi-language support
- [ ] RTL text rendering
- [ ] Image embedding
- [ ] Permission enforcement

## Deployment Guide

### Prerequisites

- nopCommerce 4.70
- .NET 8.0 SDK
- SQL Server / PostgreSQL / MySQL

### Installation Steps

1. **Copy Plugin**:
   ```
   Copy plugin folder to:
   /Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/
   ```

2. **Build**:
   ```bash
   dotnet build Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.csproj
   ```

3. **Install via Admin**:
   - Navigate to Admin > Configuration > Local plugins
   - Find "Configure Invoice and Pack Slip"
   - Click "Install"
   - Restart application

4. **Configure**:
   - Navigate to Plugins > Invoice & Packing Slips > Configure
   - Set desired PDF renderer
   - Configure page and font settings
   - Save

5. **Verify**:
   - Check Admin > System > Log for errors
   - Verify default templates in database
   - Test PDF generation

### Upgrade Path

**From 4.20**:
1. Export existing templates
2. Uninstall old plugin
3. Install new plugin
4. Import templates
5. Test PDF generation

## Maintenance

### Monitoring

**Key Metrics**:
- PDF generation success rate
- Average generation time
- Failed job count
- Template count per store/language

**Logging**:
- All exceptions logged via nopCommerce ILogger
- Job failures recorded in PdfJob.ErrorMessage
- Audit trail for template changes (via UpdatedOnUtc)

### Backup Strategy

**Database**:
- Regular backups of template tables
- Export important templates as files

**Files**:
- Backup generated PDFs periodically
- Or configure external storage (Azure Blob, S3)

## Future Enhancements

### Planned Features

1. **Template Management UI** (Priority: High)
   - Visual template editor
   - Template preview
   - Template import/export
   - Version history

2. **Advanced Job Processing** (Priority: High)
   - Hangfire integration
   - Job retry logic
   - Job scheduling
   - Email notifications

3. **Enhanced Rendering** (Priority: Medium)
   - Chart/graph support
   - Conditional content
   - Dynamic sections
   - Custom fonts upload

4. **Integration** (Priority: Medium)
   - Order list bulk actions
   - Email attachment automation
   - Webhook notifications
   - API endpoints

5. **Analytics** (Priority: Low)
   - PDF download tracking
   - Template usage statistics
   - Performance dashboard

### Technical Debt

- Chromium adapter full implementation
- Comprehensive unit test suite
- Performance benchmarking
- Visual regression testing
- Documentation i18n

## Appendix

### A. Token Reference

Complete list of supported tokens:

**Order Tokens**:
- `{Order.Number}` - Custom order number
- `{Order.Date}` - Order creation date
- `{Order.SubTotal}` - Order subtotal (incl. tax)
- `{Order.ShippingTotal}` - Shipping cost (incl. tax)
- `{Order.Tax}` - Tax amount
- `{Order.Total}` - Grand total
- `{Order.Items}` - Auto-generated items table

**Customer Tokens**:
- `{Customer.Name}` - Full customer name
- `{Customer.Email}` - Email address

**Address Tokens**:
- `{Billing.Address}` - Formatted billing address
- `{Shipping.Address}` - Formatted shipping address

**Payment Tokens**:
- `{Payment.Method}` - Payment method system name

**Shipping Tokens**:
- `{Shipping.Method}` - Shipping method name
- `{Shipment.TrackingNumber}` - Tracking number

**Store Tokens**:
- `{Store.Name}` - Store name
- `{Store.Url}` - Store URL

**Special Tokens**:
- `{BarCode}` - Order number as CODE_128 barcode (Base64 PNG)
- `{Currency}` - Currency code

### B. Database Schema DDL

Complete schema creation script:

```sql
-- Invoice Templates
CREATE TABLE Coma_InvoiceTemplate (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StoreId INT NOT NULL,
    LanguageId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    TemplateHtml NVARCHAR(MAX) NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    RenderModeId INT NOT NULL,
    CreatedOnUtc DATETIME2 NOT NULL,
    UpdatedOnUtc DATETIME2 NOT NULL
);

-- Packing Slip Templates
CREATE TABLE Coma_PackingSlipTemplate (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StoreId INT NOT NULL,
    LanguageId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    TemplateHtml NVARCHAR(MAX) NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    RenderModeId INT NOT NULL,
    CreatedOnUtc DATETIME2 NOT NULL,
    UpdatedOnUtc DATETIME2 NOT NULL
);

-- PDF Jobs
CREATE TABLE Coma_PdfJob (
    Id INT PRIMARY KEY IDENTITY(1,1),
    JobTypeId INT NOT NULL,
    StatusId INT NOT NULL,
    RequestedByCustomerId INT NOT NULL,
    ParametersJson NVARCHAR(MAX) NULL,
    CreatedOnUtc DATETIME2 NOT NULL,
    CompletedOnUtc DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL
);

-- Generated PDFs
CREATE TABLE Coma_GeneratedPdf (
    Id INT PRIMARY KEY IDENTITY(1,1),
    JobId INT NOT NULL,
    OrderId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    Size BIGINT NOT NULL,
    CreatedOnUtc DATETIME2 NOT NULL,
    FOREIGN KEY (JobId) REFERENCES Coma_PdfJob(Id) ON DELETE CASCADE
);

-- Indexes (recommended for production)
CREATE INDEX IX_InvoiceTemplate_StoreLanguage 
ON Coma_InvoiceTemplate(StoreId, LanguageId, IsDefault);

CREATE INDEX IX_PackingSlipTemplate_StoreLanguage 
ON Coma_PackingSlipTemplate(StoreId, LanguageId, IsDefault);

CREATE INDEX IX_PdfJob_Status 
ON Coma_PdfJob(StatusId, CreatedOnUtc DESC);

CREATE INDEX IX_GeneratedPdf_Order 
ON Coma_GeneratedPdf(OrderId, CreatedOnUtc DESC);

CREATE INDEX IX_GeneratedPdf_Job 
ON Coma_GeneratedPdf(JobId);
```

### C. Configuration Examples

**Basic Configuration** (QuestPDF):
```json
{
  "PdfRendererProvider": "QuestPdf",
  "PageOrientation": "Portrait",
  "PageSize": "A4",
  "TopMargin": 20,
  "BottomMargin": 20,
  "LeftMargin": 20,
  "RightMargin": 20,
  "FontFamily": "Arial",
  "FontSize": 10,
  "EnableRtl": false,
  "MaxPictureWidth": 200,
  "MaxPictureHeight": 200,
  "EmbedFonts": true,
  "NumberOfCopies": 1,
  "EnableBackgroundProcessing": true
}
```

**RTL Configuration** (Arabic/Hebrew):
```json
{
  "FontFamily": "Traditional Arabic",
  "EnableRtl": true,
  "EmbedFonts": true
}
```

**Chromium Configuration**:
```json
{
  "PdfRendererProvider": "Chromium",
  "PageSize": "Letter"
}
```

### D. API Usage Examples

**Generate Invoice PDF**:
```csharp
// In a controller or service
public async Task<byte[]> GenerateInvoicePdfAsync(int orderId)
{
    // Get order
    var order = await _orderService.GetOrderByIdAsync(orderId);
    
    // Resolve tokens
    var model = await _tokenResolverService.ResolveInvoiceTokensAsync(order);
    
    // Apply settings
    model.FontFamily = _settings.FontFamily;
    model.FontSize = _settings.FontSize;
    model.EnableRtl = _settings.EnableRtl;
    
    // Generate PDF
    var pdfBytes = await _pdfRenderer.RenderInvoiceToPdfAsync(model);
    
    return pdfBytes;
}
```

**Download as File**:
```csharp
public async Task<IActionResult> DownloadInvoice(int orderId)
{
    var pdfBytes = await GenerateInvoicePdfAsync(orderId);
    var order = await _orderService.GetOrderByIdAsync(orderId);
    
    return File(pdfBytes, "application/pdf", 
        $"invoice-{order.CustomOrderNumber}.pdf");
}
```

---

**Document Version**: 1.0
**Last Updated**: 2024-12-02
**Author**: Comalytics Development Team
**Status**: Final
