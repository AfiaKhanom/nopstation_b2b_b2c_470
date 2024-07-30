using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage
{
    /// <summary>
    /// Erp workflow message service
    /// </summary>
    public partial class ErpWorkflowMessageService : IErpWorkflowMessageService
    {
        #region Fields

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ErpWorkflowMessageService(
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            ICustomerService customerService)
        {
            _emailAccountSettings = emailAccountSettings;
            _emailAccountService = emailAccountService;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get active message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>List of message templates</returns>
        protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>EmailAccount</returns>
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

        /// <summary>
        /// Ensure language is active
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Return a value language identifier</returns>
        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);

            if (string.IsNullOrEmpty(subject))
                subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);

            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
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
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            await _queuedEmailService.InsertQueuedEmailAsync(email);

            return email.Id;
        }

        #endregion

        #region Methods

        #region Order workflow

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public async Task<int> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification, store.Id)).FirstOrDefault();

            if (messageTemplate is null)
                return 0;

            //tokens
            var commonTokens = new List<Token>();
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = erpShipToAddress.RepEmail;
            var toName = erpShipToAddress.RepFullName;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        #endregion

        #region ERP Customer Registration Application
        public async Task<int> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
        {
            if (applicationForm is null)
                throw new ArgumentNullException(nameof(applicationForm));
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var messageTemplateToAdmin = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin, store.Id)).FirstOrDefault();
            var messageTemplateToCustomer = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer, store.Id)).FirstOrDefault();
            //tokens
            var commonTokens = new List<Token>();
            await AddApplicationFormTokensAsync(commonTokens, applicationForm);
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplateToAdmin, languageId);
            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
            if (messageTemplateToAdmin is not null)
            {
                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplateToAdmin, tokens);
                var toEmail = emailAccount.Email;
                var toName = !string.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.DisplayName : "Admin";
                await SendNotificationAsync(messageTemplateToAdmin, emailAccount, languageId, tokens, toEmail, toName);
            }
            if (messageTemplateToCustomer is not null)
            {
                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplateToCustomer, tokens);
                var toEmail = applicationForm.AccountsEmail;
                var toName = applicationForm.FullRegisteredName;
                await SendNotificationAsync(messageTemplateToCustomer, emailAccount, languageId, tokens, toEmail, toName);
            }
            return 0;
        }
        public async Task<int> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
        {
            if (applicationForm is null)
                throw new ArgumentNullException(nameof(applicationForm));
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification, store.Id)).FirstOrDefault();
            //tokens
            var commonTokens = new List<Token>();
            await AddApplicationFormTokensAsync(commonTokens, applicationForm);
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
            if (messageTemplate is null)
            {
                return 0;
            }
            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
            var toEmail = applicationForm.AccountsEmail;
            var toName = applicationForm.FullRegisteredName;
            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }
        public async Task AddApplicationFormTokensAsync(List<Token> tokens, ErpAccountCustomerRegistrationForm applicationForm)
        {
            tokens.Add(new Token("Application.CustomerFullName", applicationForm.FullRegisteredName));
            tokens.Add(new Token("Application.AdminName", "Admin"));
            tokens.Add(new Token("Application.Id", applicationForm.Id));
            tokens.Add(new Token("Application.RegistrationNumber", applicationForm.RegistrationNumber));
        }
        #endregion

        #endregion

    }
}