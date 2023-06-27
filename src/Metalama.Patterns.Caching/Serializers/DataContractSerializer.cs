// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace PostSharp.Patterns.Caching.Serializers
{

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
    /// <summary>
    /// An implementation of <see cref="ISerializer"/> that uses <see cref="NetDataContractSerializer"/>.
    /// You can derive this class to use a different <see cref="XmlObjectSerializer"/>.
    /// </summary>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#if !NET_DATA_CONTRACT_SERIALIZER
    [EditorBrowsable(EditorBrowsableState.Never)]
#endif
    public class DataContractSerializer : ISerializer
    {
        /// <summary>
        /// Initializes a new <see cref="DataContractSerializer"/>.
        /// </summary>
        public DataContractSerializer()
        {
#if !NET_DATA_CONTRACT_SERIALIZER
            throw new PlatformNotSupportedException();
#endif
        }


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
        /// <summary>
        /// Creates a new <see cref="XmlObjectSerializer"/>. The default implementation creates a <see cref="NetDataContractSerializer"/>.
        /// </summary>
        /// <returns>A new <see cref="XmlObjectSerializer"/>.</returns>
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#if !NET_DATA_CONTRACT_SERIALIZER
        [EditorBrowsable(EditorBrowsableState.Never)]
#endif
        protected virtual XmlObjectSerializer CreateSerializer()
        {
#if NET_DATA_CONTRACT_SERIALIZER
            return new NetDataContractSerializer();
#else
            throw new PlatformNotSupportedException();
#endif
        }

        /// <inheritdoc />
#if !NET_DATA_CONTRACT_SERIALIZER
        [EditorBrowsable(EditorBrowsableState.Never)]
#endif
        public byte[] Serialize( object value )
        {
#if NET_DATA_CONTRACT_SERIALIZER
            if ( value == null )
#if ARRAY_EMPTY
                return Array.Empty<byte>();
#else
                return new byte[0];
#endif

            XmlObjectSerializer serializer = this.CreateSerializer();

            using ( MemoryStream stream = new MemoryStream() )
            {
                serializer.WriteObject( stream, value );
                return stream.ToArray();
            }
#else
                throw new PlatformNotSupportedException();
#endif
        }

        /// <inheritdoc />
#if !NET_DATA_CONTRACT_SERIALIZER
        [EditorBrowsable(EditorBrowsableState.Never)]
#endif
        public object Deserialize( byte[] array )
        {
#if NET_DATA_CONTRACT_SERIALIZER
            if ( array == null || array.Length == 0 )
                return null;

            XmlObjectSerializer serializer = this.CreateSerializer();

            using ( MemoryStream stream = new MemoryStream( array ) )
            {
                return serializer.ReadObject( stream );
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
