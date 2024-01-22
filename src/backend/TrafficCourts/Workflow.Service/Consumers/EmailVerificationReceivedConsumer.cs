﻿using MassTransit;
using TrafficCourts.Common.Features.Mail;
using TrafficCourts.Common.Features.Mail.Templates;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;
using TrafficCourts.Messaging.MessageContracts;
using TrafficCourts.Workflow.Service.Services;
using ApiException = TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0.ApiException;
using DisputeUpdateRequest = TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0.DisputeUpdateRequest;

namespace TrafficCourts.Workflow.Service.Consumers;

/// <summary>
/// Consumer for a EmailReceivedVerification (produced when a Disputant confirms their email address).
/// This Consumer simply updates the Disputant record for the given email verification token, setting the EmailVerification flag to true.
/// </summary>
public partial class SetEmailVerifiedOnDisputeInDatabase : IConsumer<EmailVerificationSuccessful>
{
    private readonly ILogger<SetEmailVerifiedOnDisputeInDatabase> _logger;
    private readonly IOracleDataApiService _oracleDataApiService;
    private readonly IConfirmationEmailTemplate _confirmationEmailTemplate;
    private readonly IDisputantEmailUpdateSuccessfulTemplate _emailUpdateSuccessfulTemplate;

    public SetEmailVerifiedOnDisputeInDatabase(IOracleDataApiService oracleDataApiService, IConfirmationEmailTemplate confirmationEmailTemplate, IDisputantEmailUpdateSuccessfulTemplate updateRequestReceivedTemplate, ILogger<SetEmailVerifiedOnDisputeInDatabase> logger)
    {
        _oracleDataApiService = oracleDataApiService ?? throw new ArgumentNullException(nameof(oracleDataApiService));
        _confirmationEmailTemplate = confirmationEmailTemplate ?? throw new ArgumentNullException(nameof(confirmationEmailTemplate));
        _emailUpdateSuccessfulTemplate = updateRequestReceivedTemplate ?? throw new ArgumentNullException(nameof(updateRequestReceivedTemplate));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<EmailVerificationSuccessful> context)
    {
        var message = context.Message;
        try
        {
            Dispute? dispute = await _oracleDataApiService.GetDisputeByNoticeOfDisputeGuidAsync(message.NoticeOfDisputeGuid, context.CancellationToken);
            if (dispute is null)
            {
                // TODO: how do we handle if the dispute has not been created in the database yet? 
                LogDisputeNotFound(context);
                return;
            }

            // message could be send again, if the database already says it is verified, then we are done.
            if (dispute.EmailAddressVerified)
            {
                LogEmailAddressAlreadyVerified(context);
                return;
            }

            await _oracleDataApiService.VerifyDisputeEmailAsync(dispute.DisputeId, context.CancellationToken);

            // TCVP-2009
            // Send acknowledgement email if there are pending update requests
            // Moved this up in the order of operations to avoid multiple sending of emails if this fails 
            ICollection<DisputeUpdateRequest> disputeUpdateRequests = await _oracleDataApiService.GetDisputeUpdateRequestsAsync(dispute.DisputeId, Status.PENDING, context.CancellationToken);
            if (disputeUpdateRequests.Count > 0)
            {
                UpdateRequestReceived updateRequestReceived = new()
                {
                    NoticeOfDisputeGuid = message.NoticeOfDisputeGuid
                };
                await context.PublishWithLog(_logger, updateRequestReceived, context.CancellationToken);
            }

            // File History 
            SaveFileHistoryRecord fileHistoryRecord = new SaveFileHistoryRecord();
            fileHistoryRecord.DisputeId = dispute.DisputeId;
            fileHistoryRecord.AuditLogEntryType = !message.IsUpdateEmailVerification ? 
                FileHistoryAuditLogEntryType.EMVF : // Email verification complete
                FileHistoryAuditLogEntryType.CUEV; // Email re-verification complete
            fileHistoryRecord.ActionByApplicationUser = "Disputant";
            await context.PublishWithLog(_logger, fileHistoryRecord, context.CancellationToken);

            if (!message.IsUpdateEmailVerification)
            {
                // File History 
                fileHistoryRecord.AuditLogEntryType = FileHistoryAuditLogEntryType.SUB; // Dispute submitted for staff review
                await context.PublishWithLog(_logger, fileHistoryRecord, context.CancellationToken);
            }

            // since the dispute is re-retrieved in this consumer, the email address may have been blanked out in the meantime
            // if it was dont send another email :)
            if (!string.IsNullOrEmpty(dispute.EmailAddress)) {
                // TCVP-1529 Send NoticeOfDisputeConfirmationEmail *after* validating Disputant's email
                EmailMessage emailMessage = _confirmationEmailTemplate.Create(dispute);

                // Send email with email update successful content if this event is a result of email update process
                if (message.IsUpdateEmailVerification)
                {
                    emailMessage = _emailUpdateSuccessfulTemplate.Create(dispute);
                }
                await context.PublishWithLog(_logger, new SendDisputantEmail
                {
                    Message = emailMessage,
                    TicketNumber = dispute.TicketNumber,
                    NoticeOfDisputeGuid = message.NoticeOfDisputeGuid
                }, context.CancellationToken);
            }

        }
        catch (ApiException ex)
        {
            LogErrorProcessingVerification(ex, context);
            throw;
        }
        catch (Exception ex)
        {
            LogErrorProcessingVerification(ex, context);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispute not found")]
    private partial void LogDisputeNotFound([TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)] ConsumeContext<EmailVerificationSuccessful> context);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email address already verified")]
    private partial void LogEmailAddressAlreadyVerified([TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)] ConsumeContext<EmailVerificationSuccessful> context);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error while processing email verfied message")]
    private partial void LogErrorProcessingVerification(Exception ex, [TagProvider(typeof(TagProvider), nameof(TagProvider.RecordTags), OmitReferenceName = true)] ConsumeContext<EmailVerificationSuccessful> context);
}


