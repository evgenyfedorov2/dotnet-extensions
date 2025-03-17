﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Buffering;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Diagnostics.Buffering;

internal sealed class PerRequestLogBufferingConfigureOptions : IConfigureOptions<PerRequestLogBufferingOptions>
{
    private const string ConfigSectionName = "PerIncomingRequestLogBuffering";
    private readonly IConfiguration _configuration;

    public PerRequestLogBufferingConfigureOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(PerRequestLogBufferingOptions options)
    {
        if (_configuration == null)
        {
            return;
        }

        IConfigurationSection section = _configuration.GetSection(ConfigSectionName);
        if (!section.Exists())
        {
            return;
        }

        var parsedOptions = section.Get<PerRequestLogBufferingOptions>();
        if (parsedOptions is null)
        {
            return;
        }

        foreach (LogBufferingFilterRule rule in parsedOptions.Rules)
        {
            options.Rules.Add(rule);
        }
    }
}
