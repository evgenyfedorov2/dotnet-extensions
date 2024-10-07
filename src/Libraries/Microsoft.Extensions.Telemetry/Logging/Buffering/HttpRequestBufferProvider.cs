﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.Extensions.Diagnostics.Logging.Buffering;

internal class HttpRequestBufferProvider : ILoggingBufferProvider
{
    private readonly GlobalBuffer _globalBuffer;
    private readonly IHttpContextAccessor _accessor;
    private readonly ConcurrentDictionary<HttpContext, HttpRequestBuffer> _requestBuffers = new();

    public HttpRequestBufferProvider(GlobalBuffer globalBuffer, IHttpContextAccessor accessor)
    {
        _globalBuffer = globalBuffer;
        _accessor = accessor;
    }

    public ILoggingBuffer CurrentBuffer
    {
        get
        {
            if (_accessor.HttpContext != null)
            {
                // TODO: access/create the buffer for the current request
                _requestBuffers.GetOrAdd(_accessor.HttpContext, _accessor.HttpContext.RequestServices);
            }

            return _globalBuffer;
        }
    }
}
