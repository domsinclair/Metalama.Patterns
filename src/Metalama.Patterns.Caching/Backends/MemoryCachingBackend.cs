﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Patterns.Caching.Implementation;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.Caching;
using CacheItemPriority = System.Runtime.Caching.CacheItemPriority;
using PSCacheItem = Metalama.Patterns.Caching.Implementation.CacheItem;
using MemoryCacheItemPolicy = System.Runtime.Caching.CacheItemPolicy;
using PSCacheItemPriority = Metalama.Patterns.Caching.Implementation.CacheItemPriority;

namespace Metalama.Patterns.Caching.Backends
{
    /// <summary>
    /// A <see cref="CachingBackend"/> based on <c>System.Runtime.Caching.MemoryCache</c> (<see cref="MemoryCache"/>). This cache is part of .NET Framework
    /// and is available for .NET Standard in the NuGet package System.Runtime.Caching.
    /// </summary>
    public sealed class MemoryCachingBackend : CachingBackend
    {
        private static readonly MemoryCacheItemPolicy dependencyCacheItemPolicy = new() { Priority = CacheItemPriority.NotRemovable };

        private readonly MemoryCache cache;

        private static string GetItemKey( string key )
        {
            return nameof(MemoryCachingBackend) + ":item:" + key;
        }

        private static string GetDependencyKey( string key )
        {
            return nameof(MemoryCachingBackend) + ":dependency:" + key;
        }

        /// <summary>
        /// Initializes a new <see cref="MemoryCachingBackend"/> based on the <see cref="MemoryCache.Default"/> instance of the <see cref="MemoryCache"/> class.
        /// </summary>
        public MemoryCachingBackend() : this( null ) { }

        /// <summary>
        /// Initializes a new <see cref="MemoryCachingBackend"/> based on the given <see cref="MemoryCache"/>.
        /// </summary>
        /// <param name="cache">A <see cref="MemoryCache"/>, or <c>null</c> to use  the <see cref="MemoryCache.Default"/> instance of the <see cref="MemoryCache"/> class.</param>
        public MemoryCachingBackend( MemoryCache cache )
        {
            this.cache = cache ?? MemoryCache.Default;
        }

        private static CacheItemRemovedReason CreateRemovalReason( CacheEntryRemovedReason sourceReason )
        {
            switch ( sourceReason )
            {
                case CacheEntryRemovedReason.CacheSpecificEviction:
                    return CacheItemRemovedReason.Other;

                case CacheEntryRemovedReason.ChangeMonitorChanged:
                    return CacheItemRemovedReason.Invalidated;

                case CacheEntryRemovedReason.Evicted:
                    return CacheItemRemovedReason.Evicted;

                case CacheEntryRemovedReason.Expired:
                    return CacheItemRemovedReason.Expired;

                case CacheEntryRemovedReason.Removed:
                    return CacheItemRemovedReason.Removed;

                default:
                    throw new ArgumentException( string.Format( CultureInfo.InvariantCulture, "The CacheEntryRemovedReason '{0}' is unknown.", sourceReason ) );
            }
        }

        private MemoryCacheItemPolicy CreatePolicy( PSCacheItem item )
        {
            var targetPolicy = new MemoryCacheItemPolicy { RemovedCallback = this.OnCacheItemRemoved };

            if ( item.Configuration != null )
            {
                if ( item.Configuration.AbsoluteExpiration.HasValue )
                {
                    targetPolicy.AbsoluteExpiration = DateTime.Now + item.Configuration.AbsoluteExpiration.Value;
                }

                if ( item.Configuration.SlidingExpiration.HasValue )
                {
                    targetPolicy.SlidingExpiration = item.Configuration.SlidingExpiration.Value;
                }

                switch ( item.Configuration.Priority.GetValueOrDefault() )
                {
                    case PSCacheItemPriority.Default:
                    case PSCacheItemPriority.Low:
                    case PSCacheItemPriority.High:
                        targetPolicy.Priority = CacheItemPriority.Default;

                        break;

                    case PSCacheItemPriority.NotRemovable:
                        targetPolicy.Priority = CacheItemPriority.NotRemovable;

                        break;

                    default:
                        throw new NotSupportedException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The priority '{0}' is not supported by the MemoryCache back-end.",
                                item.Configuration.Priority ) );
                }
            }

