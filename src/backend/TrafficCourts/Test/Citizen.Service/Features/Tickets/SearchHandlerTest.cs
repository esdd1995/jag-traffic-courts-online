﻿using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrafficCourts.Citizen.Service.Features.Tickets.Search;
using TrafficCourts.Citizen.Service.Models.Tickets;
using TrafficCourts.Citizen.Service.Services;
using TrafficCourts.Citizen.Service.Services.Tickets.Search;
using TrafficCourts.Citizen.Service.Services.Tickets.Search.Common;
using Xunit;
using Xunit.Abstractions;

namespace TrafficCourts.Test.Citizen.Service.Features.Tickets
{
    public class SearchHandlerTest
    {
        private readonly Mock<ITicketSearchService> _serviceMock = new Mock<ITicketSearchService>();
        private readonly Mock<ILogger<Handler>> _loggerMock = new Mock<ILogger<Handler>>();
        private readonly Mock<IRedisCacheService> _redisCacheServiceMock = new Mock<IRedisCacheService>();
        private readonly ITestOutputHelper _output;

        public SearchHandlerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void constructor_throws_ArgumentNullException_when_passed_null()
        {
            Assert.Throws<ArgumentNullException>("service", () => new Handler(null!, _loggerMock.Object, _redisCacheServiceMock.Object));
            Assert.Throws<ArgumentNullException>("logger", () => new Handler(_serviceMock.Object, null!, _redisCacheServiceMock.Object));
            Assert.Throws<ArgumentNullException>("redisCacheService", () => new Handler(_serviceMock.Object, _loggerMock.Object, null!));
        }


        [Fact]
        public async Task search_service_returns_null_empty_response_is_returned()
        {
            var expected = TrafficCourts.Citizen.Service.Features.Tickets.Search.Response.Empty;
            ViolationTicket? violationTicket = null;

            _serviceMock
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(violationTicket);

            var request = new Request("AA00000000", "00:00");

            var sut = new Handler(_serviceMock.Object, _loggerMock.Object, _redisCacheServiceMock.Object);

            var actual = await sut.Handle(request, CancellationToken.None);

            Assert.NotNull(actual);
            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task search_service_returns_ViolationTicket_response_contains_same_value()
        {
            ViolationTicket? violationTicket = new();

            _serviceMock
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(violationTicket);

            var request = new Request("AA00000000", "00:00");

            var sut = new Handler(_serviceMock.Object, _loggerMock.Object, _redisCacheServiceMock.Object);

            var actual = await sut.Handle(request, CancellationToken.None);

            Assert.NotNull(actual);
            Assert.Same(violationTicket, actual.Result.Value);
        }


        [Fact]
        public async Task search_throws_exception()
        {
            Exception exception = new();

            _serviceMock
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var request = new Request("AA00000000", "00:00");

            var sut = new Handler(_serviceMock.Object, _loggerMock.Object, _redisCacheServiceMock.Object);

            var actual = await sut.Handle(request, CancellationToken.None);

            Assert.NotNull(actual);
            Assert.Same(exception, actual.Result.Value);
        }

        [Fact]
        public async Task search_throws_InvalidTicketVersionException()
        {
            DateTime violationDate = new(2023, 4, 9);
            InvalidTicketVersionException exception = new(violationDate);

            _serviceMock
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var request = new Request("SA00000000", "00:00");

            var sut = new Handler(_serviceMock.Object, _loggerMock.Object, _redisCacheServiceMock.Object);

            var actual = await sut.Handle(request, CancellationToken.None);

            Assert.NotNull(actual);
            Assert.Same(exception, actual.Result.Value);
        }

        [Fact]
        public async Task search_filters_non_MVA_MVAR_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Trespassing",
                ViolationDate = "2023-04-09",
                Act = "DTM",
                Section = "45(a)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            // Invoice.Act does not match MVA or MVAR and so results should be null
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.Null(actual);
        }


        [Fact]
        public async Task search_includes_MVA_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVA",
                Section = "45(a)",
                FormNumber = "MV6000E (040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            // Invoice.Act should match MVA
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.NotNull(actual);
            Assert.Equal("EA00000000", actual.TicketNumber);
        }


        [Fact]
        public async Task search_includes_MVAR_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVAR",
                Section = "45(a)",
                FormNumber = "MV6000E (040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            // Invoice.Act should match MVAR 
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.NotNull(actual);
            Assert.Equal("EA00000000", actual.TicketNumber);
        }

        // test that asserts that the ticketSearchService includes Invoices with a FormNumber == "MV6000E (040924)"
        [Fact]
        public async Task search_includes_FormNumber_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVAR",
                Section = "45(a)",
                FormNumber = "MV6000E (040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.NotNull(actual);
            Assert.Equal("EA00000000", actual.TicketNumber);
        }

        // test that asserts that the ticketSearchService includes Invoices with a FormNumber == "MV6000E(040924)"
        [Fact]
        public async Task search_includes_FormNumber_tickets_wo_spaces()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVAR",
                Section = "45(a)",
                FormNumber = "MV6000E(040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.NotNull(actual);
            Assert.Equal("EA00000000", actual.TicketNumber);
        }

        // test that asserts that the ticketSearchService excludes Invoices with a FormNumber != "MV6000E (040924)"
        [Fact]
        public async Task search_excludes_FormNumber_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVAR",
                Section = "45(a)",
                FormNumber = "MV7000E (040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.Null(actual);
        }

        // test that asserts that the ticketSearchService excludes Invoices with a FormNumber != "MV6000E(040924)"
        [Fact]
        public async Task search_excludes_FormNumber_wo_spaces_tickets()
        {
            // Given
            string ticketNumber = "EA00000000";
            string ticketTime = "00:00";
            Mock<IBus> _bus = new();
            Mock<ITicketInvoiceSearchService> _invoiceSearchService = new();
            Mock<ILogger<TicketSearchService>> _logger = new();
            TicketSearchService ticketSearchService = new(_bus.Object, _invoiceSearchService.Object, _logger.Object);
            Invoice invoice = new()
            {
                InvoiceNumber = "EA000000001",
                PbcRefNumber = "n/a",
                PartyNumber = "n/a",
                PartyName = "n/a",
                AccountNumber = "n/a",
                SiteNumber = "n/a",
                InvoiceType = "Traffic Violation Ticket",
                ViolationDateTime = "2023-04-09T" + ticketTime,
                TicketedAmount = 100.00M,
                AmountDue = 100.00M,
                OffenceDescription = "Speed against area sign",
                ViolationDate = "2023-04-09",
                Act = "MVAR",
                Section = "45(a)",
                FormNumber = "MV7000E(040924)"
            };
            IList<Invoice> invoices = [invoice];

            // When
            _invoiceSearchService
                .Setup(_ => _.SearchAsync(It.IsAny<string>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(invoices));

            // Then
            var actual = await ticketSearchService.SearchAsync(ticketNumber, TimeOnly.MinValue, CancellationToken.None);
            Assert.Null(actual);
        }
    }
}
