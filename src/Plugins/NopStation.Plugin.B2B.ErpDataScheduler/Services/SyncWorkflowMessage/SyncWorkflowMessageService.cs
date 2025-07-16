using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;

public partial class SyncWorkflowMessageService : ISyncWorkflowMessageService
{
    #region Fields

    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IEmailAccountService _emailAccountService;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IStoreContext _storeContext;
    private readonly ITokenizer _tokenizer;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public SyncWorkflowMessageService(
        EmailAccountSettings emailAccountSettings,
        IEmailAccountService emailAccountService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IQueuedEmailService queuedEmailService,
        IStoreContext storeContext,
        ITokenizer tokenizer,
        ErpDataSchedulerSettings erpDataSchedulerSettings,
        IWorkContext workContext)
    {
        _emailAccountSettings = emailAccountSettings;
        _emailAccountService = emailAccountService;
        _languageService = languageService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _queuedEmailService = queuedEmailService;
        _storeContext = storeContext;
        _tokenizer = tokenizer;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId))
            ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        return emailAccount;
    }

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
        }

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
        string toEmailAddress, string toName,
        string attachmentFilePath = null, string attachmentFileName = null,
        string replyToEmailAddress = null, string replyToName = null,
        string fromEmail = null, string fromName = null, string subject = null)
    {
        ArgumentNullException.ThrowIfNull(messageTemplate);

        ArgumentNullException.ThrowIfNull(emailAccount);

        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);

        if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);

        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        toName = CommonHelper.EnsureMaximumLength(toName, 300);

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
            FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
            To = toEmailAddress,
            ToName = toName,
            ReplyTo = replyToEmailAddress,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachmentFilePath = attachmentFilePath,
            AttachmentFileName = attachmentFileName,
            AttachedDownloadId = messageTemplate.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                : (DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);

        return email.Id;
    }

    #endregion

    #region Methods

    public async Task SendSyncFailNotificationAsync(DateTime dateTime, string syncTaskName, string message = "")
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        int languageId = (await _workContext.GetWorkingLanguageAsync())?.Id ?? 0;
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = (await GetActiveMessageTemplatesAsync(ErpDataSchedulerDefaults.SyncFailedNotificationMessageTemplate, store.Id))
            .FirstOrDefault() ?? throw new ArgumentException("Message template not found.");

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        if (_erpDataSchedulerSettings.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError && emailAccount != null)
        {
            var emailList = new List<(string toEmailAddress, string toName)>
            {
                (emailAccount.Email, $"{emailAccount.Username}")
            };

            if (!string.IsNullOrWhiteSpace(_erpDataSchedulerSettings.AdditionalEmailAddresses))
            {
                var emails = _erpDataSchedulerSettings.AdditionalEmailAddresses.Split(';');
                foreach (var email in emails)
                {
                    if (!emailList.Exists(x => x.toEmailAddress == email.Trim()))
                        emailList.Add((email.Trim(), email.Trim()));
                }
            }

            var tokens = new List<Token>()
            {
                new("SyncNotification.Datetime", dateTime),
                new("SyncNotification.Message", message),
                new("SyncNotification.SyncTaskName", syncTaskName)
            };

            foreach (var email in emailList)
            {
                await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, email.toEmailAddress, email.toName);
            }
        }

        return;
    }

    #endregion
}