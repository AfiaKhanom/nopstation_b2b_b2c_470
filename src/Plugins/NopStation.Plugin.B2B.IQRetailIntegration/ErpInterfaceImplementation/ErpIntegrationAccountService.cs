using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.Model;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice;
using NopStation.Plugin.B2B.IQRetailIntegration.Service;

namespace NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation
{
    public class ErpIntegrationAccountService
    {
        #region Fields

        private readonly IErpLogsService _erpLogs;
        private readonly IErpNopMapperService _erpNopMapperService;
        private readonly IIQRetailService _iQRetailService;
        private readonly IQRetailHttpClient _iQRetailHttpClient;
        private readonly IQRetailIntegrationSettings _retailIntegrationSettings;

        #endregion

        #region Ctor

        public ErpIntegrationAccountService(
            IErpLogsService erpLogs,
            IErpNopMapperService erpNopMapperService,
            IIQRetailService iQRetailService,
            IQRetailHttpClient iQRetailHttpClient,
            IQRetailIntegrationSettings retailIntegrationSettings)
        {
            _erpLogs = erpLogs;
            _erpNopMapperService = erpNopMapperService;
            _iQRetailService = iQRetailService;
            _iQRetailHttpClient = iQRetailHttpClient;
            _retailIntegrationSettings = retailIntegrationSettings;
        }

        #endregion

        #region Methods

        public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
        {
            var erpResponseModel = new ErpResponseModel();
            var responseContent = string.Empty;

            try
            {
                if (erpCreateAccountModel.AccountNumber.Length > IQRetailIntegrationDefaults.AccountNoLengthLimit)
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = $"The Account Number {erpCreateAccountModel.AccountNumber}, exceeds the account number maximum length limit {IQRetailIntegrationDefaults.AccountNoLengthLimit}.";
                    return erpResponseModel;
                }

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseModel;
                }

                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateNewAccountPayload(erpCreateAccountModel).ToString();

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiSubmitDebtor, serialized, ErpSyncLevel.Account);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseModel;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var checkDataError = JsonConvert.DeserializeObject<IQApiResultDebtorModel>(responseContent);

                if (checkDataError is not null && checkDataError.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = checkDataError.IQApiErrors[0].ErrorDescription;
                    erpResponseModel.ErrorFullMessage = responseContent;
                }
                else
                {
                    erpResponseModel.AccountNumber = erpCreateAccountModel.AccountNumber;
                }
            }
            catch (Exception ex)
            {
                erpResponseModel.IsError = true;
                erpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseModel;
        }

        public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<ErpAccountDataModel>();
            var responseContent = string.Empty;

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("account like '%@AccountNumber%'",
                    new
                    {
                        AccountNumber = erpRequest.AccountNumber
                    });

                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDebtor,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseData;
                }

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDebtor, serialized, ErpSyncLevel.Account);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultDebtorModel>(responseContent);

                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Account record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootData.IQApiErrors[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpAccountsResponseData = rootData.IQApiResultDataModel.IQRootJson.DebtorsMasters.ToList();
                erpResponseData.Data = (await _erpNopMapperService.ErpAccountMapNop(erpAccountsResponseData))[0];
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpAccountDataModel>>();
            var responseContent = string.Empty;

            try
            {
                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDebtor,
                                           filterType: "",
                                           textFilter: "",
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseData;
                }

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDebtor, serialized, ErpSyncLevel.Account);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultDebtorModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Account record in Erp: {rootData.IQApiErrors[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpAccountsResponseData = rootData.IQApiResultDataModel.IQRootJson.DebtorsMasters.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpAccountMapNop(erpAccountsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpInvoiceDataModel>>();
            var responseContent = string.Empty;

            try
            {
                #region First get the document numbers

                var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

                var sqlText = _iQRetailService.ParameterizeQueryString("Select document From Invoices Where AccNum = '@AccountNumber' AND (OrdDate >= '@DateFrom' OR ChangedDate >= '@DateFrom') Order By document",
                    new
                    {
                        AccountNumber = erpRequest.AccountNumber,
                        DateFrom = dateFrom
                    });

                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                           filterType: "",
                                           textFilter: "",
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: sqlText).ToString();

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseData;
                }

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Invoice);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootRecordData = JsonConvert.DeserializeObject<IQApiResultInvoiceRecordModel>(responseContent);
                var erpInvByAccResponseData = rootRecordData.InvoiceRecordModel.ErpInvoiceRecords.ToList();

                if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0 || !erpInvByAccResponseData.Any())
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                #endregion

                #region Then get the invoices

                var documents = "(" + string.Join(" , ", erpInvByAccResponseData.Select(invRecords => $"'{invRecords.Document}'")) + ")";

                var textFilter = _iQRetailService.ParameterizeQueryString("document In @documents",
                    new
                    {
                        documents = documents
                    });

                serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice, serialized, ErpSyncLevel.Invoice);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpInvoiceMapNop(erpInvByDocsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };

                #endregion
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<string>();
            var responseContent = string.Empty;

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("Document = '@DocumentNumber'",
                    new
                    {
                        DocumentNumber = erpRequest.DocumentNumber,
                    });

                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           embeddingType: IQRetailIntegrationDefaults.PdfEmbedding,
                                           sqlText: "").ToString();

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseData;
                }

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice, serialized, ErpSyncLevel.Invoice);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoicePdfModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Document Number ({erpRequest.DocumentNumber}): {rootData.IQApiErrors[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvPdfByDocNoData = rootData.InvoicePdfModel.IQRootJsonForInvoicePdf.ErpInvoicePdfByteCodes.ToList();
                erpResponseData.Data = erpInvPdfByDocNoData[0].Document;
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpShipToAddressDataModel>>();
            var responseContent = string.Empty;

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("account like '%@AccountNumber%'",
                    new
                    {
                        AccountNumber = erpRequest.AccountNumber
                    });

                var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDebtor,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";

                    return erpResponseData;
                }

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDebtor, serialized, ErpSyncLevel.ShipToAddress);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();

                    return erpResponseData;
                }

                responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultDebtorModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Ship To Address record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootData.IQApiErrors[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpShipAddressByAccResponseData = rootData.IQApiResultDataModel.IQRootJson.DebtorsMasters.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpShipToAddressMapNop(erpShipAddressByAccResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        #endregion
    }
}