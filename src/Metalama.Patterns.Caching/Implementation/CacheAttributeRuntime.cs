﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.ComponentModel;
using System.Diagnostics;
using static Flashtrace.FormattedMessageBuilder;

namespace Metalama.Patterns.Caching.Implementation;

[EditorBrowsable( EditorBrowsableState.Never )]
public static class CacheAttributeRunTime
{
    public static TResult? OverrideMethod<TResult>( CachedMethodRegistration registration, object? instance, object?[] args )
    {
        Debug.Assert( registration != null, "registration must not be null." );
        Debug.Assert( registration.InvokeOriginalMethod != null, "registration.InvokeOriginalMethod must not be null" );

        var logSource = registration.Logger;

        object? result;

        // TODO: [Porting] Discuss: We could do this string interpolation at build time, but obfuscation/IL-rewriting could change the method signature before runtime. Best practice?
        using ( var activity = logSource.Default.OpenActivity( Formatted( "Processing invocation of method {Method}", registration.Method ) ) )
        {
            try
            {
                var mergedConfiguration = registration.MergedConfiguration;

                if ( !mergedConfiguration.IsEnabled.GetValueOrDefault() )
                {
                    logSource.Debug.EnabledOrNull?.Write( Formatted( "Ignoring the caching aspect because caching is disabled for this profile." ) );

                    result = registration.InvokeOriginalMethod( instance, args );
                }
                else
                {
                    var methodKey = CachingServices.DefaultKeyBuilder.BuildMethodKey(
                        registration,
                        args,
                        instance );

                    logSource.Debug.EnabledOrNull?.Write( Formatted( "Key=\"{Key}\".", methodKey ) );

                    result = CachingFrontend.GetOrAdd(
                        registration.Method,
                        methodKey,
                        registration.Method.ReturnType,
                        mergedConfiguration,
                        registration.InvokeOriginalMethod,
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

        if ( registration.ReturnValueCanBeNull )
        {
            return (TResult?) result;
        }
        else
        {
            return result == null ? default : (TResult) result;
        }
    }

    public static async Task<TTaskResultType?> OverrideMethodAsync<TTaskResultType>( CachedMethodRegistration registration, object? instance, object?[] args )
    {
        Debug.Assert( registration != null, "registration must not be null." );
        Debug.Assert( registration.InvokeOriginalMethodAsync != null, "registration.InvokeOriginalMethodAsync must not be null" );

        // TODO: What about ConfigureAwait( false )?

        var logSource = registration.Logger;

        object? result;

        // TODO: PostSharp passes an otherwise uninitialzed CallerInfo with the CallerAttributes.IsAsync flag set.

        // TODO: [Porting] Discuss: We could do this string interpolation at build time, but obfuscation/IL-rewriting could change the method signature before runtime. Best practice?
        using ( var activity = logSource.Default.OpenActivity( Formatted( "Processing invocation of async method {Method}", registration.Method ) ) )
        {
            try
            {
                var mergedConfiguration = registration.MergedConfiguration;

                if ( !mergedConfiguration.IsEnabled.GetValueOrDefault() )
                {
                    logSource.Debug.EnabledOrNull?.Write( Formatted( "Ignoring the caching aspect because caching is disabled for this profile." ) );

                    var task = registration.InvokeOriginalMethodAsync( instance, args );

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
                    var methodKey = CachingServices.DefaultKeyBuilder.BuildMethodKey(
                        registration,
                        args,
                        instance );

                    logSource.Debug.EnabledOrNull?.Write( Formatted( "Key=\"{Key}\".", methodKey ) );

                    // TODO: Pass CancellationToken (note from original code)

                    var task = CachingFrontend.GetOrAddAsync(
                        registration.Method,
                        methodKey,
                        registration.Method.ReturnType,
                        mergedConfiguration,
                        registration.InvokeOriginalMethodAsync,
                        instance,
                        args,
                        logSource,
                        CancellationToken.None );

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

        if ( registration.ReturnValueCanBeNull )
        {
            return (TTaskResultType?) result;
        }
        else
        {
            return result == null ? default : (TTaskResultType) result;
        }
    }
}