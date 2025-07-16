using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;

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
    private readonly MessagesSettings _messagesSettings;
    private readonly IAddressService _addressService;
    private readonly IAffiliateService _affiliateService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpShipToAddressService _erpShipToAddressService;

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
        ICustomerService customerService,
        MessagesSettings messagesSettings,
        IAddressService addressService,
        IAffiliateService affiliateService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpShipToAddressService erpShipToAddressService)
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
        _messagesSettings = messagesSettings;
        _addressService = addressService;
        _affiliateService = affiliateService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpShipToAddressService = erpShipToAddressService;
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

    public virtual async Task<List<int>> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
        List<(string toEmailAddresses, string toName)> emailAddressesWithNames,
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

        var results = new List<int>();

        foreach (var emailAddresses in emailAddressesWithNames)
        {
            foreach (var emailAddress in emailAddresses.toEmailAddresses.Split(';').Distinct())
            { 
                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                    FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                    To = emailAddress,
                    ToName = CommonHelper.EnsureMaximumLength(emailAddresses.toName, 300),
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

                results.Add(email.Id);
            }
        }

        return results;
    }

    protected async Task<(string email, string name)> GetStoreOwnerNameAndEmailAsync(EmailAccount messageTemplateEmailAccount)
    {
        var storeOwnerEmailAccount = _messagesSettings.UseDefaultEmailAccountForSendStoreOwnerEmails ? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) : null;
        storeOwnerEmailAccount ??= messageTemplateEmailAccount;

        return (storeOwnerEmailAccount.Email, storeOwnerEmailAccount.DisplayName);
    }

    protected async Task<(string email, string name)> GetCustomerReplyToNameAndEmailAsync(MessageTemplate messageTemplate, Order order)
    {
        if (!messageTemplate.AllowDirectReply)
            return (null, null);

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        return (billingAddress.Email, $"{billingAddress.FirstName} {billingAddress.LastName}");
    }

    #endregion

    #region Methods

    #region Order workflow

    public async Task<IList<int>> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification, store.Id)).FirstOrDefault();

        if (messageTemplate is null)
            return[];

        var commonTokens = new List<Token>();
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

        var toEmail = erpShipToAddress.RepEmail;
        var toName = erpShipToAddress.RepFullName;

        return await SendNotificationAsync(
            messageTemplate,
            emailAccount,
            languageId,
            tokens,
            new List<(string toEmailAddresses, string toName)> { (toEmail, toName) }
            );
    }

    public async Task<IList<int>> SendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_STORE_OWNER_NOTIFICATION, store.Id);
        if (messageTemplates.Count == 0)
            return [];

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        foreach (var messageTemplate in messageTemplates)
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);
            var (replyToEmail, replyToName) = await GetCustomerReplyToNameAndEmailAsync(messageTemplate, order);

            results.AddRange(await SendNotificationAsync(
                messageTemplate, 
                emailAccount, 
                languageId, 
                tokens,
                new List<(string toEmailAddresses, string toName)> { (toEmail, toName) },
                replyToEmailAddress: replyToEmail, replyToName: replyToName));
        }

        return results;
    }

    public async Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(vendor);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_VENDOR_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return [];

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        var toEmail = vendor.Email;
        var toName = vendor.Name;

        foreach (var messageTemplate in messageTemplates)
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            results.AddRange(await SendNotificationAsync(
                messageTemplate,
                emailAccount,
                languageId,
                tokens,
                new List<(string toEmailAddresses, string toName)> { (toEmail, toName) }
                ));
        }

        return results;
    }

    public async Task<IList<int>> SendOrderPlacedAffiliateNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

        ArgumentNullException.ThrowIfNull(affiliate);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_AFFILIATE_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return [];

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        var affiliateAddress = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
        var toEmail = affiliateAddress.Email;
        var toName = $"{affiliateAddress.FirstName} {affiliateAddress.LastName}";

        foreach (var messageTemplate in messageTemplates)
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            results.AddRange(await SendNotificationAsync(
                messageTemplate,
                emailAccount,
                languageId,
                tokens,
                new List<(string toEmailAddresses, string toName)> { (toEmail, toName) }
                ));
        }

        return results;
    }

    public async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        #region Prepare multiple email addresses

        // 1. Customer email
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

        if (customer is null)
            return [];
        var emailList = new List<(string toEmailAddress, string toName)>
        {
            (customer.Email, $"{customer.FirstName} {customer.LastName}")
        };

        // 2. ShipToAddress emails
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (erpNopUser != null)
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);

            if (erpShipToAddress != null)
            {
                if (erpShipToAddress is not null && erpShipToAddress.EmailAddresses is not null)
                {
                    emailList.Add((erpShipToAddress.EmailAddresses.Trim(), $"{erpShipToAddress.ShipToName}"));
                }

                // 3. Sales Rep email
                if (!string.IsNullOrWhiteSpace(erpShipToAddress.RepEmail) &&
                    !emailList.Exists(x => x.toEmailAddress == erpShipToAddress.RepEmail.Trim()))
                {
                    emailList.Add((erpShipToAddress.RepEmail.Trim(), $"{erpShipToAddress.ShipToName}"));
                }
            }
        }

        // 4. Billing Address email
        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        if (billingAddress != null && !emailList.Exists(x => x.toEmailAddress == billingAddress.Email.Trim()))
            emailList.Add((billingAddress.Email.Trim(), $"{billingAddress.FirstName} {billingAddress.LastName}"));

        #endregion

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return [];

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        foreach (var messageTemplate in messageTemplates)
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            // Send notification for each email
            results.AddRange(await SendNotificationAsync(
                messageTemplate, 
                emailAccount, 
                languageId, 
                tokens,
                emailList,
                attachmentFilePath, 
                attachmentFileName));
        }

        return results;
    }

    #endregion

    #region Erp Customer Registration Application

    public async Task<IList<int>> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplateToAdmin = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin, store.Id)).FirstOrDefault();
        var messageTemplateToCustomer = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplateToAdmin, languageId);
        var tokens = new List<Token>(commonTokens);
        
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        
        if (messageTemplateToAdmin is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToAdmin, tokens);
            var toEmail = emailAccount.Email;
            var toName = !string.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.DisplayName : "Admin";
            await SendNotificationAsync(
                messageTemplateToAdmin, 
                emailAccount, 
                languageId, 
                tokens,
                new List<(string toEmailAddresses, string toName)> { (toEmail, toName) });
        }
        
        if (messageTemplateToCustomer is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToCustomer, tokens);
            var toEmail = applicationForm.AccountsEmail;
            var toName = applicationForm.FullRegisteredName;
            await SendNotificationAsync(
                messageTemplateToCustomer, 
                emailAccount, 
                languageId, 
                tokens,
                new List<(string toEmailAddresses, string toName)> { (toEmail, toName) });
        }

        return [0];
    }

    public async Task<IList<int>> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        
        if (messageTemplate is null)
        {
            return [0];
        }

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
        var toEmail = applicationForm.AccountsEmail;
        var toName = applicationForm.FullRegisteredName;
        
        return await SendNotificationAsync(
            messageTemplate, 
            emailAccount, 
            languageId, 
            tokens,
            new List<(string toEmailAddresses, string toName)> { (toEmail, toName) });
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