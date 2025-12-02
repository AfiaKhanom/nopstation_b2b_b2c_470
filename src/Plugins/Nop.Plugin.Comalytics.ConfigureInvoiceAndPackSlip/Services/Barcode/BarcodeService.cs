using System;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Barcode
{
    /// <summary>
    /// Barcode generation service implementation using ZXing
    /// </summary>
    public class BarcodeService : IBarcodeService
    {
        public virtual async Task<string> GenerateBarcodeBase64Async(string content, int width = 250, int height = 100)
        {
            var bytes = await GenerateBarcodeBytesAsync(content, width, height);
            return Convert.ToBase64String(bytes);
        }

        public virtual async Task<byte[]> GenerateBarcodeBytesAsync(string content, int width = 250, int height = 100)
        {
            return await Task.Run(() =>
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = 10
                    }
                };

                var pixelData = writer.Write(content);
                
                // Convert pixel data to PNG bytes
                using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    var bitmapData = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                    try
                    {
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    using (var ms = new System.IO.MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            });
        }
    }
}
