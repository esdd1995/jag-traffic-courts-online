﻿using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Reflection;
using System.Text.Json.Serialization;
using TrafficCourts.Common.Converters;
using TrafficCourts.Messaging.Configuration;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Messaging;

public static class BusConfiguratorExtensions
{
    public const string MassTransitSection = "MassTransit";

    /// <summary>
    /// Adds MassTransit and its dependencies to the collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="configureBusRegistration">optional bus registration configration function</param>
    /// <param name="configureBusFactory">ptional bus registration configration function</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ConfigurationErrorsException"></exception>
    public static void AddMassTransit(this IServiceCollection services, 
        IConfiguration configuration, 
        Serilog.ILogger logger, 
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        Action<IBusFactoryConfigurator>? configureBusFactory = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        // determine the transport
        var transport = configuration.GetSection($"{MassTransitSection}:Transport").Get<MassTransitTransport>();

        
        if (transport == MassTransitTransport.RabbitMq)
        {
            logger.Information("Using MassTransit Transport: {Transport}", transport);
            services.AddMassTransit(config => UseRabbitMq(config, services, configuration, configureBusRegistration, configureBusFactory));
        }
        else if (transport == MassTransitTransport.InMemory)
        {
            logger.Information("Using MassTransit Transport: {Transport}", transport);
            services.AddMassTransit(config => UseInMemory(config, configuration, configureBusRegistration));
        }
        else
        {
            string message = $"Unknown MassTransit Transport: {transport}. Value values are {nameof(MassTransitTransport.RabbitMq)} and {nameof(MassTransitTransport.InMemory)}";

#pragma warning disable Serilog004 // Constant MessageTemplate verifier - fatal error that will terminate the process
            logger.Fatal(message);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier

            throw new ConfigurationErrorsException(message);
        }
    }

    private static void UseInMemory(IBusRegistrationConfigurator config, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureBusRegistration)
    {
        if (configureBusRegistration is not null)
        {
            configureBusRegistration(config); // add consumers
        }

        config.SetKebabCaseEndpointNameFormatter();

        config.UsingInMemory();
    }

    private static void UseRabbitMq(
        IBusRegistrationConfigurator config, 
        IServiceCollection services, 
        IConfiguration configuration, 
        Action<IBusRegistrationConfigurator>? configureBusRegistration,
        Action<IBusFactoryConfigurator>? configureBusFactory)
    {
        services.ConfigureValidatableSetting<RabbitMqHostOptions>(configuration.GetSection(RabbitMqHostOptions.Section));

        if (configureBusRegistration is not null)
        {
            configureBusRegistration(config); // add consumers
        }

        config.SetKebabCaseEndpointNameFormatter();

        config.UsingRabbitMq((context, configure) => 
        {
            var options = context.GetRequiredService<RabbitMqHostOptions>();
            string connectionName = GetConnectionName(options);

            configure.Host(options.Host, options.Port, options.VirtualHost, connectionName, host =>
            {
                host.Username(options.Username);
                host.Password(options.Password);
            });

            RegisterEndpointConventions(options);

            configure.UseConcurrencyLimit(options.Retry.ConcurrencyLimit);
            configure.UseMessageRetry(r =>
            {
                r.Ignore<ArgumentNullException>();
                r.Ignore<InvalidOperationException>();
                r.Interval(options.Retry.Times, TimeSpan.FromMinutes(options.Retry.Interval));
            });

            configure.ConfigureEndpoints(context);

            configure.ConfigureJsonSerializerOptions(settings =>
            {
                settings.Converters.Add(new DateOnlyJsonConverter());
                settings.Converters.Add(new TimeOnlyJsonConverter());
                settings.Converters.Add(new JsonStringEnumConverter());
                return settings;
            });

            // should we call the user's configuration at the start or end?
            if (configureBusFactory is not null)
            {
                configureBusFactory(configure);
            }
        });
    }

    private static void RegisterEndpointConventions(RabbitMqHostOptions options)
    {
        var messageTypes = typeof(IMessage)
            .Assembly
            .GetTypes()
            .Where(type => typeof(IMessage).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        MethodInfo method = typeof(EndpointConvention).GetMethod(nameof(EndpointConvention.Map), BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(Uri)})!;

        foreach (var messageType in messageTypes)
        {
            var attribute = messageType.GetCustomAttribute<EndpointConventionAttribute>();
            if (attribute is null)
            {
                // no endpoint convention
                continue;
            }

            // get the EndpointConvention.Map<messageType> method
            MethodInfo typedMethod = method.MakeGenericMethod(messageType);
            // call the static method
            typedMethod.Invoke(null, new object[] { new Uri($"amqp://{options.Host}:{options.Port}/{attribute.Name}") });
        }
    }

    /// <summary>
    /// Gets the connection name that will be displayed in the RabbitMq management console
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string GetConnectionName(RabbitMqHostOptions options)
    {
        string? connectionName = options.ClientProvidedName;
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            connectionName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
        }

        connectionName += " (" + Environment.MachineName + ")";
        return connectionName;
    }
}
