using System.Threading.Tasks;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Barcode
{
    /// <summary>
    /// Barcode generation service interface
    /// </summary>
    public interface IBarcodeService
    {
        /// <summary>
        /// Generates a barcode image as base64 string
        /// </summary>
        /// <param name="content">Barcode content</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>Base64 encoded image string</returns>
        Task<string> GenerateBarcodeBase64Async(string content, int width = 250, int height = 100);

        /// <summary>
        /// Generates a barcode image as bytes
        /// </summary>
        /// <param name="content">Barcode content</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>Barcode image bytes</returns>
        Task<byte[]> GenerateBarcodeBytesAsync(string content, int width = 250, int height = 100);
    }
}
