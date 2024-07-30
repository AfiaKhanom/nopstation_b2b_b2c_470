using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model
{
    public class IQApiRequestGeneratorModel
    {
        #region Fields

        private readonly IQRetailIntegrationSettings _retailIntegrationSettings;
        private const string IQ_API = "IQ_API";
        private const string IQ_TERMINAL_NUMBER = "IQ_Terminal_Number";
        private const string IQ_USER_NUMBER = "IQ_User_Number";
        private const string IQ_USER_PASSWORD = "IQ_User_Password";
        private const string IQ_COMPANY_NUMBER = "IQ_Company_Number";
        private const string IQ_SQL_TEXT = "IQ_SQL_Text";
        private const string FILTER_TYPE = "Filter_Type";
        private const string TEXT_FILTER = "Text_Filter";
        private const string EMBEDDING_TYPE = "Embedding_Type";
        private const string RECORD_LIMIT = "record_limit";
        private const string RECORD_OFFSET = "record_Offset";
        private const string IDE_INVALID_DATE_RANGE = "ideInvalidDateRange";
        private const string IDE_NEGATIVE_STOCK = "ideNegativeStock";
        private const string PREFERED_SELL_PRICE_RETAIL_PRICE = "Retail Price";
        private const string EXPORT_CLASS_DEBTOR = "Debtor";
        private const string CURRENCY = "ZAR";
        private const string CURRENCY_RATE = "1";
        private const int TOTAL_BALANCE = 0;
        private const int BALANCE_CURRENT = 0;
        private const int BALANCE_30_DAYS = 0;
        private const int BALANCE_60_DAYS = 0;
        private const int BALANCE_90_DAYS = 0;
        private const int BALANCE_120_DAYS = 0;
        private const int BALANCE_150_DAYS = 0;
        private const int BALANCE_180_DAYS = 0;
        private const int REPRESENTATIVE_NUMBER = 1;
        private const int SALES_REPRESENTATIVE_NUMBER = 1;
        private const int CREDIT_APPROVER_NUMBER = 1;
        private const int INVOICE_LAYOUT = 1;
        private const int VOLUME_LENGTH = 0;
        private const int VOLUME_WIDTH = 0;
        private const int VOLUME_HEIGHT = 0;
        private const int VOLUME_QUANTITY = 0;
        private const int VOLUME_VALUE = 0;
        private const int VOLUME_ROUNDING = 0;
        private const int INVOICED_QUANTITY = 0;
        private const int DISCOUNT_AMOUNT = 0;
        private const int PRINT_LAYOUT = 1;
        private const int CASHIER_NUMBER = 1;
        private const bool DOCUMENT_INCLUDES_VAT = false;

        #endregion

        #region Ctor

        public IQApiRequestGeneratorModel(IQRetailIntegrationSettings retailIntegrationSettings)
        {
            _retailIntegrationSettings = retailIntegrationSettings;
        }

        #endregion

        #region Method

        public JObject CreateApiRequest(string requestType = "", string filterType = "", string textFilter = "", int recordOffset = 0, string sqlText = "", string? embeddingType = "")
        {
            return new JObject(
                new JProperty(IQ_API,
                    new JObject(
                        new JProperty(requestType,
                            new JObject(
                                new JProperty(IQ_TERMINAL_NUMBER, Convert.ToInt32(_retailIntegrationSettings.TerminalNumber)),
                                new JProperty(IQ_USER_NUMBER, Convert.ToInt32(_retailIntegrationSettings.UserName)),
                                new JProperty(IQ_USER_PASSWORD, _retailIntegrationSettings.Password),
                                new JProperty(IQ_COMPANY_NUMBER, _retailIntegrationSettings.CompanyId),
                                new JProperty(IQ_SQL_TEXT, sqlText),
                                new JProperty(FILTER_TYPE, filterType),
                                new JProperty(TEXT_FILTER, textFilter),
                                new JProperty(EMBEDDING_TYPE, embeddingType),
                                new JProperty(RECORD_LIMIT, _retailIntegrationSettings.DefaultLimit),
                                new JProperty(RECORD_OFFSET, recordOffset)
                            )
                        )
                    )
                )
            );
        }

        public JObject CreateNewAccountPayload(ErpCreateAccountModel erpCreateAccountModel)
        {
            JObject additionalAddress = new JObject(
                new JProperty("branch_number", string.Empty),
                new JProperty("company_name", string.Empty),
                new JProperty("contact_name", erpCreateAccountModel.ContactName ?? string.Empty),
                new JProperty("contact_number", erpCreateAccountModel.TelNo ?? string.Empty),
                new JProperty("address1", erpCreateAccountModel.Address1 ?? string.Empty),
                new JProperty("address2", erpCreateAccountModel.Address2 ?? string.Empty),
                new JProperty("address3", erpCreateAccountModel.Address3 ?? string.Empty),
                new JProperty("address4", string.Empty),
                new JProperty("postal_code", erpCreateAccountModel.PostalCode ?? string.Empty),
                new JProperty("cellphone", erpCreateAccountModel.TelNo ?? string.Empty),
                new JProperty("fax_number", erpCreateAccountModel.FaxNo ?? string.Empty),
                new JProperty("email_address", erpCreateAccountModel.EMail ?? string.Empty)
            );
            var address = new JArray(
                erpCreateAccountModel.Address1 ?? string.Empty,
                erpCreateAccountModel.Address2 ?? string.Empty,
                erpCreateAccountModel.Address3 ?? string.Empty,
                erpCreateAccountModel.PostalCode ?? string.Empty,
                erpCreateAccountModel.ZipPostalCode ?? string.Empty
                );
            JObject debtor1 = new JObject(
                new JProperty("export_class", EXPORT_CLASS_DEBTOR),
                new JProperty("debtor_account", erpCreateAccountModel.ErpAccountNumber ?? string.Empty),
                new JProperty("email_address", erpCreateAccountModel.EMail ?? string.Empty),
                new JProperty("postal_address_details", address),
                new JProperty("delivery_address_details", address),
                new JProperty("telephone_numbers", new JArray(erpCreateAccountModel.TelNo ?? string.Empty)),
                new JProperty("fax_number", erpCreateAccountModel.FaxNo ?? string.Empty),
                new JProperty("vat_status", erpCreateAccountModel.VatNumber ?? string.Empty),
                new JProperty("total_balance", TOTAL_BALANCE),
                new JProperty("balance_current", BALANCE_CURRENT),
                new JProperty("balance_30_days", BALANCE_30_DAYS),
                new JProperty("balance_60_days", BALANCE_60_DAYS),
                new JProperty("balance_90_days", BALANCE_90_DAYS),
                new JProperty("balance_120_days", BALANCE_120_DAYS),
                new JProperty("balance_150_days", BALANCE_150_DAYS),
                new JProperty("balance_180_days", BALANCE_180_DAYS),
                new JProperty("preferred_sell_price", PREFERED_SELL_PRICE_RETAIL_PRICE),
                new JProperty("currency", CURRENCY),
                new JProperty("debtor_name", erpCreateAccountModel.Name ?? string.Empty),
                new JProperty("debtor_group", string.Empty),
                new JProperty("normal_representative", REPRESENTATIVE_NUMBER),
                new JProperty("additional_addresses", new JArray(additionalAddress)),
                new JProperty("invoice_layout", INVOICE_LAYOUT)
            );

            JObject iqIdentificationInfo = new JObject(
                new JProperty("company_store_id", _retailIntegrationSettings.Location),
                new JProperty("company_code", _retailIntegrationSettings.CompanyId)
            );

            JObject iqRootJson = new JObject(
                new JProperty("iq_identification_info", iqIdentificationInfo),
                new JProperty("debtors_master", new JArray(debtor1))
            );

            JObject iqSubmitData = new JObject(
                new JProperty("iq_root_json", iqRootJson)
            );

            JObject iqApiSubmitDebtor = new JObject(
                new JProperty(IQ_COMPANY_NUMBER, _retailIntegrationSettings.CompanyId),
                new JProperty(IQ_TERMINAL_NUMBER, Convert.ToInt32(_retailIntegrationSettings.TerminalNumber)),
                new JProperty(IQ_USER_NUMBER, Convert.ToInt32(_retailIntegrationSettings.UserName)),
                new JProperty(IQ_USER_PASSWORD, _retailIntegrationSettings.Password),
                new JProperty("IQ_Submit_Data", iqSubmitData)
            );

            JObject iqApi = new JObject(
                new JProperty("IQ_API_Submit_Debtor", iqApiSubmitDebtor)
            );

            var iQRetailSubmit = new JObject(
                new JProperty("IQ_API", iqApi)
            );

            return iQRetailSubmit;
        }

        public JObject CreateNewOrderPayload(ErpPlaceOrderDataModel erpRequest, string requestHeader, string exportClass)
        {
            var items = new JArray();

            foreach (var item in erpRequest.ErpPlaceOrderItemDatas)
            {
                var itemJObject = new JObject(
                    new JProperty("stock_code", item.ItemNo ?? string.Empty),
                    new JProperty("stock_description", item.Description ?? string.Empty),
                    new JProperty("comment", item.SpecInstruct ?? string.Empty),
                    new JProperty("quantity", item.Quantity),
                    new JProperty("volumetrics", new JObject(
                        new JProperty("units", item.UOM ?? string.Empty),
                        new JProperty("volume_length", VOLUME_LENGTH),
                        new JProperty("volume_width", VOLUME_WIDTH),
                        new JProperty("volume_height", VOLUME_HEIGHT),
                        new JProperty("volume_quantity", VOLUME_QUANTITY),
                        new JProperty("volume_value", VOLUME_VALUE),
                        new JProperty("volume_rounding", VOLUME_ROUNDING)
                    )),
                    new JProperty("discount_percentage", item.Discount),
                    new JProperty("line_total_inclusive", item.LineTotalIncl),
                    new JProperty("line_total_exclusive", item.LineTotalIncl),
                    new JProperty("item_price_inclusive", item.UnitPrice),
                    new JProperty("item_price_exclusive", item.UnitPrice),
                    new JProperty("list_price", item.UnitPrice),
                    new JProperty("delcol", string.Empty),
                    new JProperty("invoiced_quantity", INVOICED_QUANTITY)
                );

                items.Add(itemJObject);
            }

            var iqApiSubmitDocument = new JObject(
                new JProperty("IQ_API", new JObject(
                    new JProperty(requestHeader, new JObject(
                        new JProperty(IQ_COMPANY_NUMBER, _retailIntegrationSettings.CompanyId),
                        new JProperty(IQ_TERMINAL_NUMBER, Convert.ToInt32(_retailIntegrationSettings.TerminalNumber)),
                        new JProperty(IQ_USER_NUMBER, Convert.ToInt32(_retailIntegrationSettings.UserName)),
                        new JProperty(IQ_USER_PASSWORD, _retailIntegrationSettings.Password),
                        new JProperty("IQ_Submit_Data", new JObject(
                            new JProperty("iq_root_json", new JObject(
                                new JProperty("iq_identification_info", new JObject(
                                    new JProperty("company_store_id", _retailIntegrationSettings.Location),
                                    new JProperty("company_code", _retailIntegrationSettings.CompanyId)
                                )),
                                new JProperty("processing_documents", new JArray(
                                    new JObject(
                                        new JProperty("export_class", exportClass),
                                        new JProperty("document", new JObject(
                                            new JProperty("document_reference", erpRequest.CustomerReference ?? string.Empty),
                                            new JProperty("order_number", erpRequest.Reference ?? string.Empty),
                                            new JProperty("delivery_method", erpRequest.DelMethod ?? string.Empty),
                                            new JProperty("delivery_note_number", erpRequest.DelInstruction1 ?? string.Empty),
                                            new JProperty("delivery_address_information", new JArray(
                                                erpRequest.DelInstruction1 ?? string.Empty
                                            )),
                                            new JProperty("total_vat", erpRequest.Total_Vat),
                                            new JProperty("email_address", erpRequest.CustomerEmail ?? string.Empty),
                                            new JProperty("vat_number", erpRequest.TaxNumber ?? string.Empty),
                                            new JProperty("document_total", erpRequest.Total_Excl),
                                            new JProperty("discount_amount", DISCOUNT_AMOUNT),
                                            new JProperty("print_layout", PRINT_LAYOUT),
                                            new JProperty("cashier_number", CASHIER_NUMBER),
                                            new JProperty("document_includes_vat", DOCUMENT_INCLUDES_VAT),
                                            new JProperty("currency", CURRENCY),
                                            new JProperty("currency_rate", CURRENCY_RATE),
                                            new JProperty("telephone_number", erpRequest.CustomerPhoneNumber ?? string.Empty),
                                            new JProperty("debtor_account", erpRequest.AccNo ?? string.Empty),
                                            new JProperty("sales_representative_number", SALES_REPRESENTATIVE_NUMBER),
                                            new JProperty("order_information", new JObject(
                                                new JProperty("order_date", (erpRequest.OrderDate > DateTime.UtcNow.AddYears(-100)) ?
                                                                            erpRequest.OrderDate.ToString("yyyy-MM-dd") : DateTime.UtcNow.AddYears(-100).ToString("yyyy-MM-dd")),
                                                new JProperty("expected_date", (erpRequest.DateRequired > DateTime.UtcNow.AddYears(-100)) ?
                                                                            erpRequest.DateRequired.ToString("yyyy-MM-dd") : DateTime.UtcNow.AddYears(-100).ToString("yyyy-MM-dd")),
                                                new JProperty("credit_approver_number", CREDIT_APPROVER_NUMBER)
                                            ))
                                        )),
                                        new JProperty("items", items)
                                    )
                                )
                            ))
                            ),
                            new JProperty("IQ_Overrides", new JArray(IDE_NEGATIVE_STOCK, IDE_INVALID_DATE_RANGE))
                        )
                    ))
                ))
            ));

            return iqApiSubmitDocument;
        }

        #endregion
    }
}
