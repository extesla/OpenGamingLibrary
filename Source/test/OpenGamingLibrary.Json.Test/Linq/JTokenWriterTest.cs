#region License
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
#if NET40
using System.Numerics;
#endif
using System.Text;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = OpenGamingLibrary.Json.Test.Assert;
#else
using Xunit;
#endif
using OpenGamingLibrary.Json;
using System.IO;
using OpenGamingLibrary.Json.Linq;
#if NET20
using OpenGamingLibrary.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JTokenWriterTest : TestFixtureBase
    {
        [Fact]
        public void ValueFormatting()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world.");

            JToken root;
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue('@');
                jsonWriter.WriteValue("\r\n\t\f\b?{\\r\\n\"\'");
                jsonWriter.WriteValue(true);
                jsonWriter.WriteValue(10);
                jsonWriter.WriteValue(10.99);
                jsonWriter.WriteValue(0.99);
                jsonWriter.WriteValue(0.000000000000000001d);
                jsonWriter.WriteValue(0.000000000000000001m);
                jsonWriter.WriteValue((string)null);
                jsonWriter.WriteValue("This is a string.");
                jsonWriter.WriteNull();
                jsonWriter.WriteUndefined();
                jsonWriter.WriteValue(data);
                jsonWriter.WriteEndArray();

                root = jsonWriter.Token;
            }

            Assert.IsType(typeof(JArray), root);
            Assert.Equal(13, root.Children().Count());
            Assert.Equal("@", (string)root[0]);
            Assert.Equal("\r\n\t\f\b?{\\r\\n\"\'", (string)root[1]);
            Assert.Equal(true, (bool)root[2]);
            Assert.Equal(10, (int)root[3]);
            Assert.Equal(10.99, (double)root[4]);
            Assert.Equal(0.99, (double)root[5]);
            Assert.Equal(0.000000000000000001d, (double)root[6]);
            Assert.Equal(0.000000000000000001m, (decimal)root[7]);
            Assert.Equal(null, (string)root[8]);
            Assert.Equal("This is a string.", (string)root[9]);
            Assert.Equal(null, ((JValue)root[10]).Value);
            Assert.Equal(null, ((JValue)root[11]).Value);
            Assert.Equal(data, (byte[])root[12]);
        }

        [Fact]
        public void State()
        {
            using (JsonWriter jsonWriter = new JTokenWriter())
            {
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);

                jsonWriter.WriteStartObject();
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WritePropertyName("CPU");
                Assert.Equal(WriteState.Property, jsonWriter.WriteState);

                jsonWriter.WriteValue("Intel");
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WritePropertyName("Drives");
                Assert.Equal(WriteState.Property, jsonWriter.WriteState);

                jsonWriter.WriteStartArray();
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);

                jsonWriter.WriteValue("DVD read/writer");
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);

#if NET40
                jsonWriter.WriteValue(new BigInteger(123));
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);
#endif

                jsonWriter.WriteValue(new byte[0]);
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);

                jsonWriter.WriteEnd();
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);

                jsonWriter.WriteEndObject();
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);
            }
        }

        [Fact]
        public void WriteComment()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteComment("fail");
            writer.WriteEndArray();

            StringAssert.Equal(@"[
  /*fail*/]", writer.Token.ToString());
        }

#if NET40
        [Fact]
        public void WriteBigInteger()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteValue(new BigInteger(123));
            writer.WriteEndArray();

            JValue i = (JValue)writer.Token[0];

            Assert.Equal(new BigInteger(123), i.Value);
            Assert.Equal(JTokenType.Integer, i.Type);

            StringAssert.Equal(@"[
  123
]", writer.Token.ToString());
        }
#endif

        [Fact]
        public void WriteRaw()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteRaw("fail");
            writer.WriteRaw("fail");
            writer.WriteEndArray();

            // this is a bug. write raw shouldn't be autocompleting like this
            // hard to fix without introducing Raw and RawValue token types
            // meh
            StringAssert.Equal(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Fact]
        public void WriteRawValue()
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartArray();
            writer.WriteRawValue("fail");
            writer.WriteRawValue("fail");
            writer.WriteEndArray();

            StringAssert.Equal(@"[
  fail,
  fail
]", writer.Token.ToString());
        }

        [Fact]
        public void DateTimeZoneHandling()
        {
            JTokenWriter writer = new JTokenWriter
            {
                DateTimeZoneHandling = Json.DateTimeZoneHandling.Utc
            };

            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

            JValue value = (JValue)writer.Token;
            DateTime dt = (DateTime)value.Value;

            Assert.Equal(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc), dt);
        }
    }
}