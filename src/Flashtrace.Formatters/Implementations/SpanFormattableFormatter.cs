// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace.Formatters.TypeExtensions;

namespace Flashtrace.Formatters.Implementations;

#if NET6_0_OR_GREATER
internal sealed class SpanFormattableFormatter<[BindToExtendedType] TValue> : Formatter<TValue>
    where TValue : ISpanFormattable
{
    public SpanFormattableFormatter( IFormatterRepository repository ) : base( repository ) { }

    public override void Write( UnsafeStringBuilder stringBuilder, TValue? value )
    {
        if ( value == null )
        {
            stringBuilder.Append( 'n', 'u', 'l', 'l' );
        }
        else
        {
            stringBuilder.Append( value );
        }
    }
}

#endif