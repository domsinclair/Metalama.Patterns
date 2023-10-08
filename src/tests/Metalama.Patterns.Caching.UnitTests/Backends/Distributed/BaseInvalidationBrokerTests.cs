﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Patterns.Caching.Backends;
using Metalama.Patterns.Caching.Building;
using Metalama.Patterns.Caching.TestHelpers;
using Xunit.Abstractions;

namespace Metalama.Patterns.Caching.Tests.Backends.Distributed;

public abstract class BaseInvalidationBrokerTests : BaseDistributedCacheTests
{
    protected BaseInvalidationBrokerTests( CachingTestOptions cachingTestOptions, ITestOutputHelper testOutputHelper ) : base(
        cachingTestOptions,
        testOutputHelper ) { }

    protected abstract BuiltCachingBackendBuilder AddInvalidationBroker( MemoryCachingBackendBuilder builder, string prefix );

    protected override async Task<CachingBackend[]> CreateBackendsAsync()
    {
        var testId = Guid.NewGuid().ToString();

        Task<CachingBackend> CreateCacheInvalidator()
        {
            return CachingBackend.CreateAsync(
                b => this.AddInvalidationBroker( b.Memory(), testId ),
                this.ServiceProvider );
        }

        return new[] { await CreateCacheInvalidator(), await CreateCacheInvalidator(), await CreateCacheInvalidator() };
    }

    protected override CachingBackend[] CreateBackends()
    {
        return this.CreateBackendsAsync().Result;
    }
}