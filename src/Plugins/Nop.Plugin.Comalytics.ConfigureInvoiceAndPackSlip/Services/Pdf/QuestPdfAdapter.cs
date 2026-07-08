using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Pdf
{
    /// <summary>
    /// QuestPDF adapter implementation for PDF rendering
    /// </summary>
    public class QuestPdfAdapter : IPdfRenderer
    {
        public QuestPdfAdapter()
        {
            // Set QuestPDF license (Community license for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public virtual async Task<byte[]> RenderInvoiceToPdfAsync(InvoiceRenderModel model, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(model.FontSize > 0 ? model.FontSize : 10));

                        // Header
                        page.Header().Element(c => ComposeInvoiceHeader(c, model));

                        // Content
                        page.Content().Element(c => ComposeInvoiceContent(c, model));

                        // Footer
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }, cancellationToken);
        }

        public virtual async Task<byte[]> RenderPackingSlipToPdfAsync(PackingSlipRenderModel model, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(model.FontSize > 0 ? model.FontSize : 10));

                        // Header
                        page.Header().Element(c => ComposePackingSlipHeader(c, model));

                        // Content
                        page.Content().Element(c => ComposePackingSlipContent(c, model));

                        // Footer
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }, cancellationToken);
        }

        private void ComposeInvoiceHeader(IContainer container, InvoiceRenderModel model)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(model.StoreName ?? "Store").FontSize(20).Bold();
                    column.Item().Text(model.StoreUrl ?? "").FontSize(9);
                });

                row.ConstantItem(100).Column(column =>
                {
                    column.Item().Text("INVOICE").FontSize(20).Bold().AlignRight();
                    column.Item().Text($"#{model.OrderNumber}").FontSize(12).AlignRight();
                    column.Item().Text(model.OrderDate ?? "").FontSize(9).AlignRight();
                });
            });
        }

        private void ComposeInvoiceContent(IContainer container, InvoiceRenderModel model)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Customer and billing info
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Bill To:").Bold();
                        col.Item().Text(model.CustomerName ?? "");
                        col.Item().Text(model.CustomerEmail ?? "");
                        col.Item().Text(model.BillingAddress ?? "");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Ship To:").Bold();
                        col.Item().Text(model.ShippingAddress ?? "");
                    });
                });

                column.Item().PaddingTop(20);

                // Items table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("SKU");
                        header.Cell().Element(CellStyle).Text("Product");
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                        header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                        header.Cell().Element(CellStyle).AlignRight().Text("Total");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    // Rows
                    foreach (var item in model.Items)
                    {
                        table.Cell().Element(CellStyle).Text(item.Sku ?? "");
                        table.Cell().Element(CellStyle).Text(item.Name ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Total ?? "");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                });

                column.Item().PaddingTop(20).AlignRight().Column(col =>
                {
                    col.Item().Text($"Subtotal: {model.SubTotal ?? ""}");
                    col.Item().Text($"Shipping: {model.ShippingTotal ?? ""}");
                    col.Item().Text($"Tax: {model.Tax ?? ""}");
                    col.Item().Text($"Total: {model.Total ?? ""}").Bold();
                });

                // Payment method
                if (!string.IsNullOrEmpty(model.PaymentMethod))
                {
                    column.Item().PaddingTop(20).Text($"Payment Method: {model.PaymentMethod}");
                }
            });
        }

        private void ComposePackingSlipHeader(IContainer container, PackingSlipRenderModel model)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(model.StoreName ?? "Store").FontSize(20).Bold();
                    column.Item().Text(model.StoreUrl ?? "").FontSize(9);
                });

                row.ConstantItem(100).Column(column =>
                {
                    column.Item().Text("PACKING SLIP").FontSize(20).Bold().AlignRight();
                    column.Item().Text($"Order: {model.OrderNumber}").FontSize(12).AlignRight();
                    column.Item().Text(model.OrderDate ?? "").FontSize(9).AlignRight();
                });
            });
        }

        private void ComposePackingSlipContent(IContainer container, PackingSlipRenderModel model)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Shipping info
                column.Item().Column(col =>
                {
                    col.Item().Text("Ship To:").Bold();
                    col.Item().Text(model.CustomerName ?? "");
                    col.Item().Text(model.ShippingAddress ?? "");
                    if (!string.IsNullOrEmpty(model.TrackingNumber))
                        col.Item().Text($"Tracking #: {model.TrackingNumber}");
                });

                column.Item().PaddingTop(20);

                // Items table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("SKU");
                        header.Cell().Element(CellStyle).Text("Product");
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    // Rows
                    foreach (var item in model.Items)
                    {
                        table.Cell().Element(CellStyle).Text(item.Sku ?? "");
                        table.Cell().Element(CellStyle).Text(item.Name ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity ?? "");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                });
            });
        }
    }
}
