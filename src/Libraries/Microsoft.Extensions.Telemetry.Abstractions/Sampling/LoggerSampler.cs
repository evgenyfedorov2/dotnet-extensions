﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Shared.DiagnosticIds;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// Controls the number of samples of log records collected and sent to the backend.
/// </summary>
#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods
[Experimental(diagnosticId: DiagnosticIds.Experiments.Telemetry, UrlFormat = DiagnosticIds.UrlFormat)]
public abstract class LoggerSampler
#pragma warning restore S1694 // An abstract class should have both abstract and concrete methods
{
    /// <summary>
    /// Makes a sampling decision based on the provided <paramref name="parameters"/>.
    /// </summary>
    /// <param name="parameters">The parameters used to make the sampling decision.</param>
    /// <returns><see langword="true" /> if the log record should be sampled; otherwise, <see langword="false" />.</returns>
    public abstract bool ShouldSample(SamplingParameters parameters);
}