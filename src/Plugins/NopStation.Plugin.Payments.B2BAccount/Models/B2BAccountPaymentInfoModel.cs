using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.B2BAccount.Models;

public record B2BAccountPaymentInfoModel : BaseNopModel
{
    public decimal CurrentBalance { get; set; }
    public string CurrentBalanceStr { get; set; }
    public string AllowOverspend { get; set; }
    public string PaymentStatus { get; set; }
    public decimal CreditLimit { get; set; } 
    public string CreditLimitStr { get; set; } 
    public decimal CreditLimitAvailable { get; set; } 
    public string CreditLimitAvailableStr { get; set; }
    public string CustomerReferenceAsPO { get; set; }
    public int ErpAccountId { get; set; }
}
