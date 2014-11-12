﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
#if !(NETFX_CORE || NET20)
using System.Runtime.Serialization.Json;
#endif
using OpenGamingLibrary.Json.Serialization;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class WebApiIntegrationTests : TestFixtureBase
    {
        [Fact]
        public void SerializeSerializableType()
        {
            SerializableType serializableType = new SerializableType("protected")
            {
                publicField = "public",
                protectedInternalField = "protected internal",
                internalField = "internal",
                PublicProperty = "private",
                nonSerializedField = "Error"
            };

#if !(NETFX_CORE || NET20 || PORTABLE || ASPNETCORE50 || PORTABLE40)
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(SerializableType));
            dataContractJsonSerializer.WriteObject(ms, serializableType);

            string dtJson = Encoding.UTF8.GetString(ms.ToArray());
            string dtExpected = @"{""internalField"":""internal"",""privateField"":""private"",""protectedField"":""protected"",""protectedInternalField"":""protected internal"",""publicField"":""public""}";

            Assert.Equal(dtExpected, dtJson);
#endif

            string expected = "{\"publicField\":\"public\",\"internalField\":\"internal\",\"protectedInternalField\":\"protected internal\",\"protectedField\":\"protected\",\"privateField\":\"private\"}";
            string json = JsonConvert.SerializeObject(serializableType, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
                    IgnoreSerializableAttribute = false
#endif
                }
            });

            Assert.Equal(expected, json);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void SerializeInheritedType()
        {
            InheritedType serializableType = new InheritedType("protected")
            {
                publicField = "public",
                protectedInternalField = "protected internal",
                internalField = "internal",
                PublicProperty = "private",
                nonSerializedField = "Error",
                inheritedTypeField = "inherited"
            };

            string json = JsonConvert.SerializeObject(serializableType);

            Assert.Equal(@"{""inheritedTypeField"":""inherited"",""publicField"":""public"",""PublicProperty"":""private""}", json);
        }
#endif
    }

    public class InheritedType : SerializableType
    {
        public string inheritedTypeField;

        public InheritedType(string protectedFieldValue) : base(protectedFieldValue)
        {
        }
    }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
    [Serializable]
#else
    [JsonObject(MemberSerialization.Fields)]
#endif
    public class SerializableType : IEquatable<SerializableType>
    {
        public SerializableType(string protectedFieldValue)
        {
            protectedField = protectedFieldValue;
        }

        public string publicField;
        internal string internalField;
        protected internal string protectedInternalField;
        protected string protectedField;
        private string privateField;

        public string PublicProperty
        {
            get { return privateField; }
            set { privateField = value; }
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [NonSerialized]
#else
        [JsonIgnore]
#endif
        public string nonSerializedField;

        public bool Equals(SerializableType other)
        {
            return publicField == other.publicField &&
                   internalField == other.internalField &&
                   protectedInternalField == other.protectedInternalField &&
                   protectedField == other.protectedField &&
                   privateField == other.privateField;
        }
    }
}