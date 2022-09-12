﻿using MassTransit;
using TrafficCourts.Common.Features.FilePersistence;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;
using TrafficCourts.Messaging.MessageContracts;
using TrafficCourts.Staff.Service.Configuration;
using TrafficCourts.Staff.Service.Mappers;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;

namespace TrafficCourts.Staff.Service.Services;

/// <summary>
/// Summary description for Class1
/// </summary>
public class DisputeService : IDisputeService
{
    private readonly OracleDataApiConfiguration _oracleDataApiConfiguration;
    private readonly ILogger<DisputeService> _logger;
    private readonly IBus _bus;
    private readonly IFilePersistenceService _filePersistenceService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DisputeService(
        OracleDataApiConfiguration oracleDataApiConfiguration,
        IBus bus,
        IFilePersistenceService filePersistenceService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DisputeService> logger)
    {
        _oracleDataApiConfiguration = oracleDataApiConfiguration ?? throw new ArgumentNullException(nameof(oracleDataApiConfiguration));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _filePersistenceService = filePersistenceService ?? throw new ArgumentNullException(nameof(filePersistenceService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns a new inialized instance of the OracleDataApi_v1_0Client
    /// </summary>
    /// <returns></returns>
    private OracleDataApiClient GetOracleDataApi()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(_oracleDataApiConfiguration.BaseUrl) };
        OracleDataApiClient client = new(httpClient);

        var user = _httpContextAccessor.HttpContext?.User;

        var username = user?.Claims?.FirstOrDefault(_ => _.Type == "preferred_username")?.Value;
        var fullName = user?.Claims?.FirstOrDefault(_ => _.Type == ClaimTypes.Name)?.Value;

        if (username is not null && !string.IsNullOrWhiteSpace(username))
        {
            // we expect the username to be of the form: someone@domain
            int index = username.IndexOf("@");
            if (index > 0)
            {
                username = username[..index];
            }

            HttpRequestHeaders requestHeaders = httpClient.DefaultRequestHeaders;
            requestHeaders.Add("x-username", username);

            if (fullName is not null && !string.IsNullOrWhiteSpace(fullName))
            {
                requestHeaders.Add("x-fullName", fullName);
            }
        }
        else
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                // this is being executed outside of an web request
                _logger.LogError("Cannot set x-username header, no HttpContext is available, the request not executing part of a HTTP web api request");
            }
            else
            {                
                using var scope = _logger.BeginScope(new Dictionary<string, object> {
                    ["IsAuthenticated"] = _httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? false,
                    ["AuthenticationType"] = _httpContextAccessor.HttpContext.User.Identity?.AuthenticationType ?? String.Empty
                });

                _logger.LogError("Could not find preferred_username claim on current user");
            }
        }

