﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Patterns.Xaml.Implementation;
using Metalama.Patterns.Xaml.Implementation.NamingConvention;

namespace Metalama.Patterns.Xaml;

public sealed partial class CommandAttribute
{
    [CompileTime]
    private readonly struct DiagnosticReporter : IDiagnosticReporter
    {
        public IAspectBuilder<IMethod> Builder { get; init; }

        public void ReportAmbiguousDeclaration( INamingConvention namingConvention, in InspectedDeclaration inspectedDeclaration )
        {
            this.Builder.Diagnostics.Report(
                Diagnostics.WarningValidCandidateDeclarationIsAmbiguous.WithArguments(
                    (
                    inspectedDeclaration.Declaration.DeclarationKind,
                    inspectedDeclaration.Category,
                    "[Command] method ",
                    this.Builder.Target,
                    namingConvention.DiagnosticName
                    ) ),
                inspectedDeclaration.Declaration );
        }

        public void ReportConflictingDeclaration( INamingConvention namingConvention, IDeclaration conflictingDeclaration, IEnumerable<string> applicableCategories )
        {
            this.Builder.Diagnostics.Report(
                Diagnostics.WarningExistingMemberNameConflict.WithArguments(
                    (conflictingDeclaration.DeclarationKind, conflictingDeclaration, this.Builder.Target.DeclaringType, applicableCategories.PrettyList( " or " ), namingConvention.DiagnosticName) ) );
        }

        public void ReportInvalidDeclaration( INamingConvention namingConvention, in InspectedDeclaration inspectedDeclaration )
        {
            this.Builder.Diagnostics.Report(
                Diagnostics.WarningInvalidCandidateDeclarationSignature.WithArguments(
                    (
                    inspectedDeclaration.Declaration.DeclarationKind,
                    inspectedDeclaration.Category,
                    "[Command] method ",
                    this.Builder.Target,
                    namingConvention.DiagnosticName,
                    inspectedDeclaration.Declaration.DeclarationKind == DeclarationKind.Property
                        ? " The property must be of type bool and have a getter."
                        : " The method must not be generic, must return bool and may optionally have a single parameter of any type, but which must not be a ref or out parameter."
                    ) ),
                inspectedDeclaration.Declaration );
        }

        public void ReportDeclarationNotFound( INamingConvention namingConvention, IEnumerable<string> candidateNames, IEnumerable<string> applicableCategories )
        {
            this.Builder.Diagnostics.Report(
                Diagnostics.WarningCandidateNamesNotFound.WithArguments(
                    (
                        applicableCategories.PrettyList( " or " ),
                        namingConvention.DiagnosticName,
                        candidateNames.PrettyList( " or ", '\'' )
                    ) ) );
        }

        public void ReportNoNamingConventionMatched( IEnumerable<INamingConvention> namingConventionsTried )
        {
            this.Builder.Diagnostics.Report(
                Diagnostics.ErrorNoNamingConventionMatched.WithArguments( namingConventionsTried.Select( nc => nc.DiagnosticName ).PrettyList( " and " ) ) );
        }

        public void ReportNoConfiguredNamingConventions()
        {
            this.Builder.Diagnostics.Report( Diagnostics.ErrorNoConfiguredNamingConventions );
        }
    }
}