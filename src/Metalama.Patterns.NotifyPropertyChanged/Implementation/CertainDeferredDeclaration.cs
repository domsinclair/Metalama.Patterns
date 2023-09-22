﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Patterns.NotifyPropertyChanged.Implementation;

/// <summary>
/// A declaration that will certainly be defined later.
/// </summary>
/// <typeparam name="T"></typeparam>
[CompileTime]
internal sealed class CertainDeferredDeclaration<T> : DeferredDeclaration<T>
    where T : IDeclaration
{
    public CertainDeferredDeclaration()
        : base( true ) { }

    [Obsolete( "The value will always be true for " + nameof(CertainDeferredDeclaration<T>) + ", avoid uncessary conditions." )]
    public new bool WillBeDefined => true;

    /// <summary>
    /// Gets or sets the declaration. Template code will always see the final value,
    /// which will not be <see langword="null"/>.
    /// </summary>
    public new T Declaration
    {
        get => base.Declaration!;
        set => base.Declaration = value;
    }

    public static implicit operator T( CertainDeferredDeclaration<T> d ) => d.Declaration;
}