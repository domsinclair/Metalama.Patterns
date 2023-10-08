// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Patterns.Caching.Building;
using StackExchange.Redis;

namespace Metalama.Patterns.Caching.Backends.Redis;

[PublicAPI]
public static class RedisCachingBackendFactory
{
    public static RedisCachingBackendBuilder Redis(
        this CachingBackendBuilder builder,
        IConnectionMultiplexer connection,
        RedisCachingBackendConfiguration? configuration = null )
        => new( connection, configuration );

    public static RedisInvalidatedCachingBackendBuilder WithRedisInvalidator(
        this MemoryCachingBackendBuilder builder,
        IConnectionMultiplexer connection )
        => new( builder, connection );
}