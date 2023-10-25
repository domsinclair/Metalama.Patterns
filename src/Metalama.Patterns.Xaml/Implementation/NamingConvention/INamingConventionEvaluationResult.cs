﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Patterns.Xaml.Implementation.NamingConvention;

[CompileTime]
internal interface INamingConventionEvaluationResult<TMatch>
    where TMatch : INamingConventionMatch
{
    InspectedNamingConventionMatch<TMatch>? SuccessfulMatch { get; }

    IEnumerable<InspectedNamingConventionMatch<TMatch>>? UnsuccessfulMatches { get; }
}