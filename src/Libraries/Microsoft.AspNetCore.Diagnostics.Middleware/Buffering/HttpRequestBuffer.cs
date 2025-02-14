﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Shared.Diagnostics;
using static Microsoft.Extensions.Logging.ExtendedLogger;

namespace Microsoft.AspNetCore.Diagnostics.Buffering;

internal sealed class HttpRequestBuffer : ILoggingBuffer
{
    private readonly IOptionsMonitor<HttpRequestLogBufferingOptions> _options;
    private readonly IOptionsMonitor<GlobalLogBufferingOptions> _globalOptions;
    private readonly ConcurrentQueue<SerializedLogRecord> _buffer;
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly IBufferedLogger _bufferedLogger;

    private DateTimeOffset _lastFlushTimestamp;
    private int _bufferSize;

    public HttpRequestBuffer(IBufferedLogger bufferedLogger,
        IOptionsMonitor<HttpRequestLogBufferingOptions> options,
        IOptionsMonitor<GlobalLogBufferingOptions> globalOptions)
    {
        _options = options;
        _globalOptions = globalOptions;
        _bufferedLogger = bufferedLogger;
        _buffer = new ConcurrentQueue<SerializedLogRecord>();
    }

    public bool TryEnqueue<TState>(LogEntry<TState> logEntry)
    {
        SerializedLogRecord serializedLogRecord = default;
        if (logEntry.State is ModernTagJoiner modernTagJoiner)
        {
            if (!IsEnabled(logEntry.Category, logEntry.LogLevel, logEntry.EventId, modernTagJoiner))
            {
                return false;
            }

            serializedLogRecord = new SerializedLogRecord(logEntry.LogLevel, logEntry.EventId, _timeProvider.GetUtcNow(), modernTagJoiner, logEntry.Exception,
                ((Func<ModernTagJoiner, Exception?, string>)(object)logEntry.Formatter)(modernTagJoiner, logEntry.Exception));
        }
        else if (logEntry.State is LegacyTagJoiner legacyTagJoiner)
        {
            if (!IsEnabled(logEntry.Category, logEntry.LogLevel, logEntry.EventId, legacyTagJoiner))
            {
                return false;
            }

            serializedLogRecord = new SerializedLogRecord(logEntry.LogLevel, logEntry.EventId, _timeProvider.GetUtcNow(), legacyTagJoiner, logEntry.Exception,
                ((Func<LegacyTagJoiner, Exception?, string>)(object)logEntry.Formatter)(legacyTagJoiner, logEntry.Exception));
        }
        else
        {
            Throw.InvalidOperationException($"Unsupported type of the log state object detected: {typeof(TState)}");
        }

        if (serializedLogRecord.SizeInBytes > _globalOptions.CurrentValue.MaxLogRecordSizeInBytes)
        {
            return false;
        }

        _buffer.Enqueue(serializedLogRecord);
        _ = Interlocked.Add(ref _bufferSize, serializedLogRecord.SizeInBytes);

        Trim();

        return true;
    }

    public void Flush()
    {
        _lastFlushTimestamp = _timeProvider.GetUtcNow();

        SerializedLogRecord[] bufferedRecords = _buffer.ToArray();

        _buffer.Clear();

        var deserializedLogRecords = new List<DeserializedLogRecord>(bufferedRecords.Length);
        foreach (var bufferedRecord in bufferedRecords)
        {
            deserializedLogRecords.Add(
                new DeserializedLogRecord(
                    bufferedRecord.Timestamp,
                    bufferedRecord.LogLevel,
                    bufferedRecord.EventId,
                    bufferedRecord.Exception,
                    bufferedRecord.FormattedMessage,
                    bufferedRecord.Attributes));
        }

        _bufferedLogger.LogRecords(deserializedLogRecords);
    }

    public bool IsEnabled(string category, LogLevel logLevel, EventId eventId, IReadOnlyList<KeyValuePair<string, object?>> attributes)
    {
        if (_timeProvider.GetUtcNow() < _lastFlushTimestamp + _globalOptions.CurrentValue.SuspendAfterFlushDuration)
        {
            return false;
        }

        LogBufferingFilterRuleSelector.Select(_options.CurrentValue.Rules, category, logLevel, eventId, attributes, out LogBufferingFilterRule? rule);

        return rule is not null;
    }

    private void Trim()
    {
        while (_bufferSize > _options.CurrentValue.MaxPerRequestBufferSizeInBytes && _buffer.TryDequeue(out var item))
        {
            _ = Interlocked.Add(ref _bufferSize, -item.SizeInBytes);
        }
    }
}
