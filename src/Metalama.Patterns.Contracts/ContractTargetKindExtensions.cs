﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System.Globalization;

namespace Metalama.Patterns.Contracts;

[PublicAPI]
public static class ContractTargetKindExtensions
{
    /// <summary>
    /// Gets the display name for the specified <see cref="ContractTargetKind"/>.
    /// </summary>
    public static string GetDisplayName( this ContractTargetKind targetKind )
        => targetKind switch
        {
            ContractTargetKind.Field => "field",
            ContractTargetKind.Property => "property",
            ContractTargetKind.Parameter => "parameter",
            ContractTargetKind.ReturnValue => "return value",
            _ => throw new ArgumentOutOfRangeException( nameof(targetKind) )
        };

    /// <summary>
    /// Gets the display name for the specified <see cref="ContractTargetKind"/> followed by the name of the target declaration when applicable.
    /// </summary>
    public static string GetDisplayName( this ContractTargetKind targetKind, string? targetName )
        => targetKind switch
        {
            ContractTargetKind.Field => string.Format( CultureInfo.InvariantCulture, "field '{0}'", targetName ),
            ContractTargetKind.Property => string.Format( CultureInfo.InvariantCulture, "property '{0}'", targetName ),
            ContractTargetKind.Parameter =>
                string.Format( CultureInfo.InvariantCulture, "parameter '{0}'", targetName ),
            ContractTargetKind.ReturnValue => "return value",
            _ => throw new ArgumentOutOfRangeException( nameof(targetKind) )
        };

    /// <summary>
    /// Gets a name to use for the parameter associated with the target declaration.
    /// </summary>
    public static string GetParameterName( this ContractTargetKind targetKind, string? targetName )
        => targetKind switch
        {
            ContractTargetKind.Property or ContractTargetKind.Field => "value",
            ContractTargetKind.Parameter => targetName ?? string.Empty,
            ContractTargetKind.ReturnValue => "return value",
            _ => throw new ArgumentOutOfRangeException( nameof(targetKind) )
        };
}