        return client;
    }

    public async Task<ICollection<Dispute>> GetAllDisputesAsync(ExcludeStatus? excludeStatus, CancellationToken cancellationToken)
    {
        return await GetOracleDataApi().GetAllDisputesAsync(null, excludeStatus, cancellationToken);
    }

    public async Task<long> SaveDisputeAsync(Dispute dispute, CancellationToken cancellationToken)
    {
        return await GetOracleDataApi().SaveDisputeAsync(dispute, cancellationToken);
    }

    public async Task<Dispute> GetDisputeAsync(long disputeId, CancellationToken cancellationToken)
    {
        Dispute dispute = await GetOracleDataApi().GetDisputeAsync(disputeId, cancellationToken);

        // If OcrViolationTicket != null, then this Violation Ticket was scanned using the Azure OCR Form Recognizer at one point.
        // If so, retrieve the image from object storage and return it as well.

        // Get the object store reference of the image (iff this is a scanned ViolationTicket)
        string? imageFilename = GetViolationTicketImageFilename(dispute);
        dispute.ViolationTicket.ViolationTicketImage = await GetViolationTicketImageAsync(imageFilename, cancellationToken);
        
        // deserialize json string to violation ticket fields
        if (dispute.OcrViolationTicket != null) dispute.ViolationTicket.OcrViolationTicket = System.Text.Json.JsonSerializer.Deserialize<OcrViolationTicket>(dispute.OcrViolationTicket);

        return dispute;
    }

    /// <summary>
    /// Extracts the path to the image located in the object store from the dispute record, or null if not filename could be found.
    /// </summary>
    /// <param name="dispute"></param>
    /// <returns></returns>
    public string? GetViolationTicketImageFilename(Dispute dispute)
    {
        if (dispute.OcrViolationTicket is not null)
        {
            try
            {
                JsonElement element = JsonSerializer.Deserialize<JsonElement>(dispute.OcrViolationTicket);
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("ImageFilename", out JsonElement imageFilename))
                {
                    return imageFilename.GetString();
                }
            }
            catch (Exception ex)
            {
                // Should never reach here, but if so then it means the ocr json data is invalid or not parseable by .NET
                // For now, just log the error and return null to mean no image could be found so the GetDispute(id) endpoint doesn't break.
                _logger.LogError(ex, "Could not extract object store file reference from json data");
            }
        }
        return null;
    }

    /// <summary>
    /// Retrieves a image from the object store with the given imageFilename.
    /// </summary>
    /// <param name="imageFilename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<ViolationTicketImage?> GetViolationTicketImageAsync(string? imageFilename, CancellationToken cancellationToken)
    {
        if (String.IsNullOrEmpty(imageFilename))
        {
            return null;
        }

        using (_logger.BeginScope(new Dictionary<string, object> { ["FileName"] = imageFilename }))
        {
            try
            {
                MemoryStream stream = await _filePersistenceService.GetFileAsync(imageFilename, cancellationToken);
                FileMimeType? mimeType = stream.GetMimeType();
                if (mimeType is null)
                {
                    _logger.LogWarning("Could not determine mime type for file");
                    return null;
                }

                return new ViolationTicketImage(stream.ToArray(), mimeType.MimeType);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not retrieve image from object storage");
                throw;
            }
        }
    }

    public async Task<Dispute> UpdateDisputeAsync(long disputeId, Dispute dispute, CancellationToken cancellationToken)
    {
        return await GetOracleDataApi().UpdateDisputeAsync(disputeId, dispute, cancellationToken);
    }

    public async Task ValidateDisputeAsync(long disputeId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Dispute status setting to validated");

        await GetOracleDataApi().ValidateDisputeAsync(disputeId, cancellationToken);
    }

    public async Task CancelDisputeAsync(long disputeId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Dispute cancelled");

        Dispute dispute = await GetOracleDataApi().CancelDisputeAsync(disputeId, cancellationToken);

        // Publish submit event (consumer(s) will generate email, etc)
        DisputeCancelled cancelledEvent = Mapper.ToDisputeCancelled(dispute);
        await _bus.Publish(cancelledEvent, cancellationToken);

        SendEmail cancelSendEmail = Mapper.ToCancelSendEmail(dispute);
        await _bus.Publish(cancelSendEmail, cancellationToken);
    }

    public async Task RejectDisputeAsync(long disputeId, string rejectedReason, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Dispute rejected");

        Dispute dispute = await GetOracleDataApi().RejectDisputeAsync(disputeId, rejectedReason, cancellationToken);

        // Publish submit event (consumer(s) will generate email, etc)
        DisputeRejected rejectedEvent = Mapper.ToDisputeRejected(dispute);
        await _bus.Publish(rejectedEvent, cancellationToken);

        SendEmail rejectSendEmail = Mapper.ToRejectSendEmail(dispute);
        await _bus.Publish(rejectSendEmail, cancellationToken);
    }

    public async Task SubmitDisputeAsync(long disputeId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Dispute submitted for approval processing");

        // Save and status to PROCESSING
        Dispute dispute = await GetOracleDataApi().SubmitDisputeAsync(disputeId, cancellationToken);

        // Publish submit event (consumer(s) will push event to ARC and generate email)
        DisputeApproved approvedEvent = Mapper.ToDisputeApproved(dispute);
        await _bus.Publish(approvedEvent, cancellationToken);
    }

    public async Task DeleteDisputeAsync(long disputeId, CancellationToken cancellationToken)
    {
        await GetOracleDataApi().DeleteDisputeAsync(disputeId, cancellationToken);
    }
}
