﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Flashtrace.Formatters;

/// <summary>
/// A formatter for <see cref="byte"/> values.
/// </summary>
public sealed class ByteFormatter : Formatter<byte>
{
    public ByteFormatter( IFormatterRepository repository ) : base( repository )
    {
    }

    /// <inheritdoc />
    public override void Write( UnsafeStringBuilder stringBuilder, byte value )
    {
        stringBuilder.Append( value );
    }
}