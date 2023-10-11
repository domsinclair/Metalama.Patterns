// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NETSTANDARD || NETCOREAPP
using Metalama.Patterns.Caching.Backends.Azure;
using Metalama.Patterns.Caching.Building;
using Metalama.Patterns.Caching.TestHelpers;
using Metalama.Patterns.TestHelpers;
using Xunit.Abstractions;

namespace Metalama.Patterns.Caching.Tests.Backends.Distributed;

#if ENABLE_AZURE_TESTS
public
#else

// ReSharper disable once UnusedType.Global
internal
#endif
    class AzureServiceBusInvalidationTests2 : BaseInvalidationBrokerTests
{
    private static readonly string _connectionString = Secrets.Get( "CacheInvalidationNetCoreTestServiceBusConnectionString" );

    public AzureServiceBusInvalidationTests2( CachingTestOptions cachingTestOptions, ITestOutputHelper testOutputHelper ) : base(
        cachingTestOptions,
        testOutputHelper ) { }

    protected override BuiltCachingBackendBuilder AddInvalidationBroker( MemoryCachingBackendBuilder builder, string prefix )
        => builder.WithAzureInvalidation( new AzureCacheInvalidatorConfiguration( _connectionString ) { Prefix = prefix } );
}

#endif