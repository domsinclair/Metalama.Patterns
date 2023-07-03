﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Flashtrace.Formatters.UnitTests
{
    internal class NonNullableFormatter<T> : Formatter<T>
        where T : struct
    {
        public NonNullableFormatter( IFormatterRepository repository ) : base( repository ) { }

        public override void Write( UnsafeStringBuilder stringBuilder, T value )
        {
            stringBuilder.Append( '[' );
            stringBuilder.Append( value.ToString() );
            stringBuilder.Append( ']' );
        }
    }
}