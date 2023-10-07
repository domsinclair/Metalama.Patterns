// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Patterns.Caching;

/// <summary>
/// Extension methods to <see cref="IServiceCollection"/>.
/// </summary>
[PublicAPI]
public static class CachingServiceFactory
{
    public static IServiceCollection AddCaching(
        this IServiceCollection serviceCollection,
        Action<CachingService.Builder>? build = null )
    {
        serviceCollection.Add(
            ServiceDescriptor.Singleton<ICachingService, CachingService>( serviceProvider => CachingService.Create( build, serviceProvider ) ) );

        return serviceCollection;
    }
}