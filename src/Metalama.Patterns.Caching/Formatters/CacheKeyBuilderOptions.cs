// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.Caching.Formatters;

/// <summary>
/// Options of the <see cref="CacheKeyBuilder"/> class.
/// </summary>
public record CacheKeyBuilderOptions
{
    public int MaxKeySize { get; init; } = 1024;
}