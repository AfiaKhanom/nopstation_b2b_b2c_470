namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public class ErpAccountInfoAjaxLoadModel
    {
        public int ErpAccountId { get; set; }
        public string CreditLimit { get; set; }
        public string CurrentBalance { get; set; }
        public string AvailableCredit { get; set; }
        public string LastPaymentAmount { get; set; }
        public string LastPaymentDate { get; set; }
        public string CurrentOrderTotal { get; set; }
        public string CurrentBalanceWithCurrentOrderTotal { get; set; }
        public string CreditWarningMessage { get; set; }
        public bool AllowOverSpend { get; set; }
        public bool IsOverSpend { get; set; }
        public bool HasErpQuoteAssistantRole { get; set; }
        public bool HasErpOrderAssistantRole { get; set; }
    }
}
