using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.Model;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice;
using NopStation.Plugin.B2B.IQRetailIntegration.Service;

namespace NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation
{
    public class ErpIntegrationOrderService
    {
        #region Fields

        private readonly IErpLogsService _erpLogs;
        private readonly IErpNopMapperService _erpNopMapperService;
        private readonly IIQRetailService _iQRetailService;
        private readonly IQRetailHttpClient _iQRetailHttpClient;
        private readonly IQRetailIntegrationSettings _retailIntegrationSettings;

        #endregion

        #region Ctor

        public ErpIntegrationOrderService(IErpLogsService erpLogs,
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

        public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
        {
            var erpResponseModel = new ErpResponseModel();
            try
            {
                var iqApiSubmitDocument = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = "";
                var requestHeader = "";

                if (erpRequest.OrderType == ERPIntegrationCoreDefaults.ErpB2BOrderType)
                {
                    requestHeader = IQRetailIntegrationDefaults.IQApiSubmitDocumentSalesOrder;
                    serialized = iqApiSubmitDocument.CreateNewOrderPayload(erpRequest, requestHeader, IQRetailIntegrationDefaults.ExportClassSalesOrder).ToString();
                }
                else if (erpRequest.OrderType == ERPIntegrationCoreDefaults.ErpB2BQuoteType)
                {
                    requestHeader = IQRetailIntegrationDefaults.IQApiSubmitDocumentQuote;
                    serialized = iqApiSubmitDocument.CreateNewOrderPayload(erpRequest, requestHeader, IQRetailIntegrationDefaults.ExportClassQuote).ToString();
                }

                if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                    return erpResponseModel;
                }

                await _erpLogs.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, $"ERP {erpRequest.OrderType} payload prepared.", serialized);

                var response = await _iQRetailHttpClient.HttpCall(requestHeader, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseModel;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var checkDataError = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);

                if (checkDataError is not null && checkDataError.IQApiErrors.Count > 0)
                {
                    if (checkDataError.IQApiErrors[0].ErrorCode == 0)
                    {
                        List<IQApiSuccessItemModel> successData = checkDataError.IQApiSuccess.IQApiSuccessItems.Count > 0 ? checkDataError.IQApiSuccess.IQApiSuccessItems[0] : new List<IQApiSuccessItemModel>();

                        if (successData.Count > 0)
                        {
                            erpResponseModel.OrderNumber = successData[0].Data;
                        }
                    }
                    else
                    {
                        erpResponseModel.IsError = true;
                        erpResponseModel.ErrorShortMessage = checkDataError.IQApiErrors[0].ErrorDescription;
                        erpResponseModel.ErrorFullMessage = responseContent;
                    }
                }
                else
                {
                    erpResponseModel.IsError = true;
                    erpResponseModel.ErrorShortMessage = response.ToString();
                }
            }
            catch (Exception ex)
            {
                erpResponseModel.IsError = true;
                erpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseModel;
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();

            try
            {
                #region First get the document numbers

                var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

                var sqlText = _iQRetailService.ParameterizeQueryString("Select document From SOrders Where AccNum = '@AccountNumber' AND (OrdDate >= '@DateFrom' OR ChangedDate >= '@DateFrom') Order By document",
                    new
                    {
                        AccountNumber = erpRequest.AccountNumber,
                        DateFrom = dateFrom
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
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

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootRecordData = JsonConvert.DeserializeObject<IQApiResultInvoiceRecordModel>(responseContent);
                var erpOrderInvByAccResponseData = rootRecordData.InvoiceRecordModel.ErpInvoiceRecords.ToList();

                if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0 || !erpOrderInvByAccResponseData.Any())
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Order record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                #endregion

                #region Then get the invoices

                var documents = "(" + string.Join(" , ", erpOrderInvByAccResponseData.Select(invRecords => $"'{invRecords.Document}'")) + ")";

                var textFilter = _iQRetailService.ParameterizeQueryString("document In @documents",
                    new
                    {
                        documents = documents
                    });

                serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder, serialized, ErpSyncLevel.Order);
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
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Order record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
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
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("document = '@OrderNumber'",
                    new
                    {
                        OrderNumber = erpRequest.OrderNumber
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();


                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Order record in Erp for Erp Order ({erpRequest.OrderNumber}): {rootData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByNopOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("ordernum = '@NopOrderNumber'",
                    new
                    {
                        NopOrderNumber = erpRequest.OrderNumber
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();


                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentSalesOrder, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Order record in Erp for Nop Order ({erpRequest.OrderNumber}): {rootData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();

            try
            {
                #region First get the document numbers

                var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

                var sqlText = _iQRetailService.ParameterizeQueryString("Select document From Quotes Where AccNum = '@AccountNumber' AND (OrdDate >= '@DateFrom' OR ChangedDate >= '@DateFrom') Order By document",
                    new
                    {
                        AccountNumber = erpRequest.AccountNumber,
                        DateFrom = dateFrom
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
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

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootRecordData = JsonConvert.DeserializeObject<IQApiResultInvoiceRecordModel>(responseContent);
                var erpOrderInvByAccResponseData = rootRecordData.InvoiceRecordModel.ErpInvoiceRecords.ToList();

                if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0 || !erpOrderInvByAccResponseData.Any())
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Quote record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                #endregion

                #region Then get the invoices

                var documents = "(" + string.Join(" , ", erpOrderInvByAccResponseData.Select(invRecords => $"'{invRecords.Document}'")) + ")";

                var textFilter = _iQRetailService.ParameterizeQueryString("document In @documents",
                    new
                    {
                        documents = documents
                    });

                serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentQuote,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentQuote, serialized, ErpSyncLevel.Order);
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
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Quote record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
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
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }
            return erpResponseData;
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();

            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("document = '@QuoteNumber'",
                    new
                    {
                        QuoteNumber = erpRequest.OrderNumber
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentQuote,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();


                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentQuote, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Quote record in Erp for Nop Quote ({erpRequest.OrderNumber}): {rootData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseData;

        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByNopOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();
            try
            {
                var textFilter = _iQRetailService.ParameterizeQueryString("ordernum = '@NopOrderNumber'",
                    new
                    {
                        NopOrderNumber = erpRequest.OrderNumber
                    });

                var iQRetailRequest = new IQApiRequestGeneratorModel(_retailIntegrationSettings);
                var serialized = iQRetailRequest.CreateApiRequest(
                                           requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentQuote,
                                           filterType: IQRetailIntegrationDefaults.EApiFilterText,
                                           textFilter: textFilter,
                                           recordOffset: int.Parse(erpRequest.Start),
                                           sqlText: "").ToString();

                var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentQuote, serialized, ErpSyncLevel.Order);
                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                    return erpResponseData;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
                if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = $"Quote record in Erp for Nop Order ({erpRequest.OrderNumber}): {rootData?.IQApiErrors?[0].ErrorDescription}";
                    erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                    return erpResponseData;
                }

                var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpInvByDocsResponseData);
                erpResponseData.ErpResponseModel = new ErpResponseModel
                {
                    Next = rootData.IQApiPageData.NextOffset.ToString()
                };
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
            }

            return erpResponseData;
        }

        #endregion
    }
}