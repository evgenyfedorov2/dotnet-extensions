﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Shared.Diagnostics;
using Microsoft.Shared.Pools;
using static Microsoft.Extensions.Logging.ExtendedLogger;

namespace Microsoft.AspNetCore.Diagnostics.Buffering;

internal sealed class HttpRequestBuffer : ILoggingBuffer
{
    private readonly IOptionsMonitor<HttpRequestBufferOptions> _options;
    private readonly IOptionsMonitor<GlobalBufferOptions> _globalOptions;
    private readonly ConcurrentQueue<SerializedLogRecord> _buffer;
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly IBufferedLogger _bufferedLogger;
    private readonly object _bufferCapacityLocker = new();
    private readonly ObjectPool<List<PooledLogRecord>> _logRecordPool = PoolFactory.CreateListPool<PooledLogRecord>();
    private DateTimeOffset _truncateAfter;
    private DateTimeOffset _lastFlushTimestamp;

    public HttpRequestBuffer(IBufferedLogger bufferedLogger,
        IOptionsMonitor<HttpRequestBufferOptions> options,
        IOptionsMonitor<GlobalBufferOptions> globalOptions)
    {
        _options = options;
        _globalOptions = globalOptions;
        _bufferedLogger = bufferedLogger;
        _buffer = new ConcurrentQueue<SerializedLogRecord>();

        _truncateAfter = _timeProvider.GetUtcNow();
    }

    public bool TryEnqueue<TState>(
        LogLevel logLevel,
        string category,
        EventId eventId,
        TState attributes,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(category, logLevel, eventId))
        {
            return false;
        }

        switch (attributes)
        {
            case ModernTagJoiner modernTagJoiner:
                _buffer.Enqueue(new SerializedLogRecord(logLevel, eventId, _timeProvider.GetUtcNow(), modernTagJoiner, exception,
                    ((Func<ModernTagJoiner, Exception?, string>)(object)formatter)(modernTagJoiner, exception)));
                break;
            case LegacyTagJoiner legacyTagJoiner:
                _buffer.Enqueue(new SerializedLogRecord(logLevel, eventId, _timeProvider.GetUtcNow(), legacyTagJoiner, exception,
                    ((Func<LegacyTagJoiner, Exception?, string>)(object)formatter)(legacyTagJoiner, exception)));
                break;
            default:
                Throw.ArgumentException(nameof(attributes), $"Unsupported type of the log attributes object detected: {typeof(TState)}");
                break;
        }

        var now = _timeProvider.GetUtcNow();
        lock (_bufferCapacityLocker)
        {
            if (now >= _truncateAfter)
            {
                _truncateAfter = now.Add(_options.CurrentValue.PerRequestDuration);
                TruncateOverlimit();
            }
        }

        return true;
    }

    public void Flush()
    {
        var result = _buffer.ToArray();
        _buffer.Clear();

        _lastFlushTimestamp = _timeProvider.GetUtcNow();

        List<PooledLogRecord>? pooledList = null;
        try
        {
            pooledList = _logRecordPool.Get();
            foreach (var serializedRecord in result)
            {
                pooledList.Add(
                    new PooledLogRecord(
                        serializedRecord.Timestamp,
                        serializedRecord.LogLevel,
                        serializedRecord.EventId,
                        serializedRecord.Exception,
                        serializedRecord.FormattedMessage,
                        serializedRecord.Attributes));
            }

            _bufferedLogger.LogRecords(pooledList);
        }
        finally
        {
            if (pooledList is not null)
            {
                _logRecordPool.Return(pooledList);
            }
        }
    }

    public bool IsEnabled(string category, LogLevel logLevel, EventId eventId)
    {
        if (_timeProvider.GetUtcNow() < _lastFlushTimestamp + _globalOptions.CurrentValue.SuspendAfterFlushDuration)
        {
            return false;
        }

        LoggerFilterRuleSelector.Select(_options.CurrentValue.Rules, category, logLevel, eventId, out BufferFilterRule? rule);

        return rule is not null;
    }

    public void TruncateOverlimit()
    {
        // Capacity is a soft limit, which might be exceeded, esp. in multi-threaded environments.
        while (_buffer.Count > _options.CurrentValue.PerRequestCapacity)
        {
            _ = _buffer.TryDequeue(out _);
        }
    }
}
