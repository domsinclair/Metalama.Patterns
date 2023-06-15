﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.Contracts.AspectTests;

public class Range_LongStrictlyGreaterThanUlong
{
    private void MethodWithLongStrictlyGreaterThanUlong( [StrictlyGreaterThan( (ulong) long.MaxValue + 1 )] long? a )
    {
    }
}