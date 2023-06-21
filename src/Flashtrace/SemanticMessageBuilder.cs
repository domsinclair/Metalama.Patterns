// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Flashtrace.Custom.Messages;
using System.Runtime.CompilerServices;

namespace Flashtrace
{
    /// <summary>
    /// Creates semantic messages composed of a message name and a list of properties given as name-value pairs. These messages are ideal for machine analysis.
    /// For more succinct code, consider including the <c>using static PostSharp.Patterns.Diagnostics.MessageBuilder</c> statement.
    /// </summary>
    public static partial class SemanticMessageBuilder
    {
        /// <summary>
        /// Creates a semantic message with an arbitrary number of parameters.
        /// </summary>
        /// <param name="messageName">Message name.</param>
        /// <param name="parameters">Array of parameters (name-value pairs).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // To avoid copying the struct.
        // Intentionally removing the params modifier because otherwise the C# compiler picks the wrong overload which causes a performance issue.
        public static SemanticMessageArray Semantic(string messageName, ValueTuple<string, object>[] parameters) => new SemanticMessageArray(messageName, parameters);

        /// <summary>
        /// Creates a semantic message with an arbitrary number of parameters.
        /// </summary>
        /// <param name="messageName">Message name.</param>
        /// <param name="parameters">Array of parameters (name-value pairs).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // To avoid copying the struct.
        public static SemanticMessageArray Semantic(string messageName, IReadOnlyList<ValueTuple<string, object>> parameters) => new SemanticMessageArray(messageName, parameters);

        /// <summary>
        /// Creates a semantic message without parameter.
        /// </summary>
        /// <param name="messageName">Message name.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // To avoid copying the struct.
        public static SemanticMessage Semantic( string messageName ) => new( messageName );
    }
}