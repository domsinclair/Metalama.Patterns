﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NETCOREAPP3_0_OR_GREATER
using Metalama.Framework.RunTime;
using Metalama.Patterns.Caching.Implementation;

namespace Metalama.Patterns.Caching.ValueAdapters;

internal sealed class AsyncEnumeratorAdapter<T> : ValueAdapter<IAsyncEnumerator<T>>
{
    public override bool IsAsyncSupported => true;

    public override async Task<object?> GetStoredValueAsync( IAsyncEnumerator<T>? value, CancellationToken cancellationToken )
    {
        if ( value == null )
        {
            return null;
        }

        return await value.BufferToListAsync( cancellationToken );
    }

    public override IAsyncEnumerator<T>? GetExposedValue( object? storedValue ) => ((AsyncEnumerableList<T>?) storedValue)?.GetAsyncEnumerator();

    public override object GetStoredValue( IAsyncEnumerator<T>? value ) => throw new NotSupportedException();
}

#endif