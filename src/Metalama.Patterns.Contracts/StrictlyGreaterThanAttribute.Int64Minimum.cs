﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#pragma warning disable IDE0004 // Remove Unnecessary Cast: in this problem domain, explicit casts add clarity.

// Resharper disable RedundantCast

using Metalama.Framework.Aspects;

namespace Metalama.Patterns.Contracts;

public partial class StrictlyGreaterThanAttribute
{
    [RunTimeOrCompileTime]
    internal static class Int64Minimum
    {
        public static long ToInt64( long min )
        {
            if ( min == long.MaxValue )
            {
                return long.MaxValue;
            }

            return min + 1;
        }

        public static ulong ToUInt64( long min )
        {
            if ( min < 0 )
            {
                return 0;
            }

            return (ulong) min + 1;
        }

        public static double ToDouble( long min ) => (double) min + FloatingPointHelper.GetDoubleStep( (double) min );

        public static decimal ToDecimal( long min ) => (decimal) min + FloatingPointHelper.GetDecimalStep( (decimal) min );
    }
}