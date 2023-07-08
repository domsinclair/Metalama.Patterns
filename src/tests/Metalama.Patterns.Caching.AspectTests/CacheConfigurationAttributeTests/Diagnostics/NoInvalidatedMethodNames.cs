﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// @RemoveOutputCode

namespace Metalama.Patterns.Caching.AspectTests.CacheConfigurationAttributeTests.Diagnostics;

public class NoInvalidatedMethodNames
{
    [InvalidateCache()]
    public int Test( int a ) => 42;
}