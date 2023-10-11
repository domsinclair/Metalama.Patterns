// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Patterns.Caching.Backends;
using static Flashtrace.Messages.FormattedMessageBuilder;

namespace Metalama.Patterns.Caching.Implementation;

/// <summary>
/// Base class for a kind of <see cref="CachingBackendEnhancer"/> that allows several instances of the same application to use
/// a local cache, and synchronize themselves by sending invalidation messages over a publish/subscribe channel.
/// </summary>
[PublicAPI]
public abstract class CacheInvalidator : CachingBackendEnhancer
{
    private readonly BackgroundTaskScheduler _backgroundTaskScheduler;

    /// <summary>
    /// Gets the options of the current <see cref="CacheInvalidator"/>.
    /// </summary>
    public new CacheInvalidatorConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidator"/> class.
    /// </summary>
    /// <param name="underlyingBackend">The underlying <see cref="CachingBackend"/> (typically an in-memory cache).</param>
    /// <param name="configuration">Options of the new <see cref="CacheInvalidator"/>.</param>
    protected CacheInvalidator( CachingBackend underlyingBackend, CacheInvalidatorConfiguration configuration ) : base( underlyingBackend )
    {
        this.Configuration = configuration;
        this._backgroundTaskScheduler = new BackgroundTaskScheduler( underlyingBackend.ServiceProvider );
    }

    /// <inheritdoc />
    protected override async ValueTask RemoveItemAsyncCore( string key, CancellationToken cancellationToken )
    {
        await base.RemoveItemAsyncCore( key, cancellationToken );

        this._backgroundTaskScheduler.EnqueueBackgroundTask( () => this.PublishInvalidationAsync( key, CacheKeyKind.Item, CancellationToken.None ) );
    }

    /// <inheritdoc />
    protected override void RemoveItemCore( string key )
    {
        base.RemoveItemCore( key );

        this._backgroundTaskScheduler.EnqueueBackgroundTask( () => this.PublishInvalidationAsync( key, CacheKeyKind.Item, CancellationToken.None ) );
    }

    /// <inheritdoc />
    protected override async ValueTask InvalidateDependencyAsyncCore( string key, CancellationToken cancellationToken )
    {
        await base.InvalidateDependencyAsyncCore( key, cancellationToken );

        this._backgroundTaskScheduler.EnqueueBackgroundTask( () => this.PublishInvalidationAsync( key, CacheKeyKind.Dependency, CancellationToken.None ) );
    }

    /// <inheritdoc />
    protected override void InvalidateDependencyCore( string key )
    {
        base.InvalidateDependencyCore( key );

        this._backgroundTaskScheduler.EnqueueBackgroundTask( () => this.PublishInvalidationAsync( key, CacheKeyKind.Dependency, CancellationToken.None ) );
    }

    /// <summary>
    /// Implementations of <see cref="CacheInvalidator"/> must call this method when an invalidation message is received.
    /// </summary>
    /// <param name="message">The serialized invalidation message.</param>
    protected void OnMessageReceived( string message )
    {
        var tokenizer = new StringTokenizer( message );
        var prefix = tokenizer.GetNext( ':' );

        if ( !prefix.Equals( this.Configuration.Prefix.AsSpan(), StringComparison.Ordinal ) )
        {
            return;
        }

        var activity = this.Source.Default.OpenActivity( Formatted( "{This} is processing the message {Message}.", this, message ) );

        try
        {
            var kind = tokenizer.GetNext( ':' );
            var backendIdStr = tokenizer.GetNext( ':' );

#if NET6_0_OR_GREATER
            if ( !Guid.TryParse( backendIdStr, out var sourceId ) )
#else
            if ( !Guid.TryParse( backendIdStr.ToString(), out var sourceId ) )
#endif
            {
                activity.SetOutcome(
                    this.Source.Failure.Level,
                    Formatted( "Failed: cannot parse the SourceId '{SourceId}' into a Guid. Skipping the event.", backendIdStr.ToString() ) );

                return;
            }

            var key = tokenizer.GetRemainder().ToString();

            // We use synchronous APIs because most the typical consumer of InvalidationBroker is synchronous.

            if ( sourceId == this.UnderlyingBackend.Id )
            {
                this.Source.Default.Write( Formatted( "Skipping the message {Message} because it has sent it itself.", message ) );
                activity.SetResult( "Skipped." );

                return;
            }

            switch ( kind )
            {
                case "dependency":
                    this.UnderlyingBackend.InvalidateDependency( key );
                    this.Source.Default.Write( Formatted( "Invalidated the dependency {Key}.", key ) );
                    activity.SetSuccess();

                    break;

                case "item":
                    this.UnderlyingBackend.RemoveItem( key );
                    this.Source.Default.Write( Formatted( "Removed the item {Key}.", key ) );
                    activity.SetSuccess();

                    break;

                default:
                    activity.SetOutcome( this.Source.Failure.Level, Formatted( "Failed: invalid kind key: {Kind}.", kind.ToString() ) );

                    break;
            }
        }
        catch ( Exception e )
        {
            activity.SetException( e );
        }
    }

    private string GetMessage( string kind, string key )
        => this.Configuration.Prefix + ":" + kind.ToLowerInvariant() + ":" + this.UnderlyingBackend.Id + ":" + key;

    private Task PublishInvalidationAsync( string key, CacheKeyKind cacheKeyKind, CancellationToken cancellationToken )
    {
        var message = this.GetMessage( cacheKeyKind, key );

        this.Source.Debug.Write( Formatted( "{This} is sending the message {Message}.", this, message ) );

        return this.SendMessageAsync( message, cancellationToken );
    }

    private string GetMessage( CacheKeyKind cacheKeyKind, string key )
    {
        switch ( cacheKeyKind )
        {
            case CacheKeyKind.Item:
                return this.GetMessage( "item", key );

            case CacheKeyKind.Dependency:
                return this.GetMessage( "dependency", key );

            default:
                throw new ArgumentOutOfRangeException( nameof(cacheKeyKind), cacheKeyKind, null );
        }
    }

    /// <summary>
    /// Sends an invalidation message over the message bus of the implementation.
    /// </summary>
    /// <param name="message">A serialized, opaque serialization message.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected abstract Task SendMessageAsync( string message, CancellationToken cancellationToken );

    private enum CacheKeyKind
    {
        Item,
        Dependency
    }
}