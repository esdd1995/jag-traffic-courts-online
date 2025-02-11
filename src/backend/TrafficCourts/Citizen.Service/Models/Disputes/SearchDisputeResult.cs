﻿using System.Text.Json.Serialization;
using TrafficCourts.Domain.Models;

namespace TrafficCourts.Citizen.Service.Models.Disputes
{
    public class SearchDisputeResult
    {
        [JsonPropertyName("token")]
        public string? NoticeOfDisputeGuid { get; set; }

        [JsonPropertyName("dispute_status")]
        public DisputeStatus? DisputeStatus { get; set; }

        [JsonPropertyName("jjdispute_status")]
        public JJDisputeStatus? JJDisputeStatus { get; set; }

        [JsonPropertyName("hearing_type")]
        public JJDisputeHearingType? HearingType { get; set; }
        
        [JsonPropertyName("is_email_verified")]
        public bool? IsEmailVerified { get; set; }

        [JsonPropertyName("request_court_appearance")]
        public DisputeRequestCourtAppearanceYn RequestCourtAppearanceYn { get; set; }

        [JsonPropertyName("is_error")]
        public bool IsError { get; set; }
    }
}
