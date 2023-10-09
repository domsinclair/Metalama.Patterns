// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Patterns.Contracts;

public sealed class InvariantAttribute : MethodAspect
{
    public override void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        builder.MustNotBeStatic();
        builder.ReturnType().MustBe( typeof(void) );
        builder.MustSatisfy( m => m.Parameters.Count == 0, o => $"{0} must not have any parameters" );
    }

    [Introduce( Accessibility = Accessibility.Protected, IsVirtual = true, WhenExists = OverrideStrategy.Override )]
#pragma warning disable CA1822
    internal void VerifyInvariants()
#pragma warning restore CA1822
    {
        meta.Proceed();

        var invariantMethod = (IMethod) meta.AspectInstance.TargetDeclaration.GetTarget( meta.Target.Compilation );

        invariantMethod.Invoke();
    }

    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        if ( builder.Target.GetContractOptions().AreInvariantsEnabled == false )
        {
            builder.SkipAspect();

            return;
        }
        
        builder.Outbound.Select( x => x.DeclaringType ).RequireAspect<CheckInvariantsAspect>();
    }
}