using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers
{
    public class ErpAccountPublicController : BasePluginController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly IPermissionService _permissionService;
        private readonly IErpAccountPublicModelFactory _erpAccountPublicModelFactory;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
        private readonly IErpLogsService _erpLogsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public ErpAccountPublicController(
            ICustomerService customerService,
            IWorkContext workContext,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpAccountService erpAccountService,
            IErpNopUserService erpNopUserService,
            IPermissionService permissionService,
            IErpAccountPublicModelFactory erpAccountPublicModelFactory,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            ISettingService settingService,
            IStoreContext storeContext,
            IErpIntegrationPluginManager erpIntegrationPluginService,
            IErpLogsService erpLogsService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpAccountService = erpAccountService;
            _erpNopUserService = erpNopUserService;
            _permissionService = permissionService;
            _erpAccountPublicModelFactory = erpAccountPublicModelFactory;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _settingService = settingService;
            _storeContext = storeContext;
            _erpIntegrationPluginService = erpIntegrationPluginService;
            _erpLogsService = erpLogsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Utilities

        private async Task<(ErpAccount erpAccount, ErpNopUser erpNopUser)> GetErpAccountAndUserOfCurrentCustomerAsync(int customerId)
        {
            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customerId);
            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);

            return (erpAccount, erpNopUser);
        }

        #endregion

        #region Methods

        #region Account/Financial Transation

        public async Task<IActionResult> ErpAccountInfo()
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currCustomer))
                return Challenge();

            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
            if (erpAccount == null)
                return RedirectToRoute("CustomerInfo");

            var model = await _erpAccountPublicModelFactory.PrepareErpAccountInfoModelAsync(erpAccount, new ErpAccountInfoModel());
            return View(model);
        }

        public async Task<IActionResult> LoadErpAccountInfoFromErp()
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
            if (erpAccount == null)
                return new NullJsonResult();

            var model = await _erpAccountPublicModelFactory.PrepareErpAccountInfoModelAsync(erpAccount, new ErpAccountInfoModel(), enableErpAccountUpdate: true);
            return Json(new
            {
                CreditLimit = model.CreditLimit,
                CurrentBalance = model.CurrentBalance,
                AvailableCredit = model.AvailableCredit,
                LastPaymentAmount = model.LastPaymentAmount,
                LastPaymentDate = model.LastPaymentDate
            });
        }

        public async Task<IActionResult> ErpAccountInvoices()
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currCustomer))
                return Challenge();

            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
            if (erpAccount == null)
                return RedirectToRoute("CustomerInfo");

            var model = await _erpAccountPublicModelFactory.PrepareErpAccountInfoModelAsync(erpAccount, new ErpAccountInfoModel());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadErpFinancialTransactionList(ErpAccountInfoModel searchModel)
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);

            if (erpAccount == null || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRole())
                return await AccessDeniedDataTablesJson();

            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BFinancialTransactions))
                return await AccessDeniedDataTablesJson();

            if (erpAccount.Id > 0)
                searchModel.ErpAccountId = erpAccount.Id;

            if (erpNopUser != null)
            {
                searchModel.ErpNopUserId = erpNopUser.Id;
            }

            var model = await _erpAccountPublicModelFactory.PrepareRecentTransactionListAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> DownloadInvoice(string id)
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is null)
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Invoice, "No integration method found.");
                return null;
            }

            try
            {
                var erpGetRequestModel = new ErpGetRequestModel
                {
                    DocumentNumber = id
                };

                var response = await erpIntegrationPlugin.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpGetRequestModel);

                if (!response.ErpResponseModel.IsError)
                {
                    var base64PDFData = response.Data;

                    if (base64PDFData is not null)
                    {
                        // Decode the base64 string
                        byte[] pdfBytes = Convert.FromBase64String(base64PDFData);

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
                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Invoice, response.ErpResponseModel.ErrorShortMessage, response.ErpResponseModel.ErrorFullMessage);
                }
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));

                return RedirectToAction("ErpAccountInvoices");
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLavel.Account, ex.Message, ex.StackTrace);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));
                return RedirectToAction("ErpAccountInvoices");
            }
        }

        public async Task<IActionResult> DownloadInvoiceFromFtp(string id)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

            var baseUrl = b2BB2CFeaturesSettings.DownloadInvoicesPath + "/";
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

                        var counter = 1;
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
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLavel.Account, ex.Message, ex.StackTrace);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));
                return RedirectToAction("ErpAccountInvoices");
            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"));

            return RedirectToAction("ErpAccountInvoices");
        }

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? null;
        }

        #endregion

        #region Orders/Quotes

        public async Task<IActionResult> ErpAccountOrders()
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currCustomer))
                return Challenge();

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);

            if (erpAccount == null)
                return RedirectToRoute("CustomerInfo");

            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BOrders))
                return AccessDeniedView();

            var model = await _erpAccountPublicModelFactory.PrepareErpAccountOrderSearchModelAsync(erpAccount, erpNopUser, new ErpAccountOrderSearchModel());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadB2BAccountOrderList(ErpAccountOrderSearchModel searchModel)
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);
            if (erpAccount == null && erpNopUser == null)
                return await AccessDeniedDataTablesJson();

            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BOrders))
                return await AccessDeniedDataTablesJson();

            var model = new ErpAccountOrderListModel();
            if (erpNopUser?.ErpUserType == ErpUserType.B2BUser)
            {
                searchModel.ErpAccountId = erpAccount.Id;
                searchModel.ErpAccountNumber = erpAccount.AccountNumber;
                model = await _erpAccountPublicModelFactory.PrepareErpOrderListModelAsync(searchModel);
            }
            else if (erpNopUser?.ErpUserType == ErpUserType.B2CUser)
            {
                searchModel.ErpAccountId = erpAccount.Id;
                searchModel.ErpAccountNumber = erpAccount.AccountNumber;
                searchModel.ErpNopUserId = erpNopUser.Id;

                var store = await _storeContext.GetCurrentStoreAsync();
                var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
                if (b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
                    searchModel.NopCustomerId = currCustomer.Id;

                model = await _erpAccountPublicModelFactory.PrepareErpOrderListModelAsync(searchModel);
            }

            return Json(model);
        }

        public async Task<IActionResult> ErpAccountQuoteOrders()
        {
            var customer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);
            if (erpAccount == null)
                return RedirectToRoute("CustomerInfo");

            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BQuotes))
                return AccessDeniedView();

            var model = await _erpAccountPublicModelFactory.PrepareErpAccountQuoteOrderSearchModelAsync(erpAccount, erpNopUser, new ErpAccountQuoteOrderSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadErpQuoteOrderList(ErpAccountQuoteOrderSearchModel searchModel)
        {
            var customer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);
            if (erpAccount == null && erpNopUser == null)
                return await AccessDeniedDataTablesJson();

            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BQuotes))
                return await AccessDeniedDataTablesJson();

            var model = new ErpQuoteOrderListModel();
            if (erpNopUser?.ErpUserType == ErpUserType.B2BUser)
            {
                searchModel.ErpAccountId = erpAccount.Id;
                searchModel.ErpAccountNumber = erpAccount.AccountNumber;
                model = await _erpAccountPublicModelFactory.PrepareErpQuoteOrderListModelAsync(searchModel);
            }
            else if (erpNopUser?.ErpUserType == ErpUserType.B2CUser)
            {
                searchModel.ErpNopUserId = erpNopUser.Id;
                model = await _erpAccountPublicModelFactory.PrepareErpQuoteOrderListModelAsync(searchModel);
            }

            return Json(model);
        }

        #endregion

        #endregion
    }
}