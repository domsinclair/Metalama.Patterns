// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace.Formatters;
using JetBrains.Annotations;

namespace Metalama.Patterns.Caching.Formatters;
// TODO: Move to a different namespace because this namespace has only one public type.

/// <summary>
/// The <see cref="FormattingRole"/> for <c>Metalama.Patterns.Caching</c>.
/// </summary>
[PublicAPI]
public sealed class CachingFormattingRole : FormattingRole
{
    public static CachingFormattingRole Instance { get; } = new();

    private CachingFormattingRole() : base( "Caching" ) { }
}