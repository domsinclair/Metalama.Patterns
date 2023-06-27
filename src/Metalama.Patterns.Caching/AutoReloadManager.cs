// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace;
using Metalama.Patterns.Caching.Implementation;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using static Flashtrace.FormattedMessageBuilder;

namespace Metalama.Patterns.Caching;

internal sealed class AutoReloadManager
{
    private readonly CachingBackend backend;
    private int autoRefreshSubscriptions;
    private readonly ConcurrentDictionary<string, AutoRefreshInfo> autoRefreshInfos = new();
    private readonly BackgroundTaskScheduler backgroundTaskScheduler = new();

    public AutoReloadManager( CachingBackend backend )
    {
        this.backend = backend;
    }

    private void BeginAutoRefreshValue( object sender, CacheItemRemovedEventArgs args )
    {
        var key = args.Key;
        AutoRefreshInfo autoRefreshInfo;

        if ( this.autoRefreshInfos.TryGetValue( key, out autoRefreshInfo ) )
        {
            if ( autoRefreshInfo.IsAsync )
            {
                this.backgroundTaskScheduler.EnqueueBackgroundTask( () => AutoRefreshCoreAsync( key, autoRefreshInfo, CancellationToken.None ) );
            }
            else
            {
                this.backgroundTaskScheduler.EnqueueBackgroundTask( () => Task.Run( () => AutoRefreshCore( key, autoRefreshInfo ) ) );
            }
        }
    }

    internal void SubscribeAutoRefresh(
        string key,
        Type valueType,
        CacheItemConfiguration configuration,
        Func<object> valueProvider,
        LogSource logger,
        bool isAsync )
    {
        if ( !this.backend.SupportedFeatures.Events )
        {
            logger.Warning.Write( Formatted( "The backend {Backend} does not support auto-refresh.", this.backend ) );

            return;
        }

        // TODO: We may want to preemptively renew the cache item before it gets removed, otherwise there could be latency.

        this.autoRefreshInfos.GetOrAdd(
            key,
            k =>
            {
                if ( Interlocked.Increment( ref this.autoRefreshSubscriptions ) == 1 )
                {
                    this.backend.ItemRemoved += this.BeginAutoRefreshValue;
                }

                return new AutoRefreshInfo( configuration, valueType, valueProvider, logger, isAsync );
            } );

        // NOTE: We never remove things from autoRefreshInfos. AutoRefresh keys are there forever, they are never evicted.
    }

    [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
    private static void AutoRefreshCore( string key, AutoRefreshInfo info )
    {
        using ( var activity = info.Logger.Default.OpenActivity( Formatted( "Auto-refreshing: {Key}", key ) ) )
        {
            try
            {
                using ( var context = CachingContext.OpenCacheContext( key ) )
                {
                    var value = info.ValueProvider.Invoke();

                    CachingFrontend.SetItem( key, value, info.ReturnType, info.Configuration, context );
                }

                activity.SetSuccess();
            }
            catch ( Exception e )
            {
                activity.SetException( e );
            }
        }
    }

    private static async Task AutoRefreshCoreAsync( string key, AutoRefreshInfo info, CancellationToken cancellationToken )
    {
        using ( var activity = info.Logger.Default.OpenActivity( Formatted( "Auto-refreshing: {Key}", key ) ) )
        {
            try
            {
                using ( var context = CachingContext.OpenCacheContext( key ) )
                {
                    Task<object> invokeValueProviderTask = (Task<object>) info.ValueProvider.Invoke();
                    var value = await invokeValueProviderTask;

                    await CachingFrontend.SetItemAsync( key, value, info.ReturnType, info.Configuration, context, cancellationToken );
                }

                activity.SetSuccess();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch ( Exception e )
#pragma warning restore CA1031 // Do not catch general exception types
            {
                activity.SetException( e );
            }
        }
    }

    private sealed class AutoRefreshInfo
    {
        public AutoRefreshInfo( CacheItemConfiguration configuration, Type returnType, Func<object> valueProvider, LogSource logger, bool isAsync )
        {
            this.Configuration = configuration;
            this.ReturnType = returnType;
            this.ValueProvider = valueProvider;
            this.Logger = logger;
            this.IsAsync = isAsync;
        }

        public Type ReturnType { get; }

        public Func<object> ValueProvider { get; }

        public LogSource Logger { get; }

        public bool IsAsync { get; }

        public CacheItemConfiguration Configuration { get; }
    }
}