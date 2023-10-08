// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Patterns.Caching.Implementation;
using Metalama.Patterns.Caching.Serializers;

namespace Metalama.Patterns.Caching.Backends.Redis;

/// <summary>
/// Configuration for <see cref="RedisCachingBackend"/>.
/// </summary>
[PublicAPI]
public record RedisCachingBackendConfiguration : CachingBackendConfiguration
{
    private string? _keyPrefix = "cache";

    /// <summary>
    /// Gets the prefix for the key of all Redis items created by the <see cref="RedisCachingBackend"/>. The default value is <c>cache</c>.
    /// </summary>
    public string? KeyPrefix
    {
        get => this._keyPrefix;
        init
        {
            if ( value != null
#if NETFRAMEWORK || NETSTANDARD
                 && value.IndexOf( ":", StringComparison.Ordinal ) != -1
#else
                 && value.Contains( ":", StringComparison.Ordinal )
#endif
               )
            {
                throw new ArgumentOutOfRangeException( nameof(value), "The KeyPrefix property value cannot contain the ':' character." );
            }

            this._keyPrefix = value;
        }
    }

    /// <summary>
    /// Gets the index of the database to use. The default value is <c>-1</c> (automatic selection).
    /// </summary>
    public int Database { get; init; } = -1;

    /// <summary>
    /// Gets a function that creates the serializer used to serialize objects into byte arrays (and conversely).
    /// The default value is <c>null</c>, which means that <see cref="BinaryCachingSerializer"/> will be used.
    /// </summary>
    public Func<ICachingSerializer>? CreateSerializer { get; init; }

    /// <summary>
    /// Gets a value indicating whether determines whether the <see cref="RedisCachingBackend"/> should dispose the Redis connection when the <see cref="RedisCachingBackend"/>
    /// itself is disposed.
    /// </summary>
    public bool OwnsConnection { get; init; }

    /// <summary>
    /// Gets the number of times Redis transactions are retried when they fail due to a data conflict, before an exception
    /// is raised. The default value is <c>5</c>.
    /// </summary>
    public int TransactionMaxRetries { get; init; } = 5;

    /// <summary>
    /// Gets a value indicating whether the <see cref="RedisCachingBackend"/> should support dependencies. When this property is used,
    /// the <see cref="DependenciesRedisCachingBackend"/> class is used instead of <see cref="RedisCachingBackend"/>. When dependencies
    /// are enabled, at least one instance of the <see cref="RedisCacheDependencyGarbageCollector"/> MUST run.
    /// </summary>
    public bool SupportsDependencies { get; init; }

    /// <summary>
    /// Gets the default expiration time of cached items.
    /// All items that don't have an explicit expiration time are automatically expired according to the value
    /// of this property, unless they have the <see cref="CacheItemPriority.NotRemovable"/> priority.
    /// The default value is 1 day.
    /// </summary>
    public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromDays( 1 );

    /// <summary>
    /// Gets the time that the Redis backend will wait for a Redis connection.
    /// (When you create a new Redis backend, if it doesn't connect to a Redis server in this timeout, a <see cref="TimeoutException"/> is thrown.)
    /// </summary>
    /// <remarks>
    /// The default value is 1 minute.
    /// </remarks>
    public TimeSpan ConnectionTimeout { get; init; } = RedisNotificationQueue.DefaultSubscriptionTimeout;
}