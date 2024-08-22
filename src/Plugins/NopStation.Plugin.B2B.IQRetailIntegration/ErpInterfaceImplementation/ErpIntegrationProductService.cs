using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.Model;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock;
using NopStation.Plugin.B2B.IQRetailIntegration.Service;

namespace NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation;

public class ErpIntegrationProductService
{
    #region Fields

    private readonly IErpLogsService _erpLogs;
    private readonly IErpNopMapperService _erpNopMapperService;
    private readonly IIQRetailService _iQRetailService;
    private readonly IQRetailHttpClient _iQRetailHttpClient;
    private readonly IQRetailIntegrationSettings _retailIntegrationSettings;

    #endregion

    #region Ctor

    public ErpIntegrationProductService(
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

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpProductDataModel>();
        var responseContent = string.Empty;

        try
        {
            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " + 
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and code like '%@StockCode%'",
                new
                {
                    StockCode = erpRequest.ProductSku
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                        requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Product);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Product record in Erp for Product ({erpRequest.ProductSku}): {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = (await _erpNopMapperService.ErpProductMapNop(erpStockResponseData))[0];
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpProductDataModel>>();
        var responseContent = string.Empty;

        try
        {
            var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " +
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and (case when [created] > [modified] then [created] else [modified] end) >= '@DateFrom' and Code > @StartCode " +
                "Order By Code " +
                "Top @RecordLimit",
                new
                {
                    DateFrom = dateFrom,
                    StartCode = erpRequest.Start == "0" ? "NULL" : $"'{erpRequest.Start}'",
                    RecordLimit = _retailIntegrationSettings.DefaultLimit
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                        requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Product);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Product record in Erp: {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = await _erpNopMapperService.ErpProductMapNop(erpStockResponseData);
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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

    public async Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpPriceGroupPricingDataModel>();
        var responseContent = string.Empty;

        try
        {
            var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

            var sqlText = _iQRetailService.ParameterizeQueryString("Select code, " +
                "sellprice1, sellprice2, sellprice3, sellprice4, sellprice5, sellprice6, sellprice7, sellprice8, sellprice9, sellprice10, " +
                "(COALESCE(OnHand,0) - COALESCE(WIPQTY,0) - COALESCE(LBOnHand,0) - COALESCE(SalesOrder,0)) as OnHand," +
                "(case when [created] > [modified] then [created] else [modified] end) as Timestamp " +
                "From Stock Where webItem = true and (" +
                    $"((case when [created] > [modified] then [created] else [modified] end) > '@DateFrom') OR " +
                    $"((case when [created] > [modified] then [created] else [modified] end) = '@DateFrom')" +
                ") Order By Timestamp, code",
                new
                {
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

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.GroupPrice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootRecordData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Group Price record in Erp: {rootRecordData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;

                return erpResponseData;
            }

            var erpStockResponseData = rootRecordData.StockRecordModel.ErpStockRecords.ToList();
            dateFrom = erpStockResponseData.Count > 0 ? erpStockResponseData[erpStockResponseData.Count - 1].Timestamp.ToString() : DateTime.MaxValue.ToString();

            erpResponseData.Data = (await _erpNopMapperService.ErpGroupPriceMapNop(erpStockResponseData))[0];
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = dateFrom
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

    public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPriceGroupPricingDataModel>>();
        var responseContent = string.Empty;

        try
        {
            var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

            var sqlText = _iQRetailService.ParameterizeQueryString("Select code, " +
                "sellprice1, sellprice2, sellprice3, sellprice4, sellprice5, sellprice6, sellprice7, sellprice8, sellprice9, sellprice10, " +
                "(COALESCE(OnHand,0) - COALESCE(WIPQTY,0) - COALESCE(LBOnHand,0) - COALESCE(SalesOrder,0)) as OnHand," +
                "(case when [created] > [modified] then [created] else [modified] end) as Timestamp " +
                "From Stock Where webItem = true and (" +
                    $"((case when [created] > [modified] then [created] else [modified] end) > '@DateFrom') OR " +
                    $"((case when [created] > [modified] then [created] else [modified] end) = '@DateFrom')" +
                ") Order By Timestamp, code",
                new
                {
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

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.GroupPrice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootRecordData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Group Price record in Erp: {rootRecordData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootRecordData.StockRecordModel.ErpStockRecords.ToList();
            dateFrom = erpStockResponseData.Count > 0 ? erpStockResponseData[erpStockResponseData.Count - 1].Timestamp.ToString("yyyy-MM-dd") : DateTime.MaxValue.ToString("yyyy-MM-dd");

            erpResponseData.Data = await _erpNopMapperService.ErpGroupPriceMapNop(erpStockResponseData);
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = dateFrom
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

    public async Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpPriceSpecialPricingDataModel>();
        var responseContent = string.Empty;

        try
        {
            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " +
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and code like '%@StockCode%'",
                new
                {
                    StockCode = erpRequest.ProductSku
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                        requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.SpecialPrice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Special Price record in Erp for Product ({erpRequest.ProductSku}): {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = (await _erpNopMapperService.ErpSpecialPriceMapNop(erpStockResponseData))[0];
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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

    public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>();
        var responseContent = string.Empty;

        try
        {
            var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " +
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and (case when [created] > [modified] then [created] else [modified] end) >= '@DateFrom' and Code > @StartCode " +
                "Order By Code " +
                "Top @RecordLimit",
                new
                {
                    DateFrom = dateFrom,
                    StartCode = erpRequest.Start == "0" ? "NULL" : $"'{erpRequest.Start}'",
                    RecordLimit = _retailIntegrationSettings.DefaultLimit
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                       requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.SpecialPrice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Special Price record in Erp: {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = await _erpNopMapperService.ErpSpecialPriceMapNop(erpStockResponseData);
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpStockDataModel>();
        var responseContent = string.Empty;

        try
        {
            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " +
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and code like '%@StockCode%'",
                new
                {
                    StockCode = erpRequest.ProductSku
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                        requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Stock);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData != null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Product record in Erp for Product ({erpRequest.ProductSku}): {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = (await _erpNopMapperService.ErpStockMapNop(erpStockResponseData))[0];
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpStockDataModel>>();
        var responseContent = string.Empty;

        try
        {
            var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

            var sqlText = _iQRetailService.ParameterizeQueryString(
                "Select s.*, " +
                "d.Descriptio DepartmentName, " +
                "g.Descriptio GroupName, " +
                "c.Description CategoryName, " +
                "co.Colour, " +
                "si.Size, " +
                "b.Description BrandName " +
                "From Stock s " +
                "Left Join categories c on s.category = c.Category " +
                "Left Join Deptmnts d on s.Department = d.Department " +
                "Left Join Groups g on s.Subdepartm = g.group " +
                "Left Join colours co on s.colormatrix = co.colournum " +
                "Left Join sizes si on s.sizematrix = si.sizenum " +
                "Left Join Brand b on s.Brand = b.code " +
                "Where webItem = true and (case when [created] > [modified] then [created] else [modified] end) >= '@DateFrom' and Code > @StartCode " +
                "Order By Code " +
                "Top @RecordLimit",
                new
                {
                    DateFrom = dateFrom,
                    StartCode = erpRequest.Start == "0" ? "NULL" : $"'{erpRequest.Start}'",
                    RecordLimit = _retailIntegrationSettings.DefaultLimit
                });

            var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
                                        requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
                                        filterType: string.Empty,
                                        textFilter: string.Empty,
                                        recordOffset: 0,
                                        sqlText: sqlText).ToString();

            if (!_iQRetailService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
                return erpResponseData;
            }

            var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLevel.Stock);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var rootData = JsonConvert.DeserializeObject<IQApiStockRecordModel>(responseContent);

            if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Product record in Erp: {rootData.IQApiErrors[0].ErrorDescription}";
                erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
                return erpResponseData;
            }

            var erpStockResponseData = rootData.StockRecordModel.ErpStockRecords.ToList();

            erpResponseData.Data = await _erpNopMapperService.ErpStockMapNop(erpStockResponseData);
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                Next = erpStockResponseData[erpStockResponseData.Count - 1].Code
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