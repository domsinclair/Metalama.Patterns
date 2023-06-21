// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace.Custom;
using System.Diagnostics.CodeAnalysis;

namespace Flashtrace
{
    /// <summary>
    /// Options of the closing methods of the <see cref="LogActivity{TActivityDescription}"/> type.
    /// </summary>
    [SuppressMessage( "Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes" )]
    public struct CloseActivityOptions
    {
        /// <summary>
        /// Initializes a new <see cref="CloseActivityOptions"/> and specifies properties using an arbitrary object 
        /// (possibly of an anonymous class).
        /// </summary>
        /// <param name="data">Optional. Specifies the properties of the <see cref="WriteMessageOptions"/>, typically specified as an instance of a well-known or anonymous CLR type.
        /// The resulting <see cref="LogEventData"/> will have the default <see cref="LogEventMetadata"/>, which means that all CLR properties will be exposed
        /// as logging properties  unless they are annotated with <see cref="LoggingPropertyOptionsAttribute"/>.</param>
        public CloseActivityOptions( object data = null ) : this( LogEventData.Create( data ) ) { }

        /// <summary>
        /// Initializes a new <see cref="CloseActivityOptions"/> and specifies properties using a <see cref="LogEventData"/>.
        /// </summary>
        /// <param name="data">Optional. Specifies the properties of the <see cref="WriteMessageOptions"/>. See <see cref="LogEventData"/>.</param>
        public CloseActivityOptions( in LogEventData data ) : this()
        {
            this.Data = data;
        }

        /// <summary>
        /// Specifies the properties of the <see cref="CloseActivityOptions"/>, typically specified as an instance of a well-known or anonymous CLR type.
        /// </summary>
        public LogEventData Data { get; set; }
    }
}