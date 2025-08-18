using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpInvoiceController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IErpInvoiceModelFactory _erpInvoiceModelFactory;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;

    #endregion

    #region Ctor

    public ErpInvoiceController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        IErpInvoiceModelFactory erpInvoiceModelFactory,
        IErpInvoiceService erpInvoiceService,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpActivityLogsService erpActivityLogsService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _erpInvoiceModelFactory = erpInvoiceModelFactory;
        _erpInvoiceService = erpInvoiceService;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpActivityLogsService = erpActivityLogsService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Methods

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var model = new ErpInvoiceSearchModel();
        model = await _erpInvoiceModelFactory.PrepareErpInvoiceSearchModelAsync(searchModel: model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpInvoiceList(ErpInvoiceSearchModel erpInvoiceSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpInvoiceModelFactory.PrepareErpInvoiceListModelAsync(erpInvoiceSearchModel);

        return Json(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var erpInvoice = await _erpInvoiceService.GetErpInvoiceByIdAsync(id);
        if (erpInvoice == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpInvoiceModelFactory.PrepareErpInvoiceModelAsync(null, erpInvoice);

        return View(model);
    }


    public async Task<IActionResult> DownloadInvoice(int id)
    {
        if (id < 1)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.IdIsNotValid"));
            return RedirectToAction("List");
        }

        var erpInvoice = await _erpInvoiceService.GetErpInvoiceByIdAsync(id);
        if (erpInvoice == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.NotFound"));
            return RedirectToAction("List");
        }

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
        if (erpAccount == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.B2BAccountNotFound"));
            return RedirectToAction("List");
        }
        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
        if (salesOrg == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.ErpSalesOrgNotFound"));
            return RedirectToAction("List");
        }

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Invoice);
        if (erpIntegrationPlugin is null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.IntegrationMethodFindResult.NoIntegrationMethodFound"));
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Invoice, "NopStation.Plugin.B2B.B2BB2CFeatures.IntegrationMethodFindResult.NoIntegrationMethodFound");
            return RedirectToAction("List");
        }

        try
        {
            var erpGetRequestModel = new ErpGetRequestModel
            {
                DocumentNumber = erpInvoice.ErpDocumentNumber,
                OrderNumber = erpInvoice.ErpOrderNumber,
                Location = salesOrg.Code
            };

            var response = await erpIntegrationPlugin.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpGetRequestModel);

            if (!response.ErpResponseModel.IsError)
            {
                var base64PDFData = response.Data;

                if (base64PDFData is not null)
                {
                    // Decode the base64 string
                    var pdfBytes = Convert.FromBase64String(base64PDFData);

                    // Save the decoded binary data to a PDF file
                    var fileName = $"downloaded_pdf_{Guid.NewGuid()}.pdf";
                    var filePath = Path.Combine(Path.GetTempPath(), fileName);

                    System.IO.File.WriteAllBytes(filePath, pdfBytes);

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_InvoiceDownload",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpInvoiceDownload"),
                        id),
                        new ErpInvoice());

                    // Return the file as a download response
                    return PhysicalFile(filePath, "application/pdf", fileName);
                }
            }
            else
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Invoice, response.ErpResponseModel.ErrorShortMessage, response.ErpResponseModel.ErrorFullMessage);
            }
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));

            return RedirectToAction("List");
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, ex.Message, ex.StackTrace);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.ExceptionOccured"));
            return RedirectToAction("List");
        }
    }


    public async Task<IActionResult> DownloadInvoiceFromFtp(string id)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

        var baseUrl = $"{b2BB2CFeaturesSettings.DownloadInvoicesPath}/";
        //var baseUrl = "ftp://89.116.28.135/RenamedInvoiceTest/";

        try
        {
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(baseUrl);
            listRequest.UsePassive = true;
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = new NetworkCredential(b2BB2CFeaturesSettings.FtpUserName, b2BB2CFeaturesSettings.FtpPassword);

            List<string> lines = new List<string>();
            using (WebResponse listResponse = listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (StreamReader listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            var fileDictionary = new Dictionary<string, List<string>>();

            foreach (var line in lines)
            {
                string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
                var extension = Path.GetExtension(name);
                var invoicePath = Path.Combine(baseUrl, name);

                if (string.IsNullOrEmpty(invoicePath))
                    continue;

                var mimeType = GetMimeTypeFromFilePath(invoicePath);
                if (string.IsNullOrEmpty(mimeType))
                    continue;

                string[] nameTokens = fileNameWithoutExt.Split(new[] { '_' }, 2);
                string invoiceNo = nameTokens[0];


                if (String.Equals(invoiceNo, id, StringComparison.OrdinalIgnoreCase))
                {
                    if (fileDictionary.ContainsKey(invoiceNo))
                    {
                        fileDictionary[invoiceNo].Add(invoicePath);
                    }
                    else
                    {
                        fileDictionary[invoiceNo] = new List<string> { invoicePath };
                    }
                }
            }

            foreach (var filesGroup in fileDictionary.Values)
            {
                if (filesGroup.Count > 0)
                {
                    // If multiple files with the same name, zip them
                    var zipFileName = $"{id}.zip";
                    var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

                    int counter = 1;
                    while (System.IO.File.Exists(zipFilePath))
                    {
                        zipFileName = $"{id}_{counter}.zip";
                        zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
                        counter++;
                    }

                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var ftpFilePath in filesGroup)
                        {
                            var fileName = Path.GetFileName(ftpFilePath);
                            var entry = zipArchive.CreateEntry(fileName);

                            using (var entryStream = entry.Open())
                            {
                                var ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                                ftpRequest.Credentials = new NetworkCredential(b2BB2CFeaturesSettings.FtpUserName, b2BB2CFeaturesSettings.FtpPassword);

                                using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                                using (var ftpStream = ftpResponse.GetResponseStream())
                                {
                                    ftpStream.CopyTo(entryStream);
                                }
                            }
                        }
                    }

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_PODDownload",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpPODDownload"),
                        id),
                        new ErpInvoice());

                    return File(System.IO.File.ReadAllBytes(zipFilePath), "application/zip", zipFileName);
                }
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, ex.Message, ex.StackTrace);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));
            return RedirectToAction("List");
        }

        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));

        return RedirectToAction("List");
    }

    protected virtual string GetMimeTypeFromFilePath(string filePath)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

        //set to jpeg in case mime type cannot be found
        return mimeType ?? null;
    }

    #endregion
}