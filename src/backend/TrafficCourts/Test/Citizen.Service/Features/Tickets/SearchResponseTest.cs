﻿using System;
using TrafficCourts.Citizen.Service.Models.Tickets;
using Xunit;
using SearchResponse = TrafficCourts.Citizen.Service.Features.Tickets.Search.Response;

namespace TrafficCourts.Test.Citizen.Service.Features.Tickets
{
    public class SearchResponseTest
    {
        [Fact]
        public void constructor_throws_ArgumentNullException_when_passed_null()
        {
            Exception? exception = null;
            ViolationTicket? violationTicket = null;

            Assert.Throws<ArgumentNullException>("ticket", () => new SearchResponse(violationTicket!));
            Assert.Throws<ArgumentNullException>("exception", () => new SearchResponse(exception!));
        }

        [Fact]
        public void can_create_with_ViolationTicket()
        {
            ViolationTicket violationTicket = new ();

            var actual = new SearchResponse(violationTicket);

            Assert.True(actual.Result.IsT0);
            Assert.False(actual.Result.IsT1);
            Assert.Equal(violationTicket, actual.Result.Value);
        }

        [Fact]
        public void can_create_with_Exception()
        {
            Exception exception = new();

            var actual = new SearchResponse(exception);

            Assert.True(actual.Result.IsT1);
            Assert.False(actual.Result.IsT0);
            Assert.Equal(exception, actual.Result.Value);
        }

        [Fact]
        public void empty_response()
        {
            var actual = SearchResponse.Empty;

            Assert.NotNull(actual);
            // Result is a value type
            Assert.True(actual.Result.IsT0);
            Assert.False(actual.Result.IsT1);
            Assert.Null(actual.Result.Value);
        }

    }
}
