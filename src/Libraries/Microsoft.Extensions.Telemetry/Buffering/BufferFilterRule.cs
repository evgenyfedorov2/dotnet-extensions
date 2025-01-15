﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Shared.DiagnosticIds;

namespace Microsoft.Extensions.Diagnostics.Buffering;

/// <summary>
/// Defines a rule used to filter log messages for purposes of futher buffering.
/// </summary>
[Experimental(diagnosticId: DiagnosticIds.Experiments.Telemetry, UrlFormat = DiagnosticIds.UrlFormat)]
public class BufferFilterRule : ILoggerFilterRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BufferFilterRule"/> class.
    /// </summary>
    public BufferFilterRule()
        : this(null, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferFilterRule"/> class.
    /// </summary>
    /// <param name="categoryName">The category name to use in this filter rule.</param>
    /// <param name="logLevel">The <see cref="LogLevel"/> to use in this filter rule.</param>
    /// <param name="eventId">The <see cref="EventId"/> to use in this filter rule.</param>
    /// <param name="filter">The optional filter delegate to use if a log message passes other filters.</param>
    public BufferFilterRule(string? categoryName, LogLevel? logLevel, int? eventId,
        Func<string?, LogLevel?, EventId?, IReadOnlyList<KeyValuePair<string, object?>>, bool>? filter = null)
    {
        Category = categoryName;
        LogLevel = logLevel;
        EventId = eventId;
        Filter = filter ?? ((_, _, _, _) => true);
    }

    /// <inheritdoc/>
    public string? Category { get; set; }

    /// <inheritdoc/>
    public LogLevel? LogLevel { get; set; }

    /// <inheritdoc/>
    public int? EventId { get; set; }

    /// <inheritdoc/>
    public Func<string?, LogLevel?, EventId?, IReadOnlyList<KeyValuePair<string, object?>>, bool> Filter { get; }
}
