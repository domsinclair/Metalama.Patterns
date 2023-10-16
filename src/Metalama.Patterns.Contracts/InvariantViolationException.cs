// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Patterns.Contracts;

public class InvariantViolationException : ApplicationException
{
    public InvariantViolationException() { }

    public InvariantViolationException( string message ) : base( message ) { }

    public InvariantViolationException( string message, Exception innerException ) : base( message, innerException ) { }
}