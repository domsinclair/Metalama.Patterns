﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.Contracts.AspectTests;

public class NotNull_Ineligible_GenericUnconstrained
{
    public void Method<T>( [NotNull] T x )
    {        
    }
}