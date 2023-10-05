// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Options;

namespace Metalama.Patterns.Contracts;

#pragma warning disable SA1623

/// <summary>
/// Options for all aspects of the <c>Metalama.Patterns.Contracts</c> namespace.
/// </summary>
public sealed class ContractOptions : IHierarchicalOptions<ICompilation>, IHierarchicalOptions<INamespace>, IHierarchicalOptions<INamedType>,
                                      IHierarchicalOptions<IFieldOrPropertyOrIndexer>,
                                      IHierarchicalOptions<IMethod>, IHierarchicalOptions<IParameter>
{
    /// <summary>
    /// Gets or sets the code templates that are used by the aspects. You can set this property to an instance of a class that derives from <see cref="ContractTemplates"/>
    /// and overrides some of the virtual methods.
    /// </summary>
    public ContractTemplates? Templates { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the contracts are inheritable by default. The default value is <c>true</c>. This option can also be configured
    /// for each aspect using the <see cref="ContractBaseAttribute.IsInheritable"/> property of the aspect class.
    /// </summary>
    public bool? IsInheritable { get; init; }

    /// <summary>
    /// Gets or sets the default direction of contracts. The default value is <see cref="ContractDirection.Default"/>, which has a different behavior
    /// according to the declaration to which it is applied. This option can also be configured for each aspect using the <see cref="ContractBaseAttribute.Direction"/>
    /// property of the aspect class.
    /// </summary>
    public ContractDirection? Direction { get; init; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the LAMA5002 warning should be reported when attempting to add a <see cref="NotNullAttribute"/> or
    /// <see cref="RequiredAttribute"/> contract on a nullable parameter, field or property. This option is <c>true</c> by default.
    /// </summary>
    public bool? WarnOnNotNullableOnNullable { get; init; }

    object IIncrementalObject.ApplyChanges( object changes, in ApplyChangesContext context )
    {
        var other = (ContractOptions) changes;

        return new ContractOptions
        {
            Templates = other.Templates ?? this.Templates,
            IsInheritable = other.IsInheritable ?? this.IsInheritable,
            Direction = other.Direction ?? this.Direction,
            WarnOnNotNullableOnNullable = other.WarnOnNotNullableOnNullable ?? this.WarnOnNotNullableOnNullable
        };
    }

    IHierarchicalOptions IHierarchicalOptions.GetDefaultOptions( OptionsInitializationContext context )
        => new ContractOptions { Templates = new ContractTemplates(), IsInheritable = true };
}