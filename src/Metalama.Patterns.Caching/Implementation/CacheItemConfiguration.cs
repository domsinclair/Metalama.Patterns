﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Patterns.Caching.Implementation;

/// <summary>
/// Configuration of a <see cref="CacheItem"/>.
/// </summary>
internal sealed class CacheItemConfiguration : IRunTimeCacheItemConfiguration
{
    public CacheItemConfiguration() { }

    /// <inheritdoc />
    public bool? IsEnabled { get; set; }

    /// <inheritdoc />
    public string? ProfileName { get; set; }

    /// <inheritdoc />
    public bool? AutoReload { get; set; }

    /// <inheritdoc />
    public TimeSpan? AbsoluteExpiration { get; set; }

    /// <inheritdoc />
    public TimeSpan? SlidingExpiration { get; set; }

    /// <inheritdoc />
    public CacheItemPriority? Priority { get; set; }

    /// <inheritdoc />
    public bool? IgnoreThisParameter { get; set; }

    public void ApplyFallback( IRunTimeCacheItemConfiguration fallback )
    {
        this.AutoReload ??= fallback.AutoReload;
        this.AbsoluteExpiration ??= fallback.AbsoluteExpiration;
        this.SlidingExpiration ??= fallback.SlidingExpiration;
        this.Priority ??= fallback.Priority;
        this.ProfileName ??= fallback.ProfileName;
        this.IsEnabled ??= fallback.IsEnabled;
        this.IgnoreThisParameter ??= fallback.IgnoreThisParameter;
    }

    internal CacheItemConfiguration Clone() => (CacheItemConfiguration) this.MemberwiseClone();
}