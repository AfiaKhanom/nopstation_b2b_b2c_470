namespace NopStation.Plugin.B2B.IQRetailIntegration
{
    public static class IQRetailIntegrationDefaults
    {
        public static int DefaultTimeOutPeriod => 1800;
        public static string IQRetailIntegrationDefaultBaseUrl => "https://iqretailapi.rna.co.za:7381/IQRetailRestAPI/v1/";
        public static string EApiFilterText => "EAPIFilter_TEXT";
        public static string PdfEmbedding => "pdf";
        public static string IQApiRequestGenericSQL => "IQ_API_Request_GenericSQL";
        public static string IQApiSubmitDebtor => "IQ_API_Submit_Debtor";
        public static string IQApiRequestDebtor => "IQ_API_Request_Debtor";
        public static string IQApiRequestDocumentInvoice => "IQ_API_Request_Document_Invoice";
        public static string IQApiRequestStock => "IQ_API_Request_Stock";
        public static string IQApiSubmitDocumentSalesOrder => "IQ_API_Submit_Document_Sales_Order";
        public static string IQApiRequestDocumentSalesOrder => "IQ_API_Request_Document_Sales_Order";
        public static string IQApiSubmitDocumentQuote => "IQ_API_Submit_Document_Quote";
        public static string IQApiRequestDocumentQuote => "IQ_API_Request_Document_Quote";
        public static int AccountNoLengthLimit => 14;        
        public static string ExportClassSalesOrder => "Sales_Order";
        public static string ExportClassQuote => "Quote";
    }
}
