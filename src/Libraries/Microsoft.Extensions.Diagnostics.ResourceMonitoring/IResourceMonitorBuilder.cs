﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Diagnostics.ResourceMonitoring;

/// <summary>
/// Helps building the resource monitoring infrastructure.
/// </summary>
public interface IResourceMonitorBuilder
{
    /// <summary>
    /// Gets the service collection being manipulated by the builder.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds a resource utilization publisher that gets invoked whenever resource utilization is computed.
    /// </summary>
    /// <typeparam name="T">The publisher's implementation type.</typeparam>
    /// <returns>The value of the object instance.</returns>
    [Obsolete("This method is obsolete. Instead of IResourceUtilizationPublisher use observable instruments from Microsoft.Extensions.Diagnostics.ResourceMonitoring.ResourceUtilizationInstruments.")]
    IResourceMonitorBuilder AddPublisher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IResourceUtilizationPublisher;
}
