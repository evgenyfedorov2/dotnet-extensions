﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#if NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Testing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Diagnostics.Logging.Test;

public class HeaderReaderTests
{
    [Fact]
    public void ShouldNotAddHeaders_WhenFilteringSetEmpty()
    {
        var reader = new HeaderReader(new Dictionary<string, DataClassification>(), null!);
        var listToFill = new List<KeyValuePair<string, object?>>();
        var headers = new HeaderDictionary(new Dictionary<string, StringValues> { [HeaderNames.Accept] = MediaTypeNames.Text.Plain });
        reader.Read(headers, listToFill, string.Empty);
        Assert.Empty(listToFill);
    }

    [Fact]
    public void ShouldNotAddHeaders_WhenHeadersCollectionEmpty()
    {
        var reader = new HeaderReader(new Dictionary<string, DataClassification> { [HeaderNames.Accept] = DataClassification.Unknown }, null!);
        var listToFill = new List<KeyValuePair<string, object?>>();
        reader.Read(new HeaderDictionary(), listToFill, string.Empty);
        Assert.Empty(listToFill);
    }

    [Fact]
    public void ShouldAddHeaders_WhenHeadersCollectionNotEmpty()
    {
        var headersToLog = new Dictionary<string, DataClassification> { [HeaderNames.Accept] = DataClassification.Unknown };
        var reader = new HeaderReader(headersToLog, new FakeRedactorProvider(new FakeRedactorOptions { RedactionFormat = "<redacted:{0}>" }));
        var headers = new Dictionary<string, StringValues>
        {
            [HeaderNames.Accept] = MediaTypeNames.Text.Xml,
            [HeaderNames.ContentType] = MediaTypeNames.Application.Pdf
        };

        var listToFill = new List<KeyValuePair<string, object?>>();
        reader.Read(new HeaderDictionary(headers), listToFill, string.Empty);

        Assert.Single(listToFill);

        var redacted = listToFill[0];
        Assert.Equal(HeaderNames.Accept, redacted.Key);
        Assert.Equal($"<redacted:{MediaTypeNames.Text.Xml}>", redacted.Value);
    }
}
#endif
