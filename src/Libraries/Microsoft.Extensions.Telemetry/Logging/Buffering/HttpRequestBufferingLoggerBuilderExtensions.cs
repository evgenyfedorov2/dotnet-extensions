﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.Diagnostics.Logging.Buffering;

/// <summary>
/// Lets you register log buffers in a dependency injection container.
/// </summary>
public static class HttpRequestBufferingLoggerBuilderExtensions
{
    /// <summary>
    /// Adds HTTP request logging buffering.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the buffer to.</param>
    /// <param name="filter">The filter to be used to decide what to buffer.</param>
    /// <param name="options">Options for the buffering.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddHttpRequestBuffering(
        this ILoggingBuilder builder,
        Func<string?, EventId?, LogLevel?, bool> filter,
        Action<HttpRequestBufferingOptions>? options = null)
    {
        _ = Throw.IfNull(builder);

        return builder
            .AddHttpRequestBufferProvider()
            .ConfigureHttpRequestBuffering(filter, options);
    }

    /// <summary>
    /// Adds a log buffer to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the buffer to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddHttpRequestBufferProvider(this ILoggingBuilder builder)
    {
        _ = Throw.IfNull(builder);

        _ = builder.Services.AddActivatedSingleton<ILoggingBufferProvider, HttpRequestBufferProvider>();

        return builder;
    }

    /// <summary>
    /// Adds a log buffer to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the buffer to.</param>
    /// <param name="filter">The filter to be used to decide what to buffer.</param>
    /// <param name="configureOptions">The delegate to configure <see cref="HttpRequestBufferingOptions"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder ConfigureHttpRequestBuffering(
        this ILoggingBuilder builder,
        Func<string?, EventId?, LogLevel?, bool> filter,
        Action<HttpRequestBufferingOptions>? configureOptions = null)
    {
        _ = Throw.IfNull(builder);

        _ = builder.Services.Configure(configureOptions ?? new Action<HttpRequestBufferingOptions>((_) => { }));
        _ = builder.Services.Configure<HttpRequestBufferingOptions>(opts => opts.AddHttpRequestBufferingFilter(filter));
        _ = builder.Services
            .AddOptions<GlobalBufferingOptions>()
            .Configure<HttpRequestBufferingOptions>((globalOpts, requestOpts) =>
            {
                globalOpts.Capacity = requestOpts.GlobalCapacity;
                globalOpts.Filter = requestOpts.Filter;
                globalOpts.SuspendAfterFlushDuration = requestOpts.SuspendAfterFlushDuration;
            });

        return builder;
    }

    /// <summary>
    /// Adds a log buffer to the factory.
    /// </summary>
    public static void AddHttpRequestBufferingFilter(
        this HttpRequestBufferingOptions options,
        Func<string?, EventId?, LogLevel?, bool> filter)
    {
        _ = Throw.IfNull(options);
        _ = Throw.IfNull(filter);

        options.Filter = filter;
    }
}