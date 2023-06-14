﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// #ExpectedMessage(COM010/Range_InvalidType_Byte.cs[12,26-12,31])

using Metalama.Patterns.Contracts;

public class Range_InvalidType_Byte
{
    [GreaterThan(256)]
    private byte field;
}