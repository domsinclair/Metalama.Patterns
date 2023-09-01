﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace;
using JetBrains.Annotations;
using Metalama.Patterns.Caching.Implementation;
using System.ComponentModel;
using static Flashtrace.Messages.FormattedMessageBuilder;

namespace Metalama.Patterns.Caching;

/// <summary>
/// Runtime helpers called by code generated by <see cref="CacheAttribute"/>.
/// </summary>
[PublicAPI]
public partial class CachingService
{
    private ICacheItemConfiguration GetMergedMethodConfiguration( CachedMethodMetadata methodMetadata )
        => this.Profiles[methodMetadata.BuildTimeConfiguration.ProfileName].GetMergedConfiguration( methodMetadata );

    [EditorBrowsable( EditorBrowsableState.Never )]
    public TResult? GetFromCacheOrExecute<TResult>(
        CachedMethodMetadata metadata,
        Func<object?, object?[], object?> func,
        object? instance,
        object?[] args,
        CancellationToken cancellationToken )
    {
#if DEBUG
        if ( metadata == null )
        {
            throw new ArgumentNullException( nameof(metadata) );
        }
#endif

        var logSource = this.ServiceProvider.GetLogSource( metadata.Method.DeclaringType!, LoggingRoles.Caching );

        object? result;

        using ( var activity = logSource.Default.OpenActivity( Formatted( "Processing invocation of method {Method}", metadata.Method ) ) )
        {
            try
            {
                var mergedConfiguration = this.GetMergedMethodConfiguration( metadata );

                if ( !mergedConfiguration.IsEnabled.GetValueOrDefault() )
                {
                    logSource.Debug.IfEnabled?.Write( Formatted( "Ignoring the caching aspect because caching is disabled for this profile." ) );

                    result = func( instance, args );
                }
                else
                {
                    var methodKey = this.KeyBuilder.BuildMethodKey(
                        metadata,
                        args,
                        instance );

                    logSource.Debug.IfEnabled?.Write( Formatted( "Key=\"{Key}\".", methodKey ) );

                    result = this.Frontend.GetOrAdd(
                        metadata.Method,
                        methodKey,
                        metadata.Method.ReturnType,
                        mergedConfiguration,
                        func,
                        instance,
                        args,
                        logSource );
                }

                activity.SetSuccess();
            }
            catch ( Exception e )
            {
                activity.SetException( e );

                throw;
            }
        }

        if ( metadata.ReturnValueCanBeNull )
        {
            return (TResult?) result;
        }
        else
        {
            return result == null ? default : (TResult) result;
        }
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public async Task<TTaskResultType?> GetFromCacheOrExecuteTaskAsync<TTaskResultType>(
        CachedMethodMetadata metadata,
        Func<object?, object?[], CancellationToken, Task<object?>> func,
        object? instance,
        object?[] args,
        CancellationToken cancellationToken )
    {
#if DEBUG
        if ( metadata == null )
        {
            throw new ArgumentNullException( nameof(metadata) );
        }
#endif

        // TODO: What about ConfigureAwait( false )?

        var logSource = this.ServiceProvider.GetLogSource( metadata.Method.DeclaringType!, LoggingRoles.Caching );

        object? result;

        using ( var activity = logSource.Default.OpenAsyncActivity( Formatted( "Processing invocation of async method {Method}", metadata.Method ) ) )
        {
            try
            {
                var mergedConfiguration = this.GetMergedMethodConfiguration( metadata );

                if ( !mergedConfiguration.IsEnabled.GetValueOrDefault() )
                {
                    logSource.Debug.IfEnabled?.Write( Formatted( "Ignoring the caching aspect because caching is disabled for this profile." ) );

                    var task = func( instance, args, cancellationToken );

                    if ( !task.IsCompleted )
                    {
                        // We need to call LogActivity.Suspend and LogActivity.Resume manually because we're in an aspect,
                        // and the await instrumentation policy is not applied.
                        activity.Suspend();

                        try
                        {
                            result = await task;
                        }
                        finally
                        {
                            activity.Resume();
                        }
                    }
                    else
                    {
                        // Don't wrap any exception.
                        result = task.GetAwaiter().GetResult();
                    }
                }
                else
                {
                    var methodKey = Default.KeyBuilder.BuildMethodKey(
                        metadata,
                        args,
                        instance );

                    logSource.Debug.IfEnabled?.Write( Formatted( "Key=\"{Key}\".", methodKey ) );

                    var task = this.Frontend.GetOrAddAsync(
                        metadata.Method,
                        methodKey,
                        metadata.AwaitableResultType!,
                        mergedConfiguration,
                        func,
                        instance,
                        args,
                        logSource,
                        cancellationToken );

                    if ( !task.IsCompleted )
                    {
                        // We need to call LogActivity.Suspend and LogActivity.Resume manually because we're in an aspect,
                        // and the await instrumentation policy is not applied.
                        activity.Suspend();

                        try
                        {
                            result = await task;
                        }
                        finally
                        {
                            activity.Resume();
                        }
                    }
                    else
                    {
                        // Don't wrap any exception.
                        result = task.GetAwaiter().GetResult();
                    }
                }

                activity.SetSuccess();
            }
            catch ( Exception e )
            {
                activity.SetException( e );

                throw;
            }
        }

        if ( metadata.ReturnValueCanBeNull )
        {
            return (TTaskResultType?) result;
        }
        else
        {
            return result == null ? default : (TTaskResultType) result;
        }
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public async ValueTask<TTaskResultType?> GetFromCacheOrExecuteValueTaskAsync<TTaskResultType>(
        CachedMethodMetadata metadata,
        Func<object?, object?[], CancellationToken, ValueTask<object?>> func,
        object? instance,
        object?[] args,
        CancellationToken cancellationToken )
    {
#if DEBUG
        if ( metadata == null )
        {
            throw new ArgumentNullException( nameof(metadata) );
        }
#endif

        // TODO: What about ConfigureAwait( false )?

        var logSource = this.ServiceProvider.GetLogSource( metadata.Method.DeclaringType!, LoggingRoles.Caching );

        object? result;

        // TODO: PostSharp passes an otherwise uninitialized CallerInfo with the CallerAttributes.IsAsync flag set.

        using ( var activity = logSource.Default.OpenAsyncActivity( Formatted( "Processing invocation of async method {Method}", metadata.Method ) ) )
        {
            try
            {
                var mergedConfiguration = this.GetMergedMethodConfiguration( metadata );

                if ( !mergedConfiguration.IsEnabled.GetValueOrDefault() )
                {
                    logSource.Debug.IfEnabled?.Write( Formatted( "Ignoring the caching aspect because caching is disabled for this profile." ) );

                    var task = func( instance, args, cancellationToken );

                    if ( !task.IsCompleted )
                    {
                        // We need to call LogActivity.Suspend and LogActivity.Resume manually because we're in an aspect,
                        // and the await instrumentation policy is not applied.
                        activity.Suspend();

                        try
                        {
                            result = await task;
                        }
                        finally
                        {
                            activity.Resume();
                        }
                    }
                    else
                    {
                        // Don't wrap any exception.
                        result = task.GetAwaiter().GetResult();
                    }
                }
                else
                {
                    var methodKey = Default.KeyBuilder.BuildMethodKey(
                        metadata,
                        args,
                        instance );

                    logSource.Debug.IfEnabled?.Write( Formatted( "Key=\"{Key}\".", methodKey ) );

                    // TODO: Pass CancellationToken (note from original code)

                    var task = this.Frontend.GetOrAddAsync(
                        metadata.Method,
                        methodKey,
                        metadata.AwaitableResultType!,
                        mergedConfiguration,
                        func,
                        instance,
                        args,
                        logSource,
                        cancellationToken );

                    if ( !task.IsCompleted )
                    {
                        // We need to call LogActivity.Suspend and LogActivity.Resume manually because we're in an aspect,
                        // and the await instrumentation policy is not applied.
                        activity.Suspend();

                        try
                        {
                            result = await task;
                        }
                        finally
                        {
                            activity.Resume();
                        }
                    }
                    else
                    {
                        // Don't wrap any exception.
                        result = task.GetAwaiter().GetResult();
                    }
                }

                activity.SetSuccess();
            }
            catch ( Exception e )
            {
                activity.SetException( e );

                throw;
            }
        }

        if ( metadata.ReturnValueCanBeNull )
        {
            return (TTaskResultType?) result;
        }
        else
        {
            return result == null ? default : (TTaskResultType) result;
        }
    }
}