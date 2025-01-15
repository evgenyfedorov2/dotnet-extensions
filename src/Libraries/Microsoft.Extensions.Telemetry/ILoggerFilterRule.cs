﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Shared.DiagnosticIds;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// Represents a rule used for filtering log messages for purposes of log sampling and buffering.
/// </summary>
[Experimental(diagnosticId: DiagnosticIds.Experiments.Telemetry, UrlFormat = DiagnosticIds.UrlFormat)]
public interface ILoggerFilterRule
{
    /// <summary>
    /// Gets the logger category this rule applies to.
    /// </summary>
    string? Category { get; }

    /// <summary>
    /// Gets the maximum <see cref="LogLevel"/> of messages.
    /// </summary>
    LogLevel? LogLevel { get; }

    /// <summary>
    /// Gets the <see cref="EventId"/> of messages where this rule applies to.
    /// </summary>
    int? EventId { get; }

    /// <summary>
    /// Gets the filter delegate that would be additionally applied to messages that passed the <see cref="LogLevel"/>, <see cref="Category"/>, and <see cref="EventId"/> filters.
    /// </summary>
    Func<string?, LogLevel?, EventId?, IReadOnlyList<KeyValuePair<string, object?>>, bool> Filter { get; }
}
