﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET9_0_OR_GREATER
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Diagnostics.Logging;

/// <summary>
/// The options for LoggerBuffer.
/// </summary>
public class HttpRequestBufferOptions
{
    /// <summary>
    /// Gets or sets the time to suspend the buffer after flushing.
    /// </summary>
    /// <remarks>
    /// Use this to temporarily suspend buffering after a flush, e.g. in case of an incident you may want all logs to be emitted immediately,
    /// so the buffering will be suspended for the <see paramref="SuspendAfterFlushDuration"/> time.
    /// </remarks>
    public TimeSpan SuspendAfterFlushDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the size of the buffer for a request.
    /// </summary>
    public int PerRequestCapacity { get; set; } = 1_000;

    /// <summary>
    /// Gets or sets the size of the global buffer which applies to non-request logs only.
    /// </summary>
    public int GlobalCapacity { get; set; } = 1_000_000;

#pragma warning disable CA1002 // Do not expose generic lists - List is necessary to be able to call .AddRange()
#pragma warning disable CA2227 // Collection properties should be read only - setter is necessary for options pattern
    /// <summary>
    /// Gets or sets the collection of <see cref="BufferFilterRule"/> used for filtering log messages for the purpose of further buffering.
    /// </summary>
    public List<BufferFilterRule> Rules { get; set; } = [];
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists
}
#endif