            return targetPolicy;
        }

        private void OnCacheItemRemoved( CacheEntryRemovedArguments arguments )
        {
            if ( arguments.RemovedReason == CacheEntryRemovedReason.Removed )
            {
                // In this case, all actions are taken by the method that removes the item.
                return;
            }

            var prefix = GetItemKey( "" );

            if ( arguments.CacheItem.Key.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
            {
                var key = arguments.CacheItem.Key.Substring( prefix.Length );

                var item = (CacheValue) arguments.CacheItem.Value;
                this.CleanDependencies( key, item );

                this.OnItemRemoved( key, CreateRemovalReason( arguments.RemovedReason ), this.Id );
            }
        }

        private void AddDependencies( string key, IImmutableList<string> dependencies )
        {
            if ( dependencies == null || dependencies.Count <= 0 )
            {
                return;
            }

            foreach ( var dependency in dependencies )
            {
                var dependencyKey = GetDependencyKey( dependency );

                HashSet<string> backwardDependencies = (HashSet<string>) this.cache.Get( dependencyKey );

                if ( backwardDependencies == null )
                {
                    HashSet<string> newHashSet = new();
                    backwardDependencies = (HashSet<string>) this.cache.AddOrGetExisting( dependencyKey, newHashSet, dependencyCacheItemPolicy ) ?? newHashSet;
                }

                lock ( backwardDependencies )
                {
                    backwardDependencies.Add( key );

                    // The invalidation callback may have removed the key.
                    var addOrGetExisting = this.cache.AddOrGetExisting( dependencyKey, backwardDependencies, dependencyCacheItemPolicy );
                }
            }
        }

        /// <inheritdoc />
        protected override void SetItemCore( string key, PSCacheItem item )
        {
            var itemKey = GetItemKey( key );
            var lockTaken = false;
            var previousValue = (MemoryCacheValue) this.cache.Get( itemKey );

            try
            {
                if ( previousValue != null )
                {
                    Monitor.Enter( previousValue.Sync, ref lockTaken );
                    this.CleanDependencies( key, previousValue );
                }

                if ( item.Dependencies != null && item.Dependencies.Count > 0 )
                {
                    this.AddDependencies( key, item.Dependencies );
                }

                this.cache.Set(
                    itemKey,
                    new MemoryCacheValue( item.Value, item.Dependencies, previousValue?.Sync ?? new object() ),
                    this.CreatePolicy( item ) );
            }
            finally
            {
                if ( lockTaken )
                {
                    Monitor.Exit( previousValue.Sync );
                }
            }
        }

        /// <inheritdoc />
        protected override bool ContainsItemCore( string key )
        {
            return this.cache.Contains( GetItemKey( key ) );
        }

        /// <inheritdoc />  
        protected override CacheValue GetItemCore( string key, bool includeDependencies )
        {
            return (CacheValue) this.cache.Get( GetItemKey( key ) );
        }

        /// <inheritdoc />
        protected override void InvalidateDependencyCore( string key )
        {
            this.InvalidateDependencyImpl( key );
        }

        internal void InvalidateDependencyImpl( string key, MemoryCacheValue replacementValue = null, DateTimeOffset? replacementValueExpiration = null )
        {
            HashSet<string> items = (HashSet<string>) this.cache.Get( GetDependencyKey( key ) );

            if ( items != null )
            {
                lock ( items )
                {
                    foreach ( var item in items.ToList() )
                    {
                        if ( this.RemoveItemImpl( item, replacementValue, replacementValueExpiration ) )
                        {
                            this.OnItemRemoved( item, CacheItemRemovedReason.Invalidated, this.Id );
                        }
                    }

                    // A side effect of calling RemoveItems is to remove the dependency entry so
                    // we don't have to do it a second time.
                }
            }

            this.OnDependencyInvalidated( key, this.Id );
        }

        internal bool RemoveItemImpl( string key, MemoryCacheValue replacementValue = null, DateTimeOffset? replacementValueExpiration = null )
        {
            var itemKey = GetItemKey( key );

            var cacheValue = (MemoryCacheValue) this.cache.Get( itemKey );

            if ( cacheValue == null )
            {
                return false;
            }

            lock ( cacheValue.Sync )
            {
                if ( replacementValue == null )
                {
                    cacheValue = (MemoryCacheValue) this.cache.Remove( itemKey );

                    if ( cacheValue == null )
                    {
                        // The item has been removed by another thread.
                        return false;
                    }
                }
                else
                {
                    replacementValue.Sync = cacheValue.Sync;
                    this.cache.Set( itemKey, replacementValue, replacementValueExpiration.Value );
                }

                this.CleanDependencies( key, cacheValue );
            }

            return true;
        }

        private void CleanDependencies( string key, CacheValue cacheValue )
        {
            if ( cacheValue.Dependencies == null )
            {
                return;
            }

            foreach ( var dependency in cacheValue.Dependencies )
            {
                var dependencyKey = GetDependencyKey( dependency );
                HashSet<string> backwardDependencies = (HashSet<string>) this.cache.Get( dependencyKey );

                if ( backwardDependencies == null )
                {
                    continue;
                }

                lock ( backwardDependencies )
                {
                    backwardDependencies.Remove( key );

                    if ( backwardDependencies.Count == 0 )
                    {
                        this.cache.Remove( dependencyKey );
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override bool ContainsDependencyCore( string key )
        {
            return this.cache.Contains( GetDependencyKey( key ) );
        }

        /// <inheritdoc />
        protected override void ClearCore()
        {
            this.cache.Trim( 100 );
        }

        /// <inheritdoc />
        protected override void RemoveItemCore( string key )
        {
            if ( this.RemoveItemImpl( key ) )
            {
                this.OnItemRemoved( key, CacheItemRemovedReason.Removed, this.Id );
            }
        }
    }
}