﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Flashtrace.Formatters;

internal class CovariantTypeExtensionFactory<T> : TypeExtensionFactory<T>
    where T : class
{
    public CovariantTypeExtensionFactory( Type genericInterfaceType, Type converterType )
        : base( genericInterfaceType, converterType ) { }

    protected override IEnumerable<Type> GetAssignableTypes( Type type ) => CovariantTypeExtensionFactoryHelpers.GetAssignableTypes( type );
}