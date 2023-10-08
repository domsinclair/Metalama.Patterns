// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace;
using Flashtrace.Formatters;
using Metalama.Patterns.Caching.Backends;
using Metalama.Patterns.Caching.Building;
using Metalama.Patterns.Caching.Formatters;
using Metalama.Patterns.Caching.Implementation;
using Metalama.Patterns.Caching.ValueAdapters;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Patterns.Caching;

public sealed partial class CachingService : IDisposable, IAsyncDisposable, ICachingService
{
    private readonly FormatterRepository _formatters;

    public static CachingService Default { get; set; } = CreateUninitialized();

    public static CachingService Create( Action<ICachingServiceBuilder>? build = null, IServiceProvider? serviceProvider = null )
    {
        var builder = new Builder( serviceProvider );
        build?.Invoke( builder );
        builder.Dispose();

        var backend = builder.CreateBackend();

        return new CachingService( serviceProvider, backend, builder );
    }

    public static async Task<CachingService> CreateAsync(
        Action<ICachingServiceBuilder>? build = null,
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default )
    {
        var builder = new Builder( serviceProvider );
        build?.Invoke( builder );
        builder.Dispose();

        var backend = await builder.CreateBackendAsync( cancellationToken );

        return new CachingService( serviceProvider, backend, builder );
    }

    internal IServiceProvider? ServiceProvider { get; }

    internal AutoReloadManager AutoReloadManager { get; }

    internal CachingFrontend Frontend { get; }

    internal ValueAdapterFactory ValueAdapters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingService"/> class.
    /// </summary>
    /// <param name="backend">The default back-end. If null, a new <see cref="MemoryCachingBackend"/> is created.</param>
    /// <param name="serviceProvider">An optional <see cref="IServiceProvider"/>.</param>
    private CachingService( IServiceProvider? serviceProvider, CachingBackend? defaultBackend, Builder builder )
    {
        // Run the builder.

        this.DefaultBackend = defaultBackend ?? CachingBackend.Create( b => b.Memory(), serviceProvider );

        // Set profiles.
        var profilesDictionary = ImmutableDictionary.CreateBuilder<string, CachingProfile>( StringComparer.Ordinal );

        foreach ( var profile in builder.Profiles )
        {
            profilesDictionary.Add( profile.Name, profile );
            profile.Initialize( this.DefaultBackend );
        }

        if ( !profilesDictionary.ContainsKey( CachingProfile.DefaultName ) )
        {
            var profile = new CachingProfile();
            profilesDictionary.Add( CachingProfile.DefaultName, profile );
            profile.Initialize( this.DefaultBackend );
        }

        this._formatters = FormatterRepository.Create(
            CachingFormattingRole.Instance,
            formattersBuilder =>
            {
                formattersBuilder.AddFormatter( typeof(IEnumerable<>), typeof(CollectionFormatter<>) );

                foreach ( var action in builder.FormattersBuildActions )
                {
                    action( formattersBuilder );
                }
            } );

        this.ValueAdapters = new ValueAdapterFactory( builder.ValueAdapters );
        this.ServiceProvider = builder.ServiceProvider;
        this.KeyBuilder = new CacheKeyBuilder( this._formatters );
        this.Profiles = new CachingProfileRegistry( profilesDictionary.ToImmutable() );
        this.AllBackends = this.Profiles.AllBackends.ToImmutableArray();
        this.Frontend = new CachingFrontend( this );
        this.AutoReloadManager = new AutoReloadManager( this );
        this.KeyBuilder = builder.KeyBuilderFactory?.Invoke( this._formatters ) ?? new CacheKeyBuilder( this._formatters );
        this.Logger = builder.ServiceProvider.GetFlashtraceSource( this.GetType(), FlashtraceRole.Caching );
    }

    internal static CachingService CreateUninitialized( IServiceProvider? serviceProvider = null )
        => Create( b => b.WithBackend( b => b.Uninitialized() ), serviceProvider );

    /// <summary>
    /// Gets the set of distinct backends used in the service.
    /// </summary>
    public ImmutableArray<CachingBackend> AllBackends { get; }

    public FlashtraceSource Logger { get; }

    /// <summary>
    /// Gets the <see cref="CacheKeyBuilder"/> used to generate caching keys, i.e. to serialize objects into a <see cref="string"/>.
    /// </summary>
    [AllowNull]
    public ICacheKeyBuilder KeyBuilder { get; }

    /// <summary>
    /// Gets default <see cref="CachingBackend"/>, i.e. the physical storage of cache items.
    /// </summary>
    [AllowNull]
    public CachingBackend DefaultBackend { get; }

    /// <summary>
    /// Gets the repository of caching profiles (<see cref="CachingProfile"/>).
    /// </summary>
    public CachingProfileRegistry Profiles { get; }

    public void Dispose()
    {
        this.AutoReloadManager.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await this.AutoReloadManager.DisposeAsync();
    }
}