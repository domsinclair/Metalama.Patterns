﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// #ExpectedMessage(COM010)

using Metalama.Patterns.Contracts;

public class Range_InvalidType_Byte4
{
    [GreaterThan(256.0)]
    private byte field;
}