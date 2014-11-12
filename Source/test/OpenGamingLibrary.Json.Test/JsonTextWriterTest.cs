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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;


using OpenGamingLibrary.Json;
using System.IO;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Utilities;

namespace OpenGamingLibrary.Json.Test
{
    
    public class JsonTextWriterTest : TestFixtureBase
    {
        [Fact]
        public void NewLine()
        {
            MemoryStream ms = new MemoryStream();

            using (var streamWriter = new StreamWriter(ms, new UTF8Encoding(false)) { NewLine = "\n" })
            using (var jsonWriter = new JsonTextWriter(streamWriter)
            {
                CloseOutput = true,
                Indentation = 2,
                Formatting = Formatting.Indented
            })
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("prop");
                jsonWriter.WriteValue(true);
                jsonWriter.WriteEndObject();
            }

            byte[] data = ms.ToArray();

            string json = Encoding.UTF8.GetString(data, 0, data.Length);

            Assert.Equal(@"{" + '\n' + @"  ""prop"": true" + '\n' + "}", json);
        }

        [Fact]
        public void QuoteNameAndStrings()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonTextWriter writer = new JsonTextWriter(sw) { QuoteName = false };

            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue("value");

            writer.WriteEndObject();
            writer.Flush();

            Assert.Equal(@"{name:""value""}", sb.ToString());
        }

        [Fact]
        public void CloseOutput()
        {
            MemoryStream ms = new MemoryStream();
            JsonTextWriter writer = new JsonTextWriter(new StreamWriter(ms));

            Assert.True(ms.CanRead);
            writer.Close();
            Assert.False(ms.CanRead);

            ms = new MemoryStream();
            writer = new JsonTextWriter(new StreamWriter(ms)) { CloseOutput = false };

            Assert.True(ms.CanRead);
            writer.Close();
            Assert.True(ms.CanRead);
        }

#if !(PORTABLE || ASPNETCORE50 || NETFX_CORE)
        [Fact]
        public void WriteIConvertable()
        {
            var sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteValue(new ConvertibleInt(1));

            Assert.Equal("1", sw.ToString());
        }
#endif

        [Fact]
        public void ValueFormatting()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
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
                jsonWriter.WriteValue((object)null);
                jsonWriter.WriteValue("This is a string.");
                jsonWriter.WriteNull();
                jsonWriter.WriteUndefined();
                jsonWriter.WriteEndArray();
            }

            string expected = @"[""@"",""\r\n\t\f\b?{\\r\\n\""'"",true,10,10.99,0.99,1E-18,0.000000000000000001,null,null,""This is a string."",null,undefined]";
            string result = sb.ToString();

            Console.WriteLine("ValueFormatting");
            Console.WriteLine(result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NullableValueFormatting()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue((char?)null);
                jsonWriter.WriteValue((char?)'c');
                jsonWriter.WriteValue((bool?)null);
                jsonWriter.WriteValue((bool?)true);
                jsonWriter.WriteValue((byte?)null);
                jsonWriter.WriteValue((byte?)1);
                jsonWriter.WriteValue((sbyte?)null);
                jsonWriter.WriteValue((sbyte?)1);
                jsonWriter.WriteValue((short?)null);
                jsonWriter.WriteValue((short?)1);
                jsonWriter.WriteValue((ushort?)null);
                jsonWriter.WriteValue((ushort?)1);
                jsonWriter.WriteValue((int?)null);
                jsonWriter.WriteValue((int?)1);
                jsonWriter.WriteValue((uint?)null);
                jsonWriter.WriteValue((uint?)1);
                jsonWriter.WriteValue((long?)null);
                jsonWriter.WriteValue((long?)1);
                jsonWriter.WriteValue((ulong?)null);
                jsonWriter.WriteValue((ulong?)1);
                jsonWriter.WriteValue((double?)null);
                jsonWriter.WriteValue((double?)1.1);
                jsonWriter.WriteValue((float?)null);
                jsonWriter.WriteValue((float?)1.1);
                jsonWriter.WriteValue((decimal?)null);
                jsonWriter.WriteValue((decimal?)1.1m);
                jsonWriter.WriteValue((DateTime?)null);
                jsonWriter.WriteValue((DateTime?)new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc));
#if !NET20
                jsonWriter.WriteValue((DateTimeOffset?)null);
                jsonWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero));
#endif
                jsonWriter.WriteEndArray();
            }

            string json = sw.ToString();
            string expected;

#if !NET20
            expected = @"[null,""c"",null,true,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1.1,null,1.1,null,1.1,null,""1970-01-01T00:00:00Z"",null,""1970-01-01T00:00:00+00:00""]";
#else
      expected = @"[null,""c"",null,true,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1.1,null,1.1,null,1.1,null,""1970-01-01T00:00:00Z""]";
#endif

            Assert.Equal(expected, json);
        }

        [Fact]
        public void WriteValueObjectWithNullable()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                char? value = 'c';

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue((object)value);
                jsonWriter.WriteEndArray();
            }

            string json = sw.ToString();
            string expected = @"[""c""]";

            Assert.Equal(expected, json);
        }

        [Fact]
        public void WriteValueObjectWithUnsupportedValue()
        {
            AssertException.Throws<JsonWriterException>(() =>
            {
                StringWriter sw = new StringWriter();
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.WriteStartArray();
                    jsonWriter.WriteValue(new Version(1, 1, 1, 1));
                    jsonWriter.WriteEndArray();
                }
            }, @"Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation. Path ''.");
        }

        [Fact]
        public void StringEscaping()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(@"""These pretzels are making me thirsty!""");
                jsonWriter.WriteValue("Jeff's house was burninated.");
                jsonWriter.WriteValue("1. You don't talk about fight club.\r\n2. You don't talk about fight club.");
                jsonWriter.WriteValue("35% of\t statistics\n are made\r up.");
                jsonWriter.WriteEndArray();
            }

            string expected = @"[""\""These pretzels are making me thirsty!\"""",""Jeff's house was burninated."",""1. You don't talk about fight club.\r\n2. You don't talk about fight club."",""35% of\t statistics\n are made\r up.""]";
            string result = sb.ToString();

            Console.WriteLine("StringEscaping");
            Console.WriteLine(result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WriteEnd()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("CPU");
                jsonWriter.WriteValue("Intel");
                jsonWriter.WritePropertyName("PSU");
                jsonWriter.WriteValue("500W");
                jsonWriter.WritePropertyName("Drives");
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue("DVD read/writer");
                jsonWriter.WriteComment("(broken)");
                jsonWriter.WriteValue("500 gigabyte hard drive");
                jsonWriter.WriteValue("200 gigabype hard drive");
                jsonWriter.WriteEndObject();
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);
            }

            string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void CloseWithRemainingContent()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("CPU");
                jsonWriter.WriteValue("Intel");
                jsonWriter.WritePropertyName("PSU");
                jsonWriter.WriteValue("500W");
                jsonWriter.WritePropertyName("Drives");
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue("DVD read/writer");
                jsonWriter.WriteComment("(broken)");
                jsonWriter.WriteValue("500 gigabyte hard drive");
                jsonWriter.WriteValue("200 gigabype hard drive");
                jsonWriter.Close();
            }

            string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void Indenting()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("CPU");
                jsonWriter.WriteValue("Intel");
                jsonWriter.WritePropertyName("PSU");
                jsonWriter.WriteValue("500W");
                jsonWriter.WritePropertyName("Drives");
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue("DVD read/writer");
                jsonWriter.WriteComment("(broken)");
                jsonWriter.WriteValue("500 gigabyte hard drive");
                jsonWriter.WriteValue("200 gigabype hard drive");
                jsonWriter.WriteEnd();
                jsonWriter.WriteEndObject();
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);
            }

            // {
            //   "CPU": "Intel",
            //   "PSU": "500W",
            //   "Drives": [
            //     "DVD read/writer"
            //     /*(broken)*/,
            //     "500 gigabyte hard drive",
            //     "200 gigabype hard drive"
            //   ]
            // }

            string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void State()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);

                jsonWriter.WriteStartObject();
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);
                Assert.Equal("", jsonWriter.Path);

                jsonWriter.WritePropertyName("CPU");
                Assert.Equal(WriteState.Property, jsonWriter.WriteState);
                Assert.Equal("CPU", jsonWriter.Path);

                jsonWriter.WriteValue("Intel");
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);
                Assert.Equal("CPU", jsonWriter.Path);

                jsonWriter.WritePropertyName("Drives");
                Assert.Equal(WriteState.Property, jsonWriter.WriteState);
                Assert.Equal("Drives", jsonWriter.Path);

                jsonWriter.WriteStartArray();
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);

                jsonWriter.WriteValue("DVD read/writer");
                Assert.Equal(WriteState.Array, jsonWriter.WriteState);
                Assert.Equal("Drives[0]", jsonWriter.Path);

                jsonWriter.WriteEnd();
                Assert.Equal(WriteState.Object, jsonWriter.WriteState);
                Assert.Equal("Drives", jsonWriter.Path);

                jsonWriter.WriteEndObject();
                Assert.Equal(WriteState.Start, jsonWriter.WriteState);
                Assert.Equal("", jsonWriter.Path);
            }
        }

        [Fact]
        public void FloatingPointNonFiniteNumbers_Symbol()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteValue(double.PositiveInfinity);
                jsonWriter.WriteValue(double.NegativeInfinity);
                jsonWriter.WriteValue(float.NaN);
                jsonWriter.WriteValue(float.PositiveInfinity);
                jsonWriter.WriteValue(float.NegativeInfinity);
                jsonWriter.WriteEndArray();

                jsonWriter.Flush();
            }

            string expected = @"[
  NaN,
  Infinity,
  -Infinity,
  NaN,
  Infinity,
  -Infinity
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void FloatingPointNonFiniteNumbers_Zero()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.DefaultValue;

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteValue(double.PositiveInfinity);
                jsonWriter.WriteValue(double.NegativeInfinity);
                jsonWriter.WriteValue(float.NaN);
                jsonWriter.WriteValue(float.PositiveInfinity);
                jsonWriter.WriteValue(float.NegativeInfinity);
                jsonWriter.WriteValue((double?)double.NaN);
                jsonWriter.WriteValue((double?)double.PositiveInfinity);
                jsonWriter.WriteValue((double?)double.NegativeInfinity);
                jsonWriter.WriteValue((float?)float.NaN);
                jsonWriter.WriteValue((float?)float.PositiveInfinity);
                jsonWriter.WriteValue((float?)float.NegativeInfinity);
                jsonWriter.WriteEndArray();

                jsonWriter.Flush();
            }

            string expected = @"[
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  null,
  null,
  null,
  null,
  null,
  null
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void FloatingPointNonFiniteNumbers_String()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.String;

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteValue(double.PositiveInfinity);
                jsonWriter.WriteValue(double.NegativeInfinity);
                jsonWriter.WriteValue(float.NaN);
                jsonWriter.WriteValue(float.PositiveInfinity);
                jsonWriter.WriteValue(float.NegativeInfinity);
                jsonWriter.WriteEndArray();

                jsonWriter.Flush();
            }

            string expected = @"[
  ""NaN"",
  ""Infinity"",
  ""-Infinity"",
  ""NaN"",
  ""Infinity"",
  ""-Infinity""
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void FloatingPointNonFiniteNumbers_QuoteChar()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.String;
                jsonWriter.QuoteChar = '\'';

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteValue(double.PositiveInfinity);
                jsonWriter.WriteValue(double.NegativeInfinity);
                jsonWriter.WriteValue(float.NaN);
                jsonWriter.WriteValue(float.PositiveInfinity);
                jsonWriter.WriteValue(float.NegativeInfinity);
                jsonWriter.WriteEndArray();

                jsonWriter.Flush();
            }

            string expected = @"[
  'NaN',
  'Infinity',
  '-Infinity',
  'NaN',
  'Infinity',
  '-Infinity'
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void WriteRawInStart()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

                jsonWriter.WriteRaw("[1,2,3,4,5]");
                jsonWriter.WriteWhitespace("  ");
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteEndArray();
            }

            string expected = @"[1,2,3,4,5]  [
  NaN
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void WriteRawInArray()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteRaw(",[1,2,3,4,5]");
                jsonWriter.WriteRaw(",[1,2,3,4,5]");
                jsonWriter.WriteValue(float.NaN);
                jsonWriter.WriteEndArray();
            }

            string expected = @"[
  NaN,[1,2,3,4,5],[1,2,3,4,5],
  NaN
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void WriteRawInObject()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                jsonWriter.WriteRaw(@"""PropertyName"":[1,2,3,4,5]");
                jsonWriter.WriteEnd();
            }

            string expected = @"{""PropertyName"":[1,2,3,4,5]}";
            string result = sb.ToString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WriteToken()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
            reader.Read();
            reader.Read();

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteToken(reader);

            Assert.Equal("1", sw.ToString());
        }

        [Fact]
        public void WriteRawValue()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                int i = 0;
                string rawJson = "[1,2]";

                jsonWriter.WriteStartObject();

                while (i < 3)
                {
                    jsonWriter.WritePropertyName("d" + i);
                    jsonWriter.WriteRawValue(rawJson);

                    i++;
                }

                jsonWriter.WriteEndObject();
            }

            Assert.Equal(@"{""d0"":[1,2],""d1"":[1,2],""d2"":[1,2]}", sb.ToString());
        }

        [Fact]
        public void WriteObjectNestedInConstructor()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("con");

                jsonWriter.WriteStartConstructor("Ext.data.JsonStore");
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("aa");
                jsonWriter.WriteValue("aa");
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndConstructor();

                jsonWriter.WriteEndObject();
            }

            Assert.Equal(@"{""con"":new Ext.data.JsonStore({""aa"":""aa""})}", sb.ToString());
        }

        [Fact]
        public void WriteFloatingPointNumber()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

                jsonWriter.WriteStartArray();

                jsonWriter.WriteValue(0.0);
                jsonWriter.WriteValue(0f);
                jsonWriter.WriteValue(0.1);
                jsonWriter.WriteValue(1.0);
                jsonWriter.WriteValue(1.000001);
                jsonWriter.WriteValue(0.000001);
                jsonWriter.WriteValue(double.Epsilon);
                jsonWriter.WriteValue(double.PositiveInfinity);
                jsonWriter.WriteValue(double.NegativeInfinity);
                jsonWriter.WriteValue(double.NaN);
                jsonWriter.WriteValue(double.MaxValue);
                jsonWriter.WriteValue(double.MinValue);
                jsonWriter.WriteValue(float.PositiveInfinity);
                jsonWriter.WriteValue(float.NegativeInfinity);
                jsonWriter.WriteValue(float.NaN);

                jsonWriter.WriteEndArray();
            }

            Assert.Equal(@"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN]", sb.ToString());
        }

        [Fact]
        public void WriteIntegerNumber()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                jsonWriter.WriteStartArray();

                jsonWriter.WriteValue(int.MaxValue);
                jsonWriter.WriteValue(int.MinValue);
                jsonWriter.WriteValue(0);
                jsonWriter.WriteValue(-0);
                jsonWriter.WriteValue(9L);
                jsonWriter.WriteValue(9UL);
                jsonWriter.WriteValue(long.MaxValue);
                jsonWriter.WriteValue(long.MinValue);
                jsonWriter.WriteValue(ulong.MaxValue);
                jsonWriter.WriteValue(ulong.MinValue);

                jsonWriter.WriteEndArray();
            }

            Console.WriteLine(sb.ToString());

            StringAssert.Equal(@"[
  2147483647,
  -2147483648,
  0,
  0,
  9,
  9,
  9223372036854775807,
  -9223372036854775808,
  18446744073709551615,
  0
]", sb.ToString());
        }

        [Fact]
        public void BadWriteEndArray()
        {
            AssertException.Throws<JsonWriterException>(() =>
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                using (JsonWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.WriteStartArray();

                    jsonWriter.WriteValue(0.0);

                    jsonWriter.WriteEndArray();
                    jsonWriter.WriteEndArray();
                }
            }, "No token to close. Path ''.");
        }

        [Fact]
        public void InvalidQuoteChar()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.QuoteChar = '*';
                }
            }, @"Invalid JavaScript string quote character. Valid quote characters are ' and "".");
        }

        [Fact]
        public void Indentation()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

                Assert.Equal(Formatting.Indented, jsonWriter.Formatting);

                jsonWriter.Indentation = 5;
                Assert.Equal(5, jsonWriter.Indentation);
                jsonWriter.IndentChar = '_';
                Assert.Equal('_', jsonWriter.IndentChar);
                jsonWriter.QuoteName = true;
                Assert.Equal(true, jsonWriter.QuoteName);
                jsonWriter.QuoteChar = '\'';
                Assert.Equal('\'', jsonWriter.QuoteChar);

                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("propertyName");
                jsonWriter.WriteValue(double.NaN);

                jsonWriter.IndentChar = '?';
                Assert.Equal('?', jsonWriter.IndentChar);
                jsonWriter.Indentation = 6;
                Assert.Equal(6, jsonWriter.Indentation);

                jsonWriter.WritePropertyName("prop2");
                jsonWriter.WriteValue(123);

                jsonWriter.WriteEndObject();
            }

            string expected = @"{
_____'propertyName': NaN,
??????'prop2': 123
}";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void WriteSingleBytes()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            string text = "Hello world.";
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                Assert.Equal(Formatting.Indented, jsonWriter.Formatting);

                jsonWriter.WriteValue(data);
            }

            string expected = @"""SGVsbG8gd29ybGQu""";
            string result = sb.ToString();

            Assert.Equal(expected, result);

            byte[] d2 = Convert.FromBase64String(result.Trim('"'));

            Assert.Equal(text, Encoding.UTF8.GetString(d2, 0, d2.Length));
        }

        [Fact]
        public void WriteBytesInArray()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            string text = "Hello world.";
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                Assert.Equal(Formatting.Indented, jsonWriter.Formatting);

                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(data);
                jsonWriter.WriteValue(data);
                jsonWriter.WriteValue((object)data);
                jsonWriter.WriteValue((byte[])null);
                jsonWriter.WriteValue((Uri)null);
                jsonWriter.WriteEndArray();
            }

            string expected = @"[
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  null,
  null
]";
            string result = sb.ToString();

            StringAssert.Equal(expected, result);
        }

        [Fact]
        public void Path()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            string text = "Hello world.";
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartArray();
                Assert.Equal("", writer.Path);
                writer.WriteStartObject();
                Assert.Equal("[0]", writer.Path);
                writer.WritePropertyName("Property1");
                Assert.Equal("[0].Property1", writer.Path);
                writer.WriteStartArray();
                Assert.Equal("[0].Property1", writer.Path);
                writer.WriteValue(1);
                Assert.Equal("[0].Property1[0]", writer.Path);
                writer.WriteStartArray();
                Assert.Equal("[0].Property1[1]", writer.Path);
                writer.WriteStartArray();
                Assert.Equal("[0].Property1[1][0]", writer.Path);
                writer.WriteStartArray();
                Assert.Equal("[0].Property1[1][0][0]", writer.Path);
                writer.WriteEndObject();
                Assert.Equal("[0]", writer.Path);
                writer.WriteStartObject();
                Assert.Equal("[1]", writer.Path);
                writer.WritePropertyName("Property2");
                Assert.Equal("[1].Property2", writer.Path);
                writer.WriteStartConstructor("Constructor1");
                Assert.Equal("[1].Property2", writer.Path);
                writer.WriteNull();
                Assert.Equal("[1].Property2[0]", writer.Path);
                writer.WriteStartArray();
                Assert.Equal("[1].Property2[1]", writer.Path);
                writer.WriteValue(1);
                Assert.Equal("[1].Property2[1][0]", writer.Path);
                writer.WriteEnd();
                Assert.Equal("[1].Property2[1]", writer.Path);
                writer.WriteEndObject();
                Assert.Equal("[1]", writer.Path);
                writer.WriteEndArray();
                Assert.Equal("", writer.Path);
            }

            StringAssert.Equal(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]", sb.ToString());
        }

        [Fact]
        public void BuildStateArray()
        {
            JsonWriter.State[][] stateArray = JsonWriter.BuildStateArray();

            var valueStates = JsonWriter.StateArrayTempate[7];

            foreach (JsonToken valueToken in EnumUtils.GetValues(typeof(JsonToken)))
            {
                switch (valueToken)
                {
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        Assert.Equal(valueStates, stateArray[(int)valueToken]);
                        break;
                }
            }
        }

        [Fact]
        public void DateTimeZoneHandling()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw)
            {
                DateTimeZoneHandling = Json.DateTimeZoneHandling.Utc
            };

            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

            Assert.Equal(@"""2000-01-01T01:01:01Z""", sw.ToString());
        }

        [Fact]
        public void HtmlStringEscapeHandling()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw)
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            string script = @"<script type=""text/javascript"">alert('hi');</script>";

            writer.WriteValue(script);

            string json = sw.ToString();

            Assert.Equal(@"""\u003cscript type=\u0022text/javascript\u0022\u003ealert(\u0027hi\u0027);\u003c/script\u003e""", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.Equal(script, reader.ReadAsString());

            //Console.WriteLine(HttpUtility.HtmlEncode(script));

            //System.Web.Script.Serialization.JavaScriptSerializer s = new System.Web.Script.Serialization.JavaScriptSerializer();
            //Console.WriteLine(s.Serialize(new { html = script }));
        }

        [Fact]
        public void NonAsciiStringEscapeHandling()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw)
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };

            string unicode = "\u5f20";

            writer.WriteValue(unicode);

            string json = sw.ToString();

            Assert.Equal(8, json.Length);
            Assert.Equal(@"""\u5f20""", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.Equal(unicode, reader.ReadAsString());

            sw = new StringWriter();
            writer = new JsonTextWriter(sw)
            {
                StringEscapeHandling = StringEscapeHandling.Default
            };

            writer.WriteValue(unicode);

            json = sw.ToString();

            Assert.Equal(3, json.Length);
            Assert.Equal("\"\u5f20\"", json);
        }

        [Fact]
        public void WriteEndOnProperty()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.QuoteChar = '\'';

            writer.WriteStartObject();
            writer.WritePropertyName("Blah");
            writer.WriteEnd();

            Assert.Equal("{'Blah':null}", sw.ToString());
        }

#if !NET20
        [Fact]
        public void QuoteChar()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            writer.QuoteChar = '\'';

            writer.WriteStartArray();

            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

            writer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

            writer.DateFormatString = "yyyy gg";
            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

            writer.WriteValue(new byte[] { 1, 2, 3 });
            writer.WriteValue(TimeSpan.Zero);
            writer.WriteValue(new Uri("http://www.google.com/"));
            writer.WriteValue(Guid.Empty);

            writer.WriteEnd();

            StringAssert.Equal(@"[
  '2000-01-01T01:01:01Z',
  '2000-01-01T01:01:01+00:00',
  '\/Date(946688461000)\/',
  '\/Date(946688461000+0000)\/',
  '2000 A.D.',
  '2000 A.D.',
  'AQID',
  '00:00:00',
  'http://www.google.com/',
  '00000000-0000-0000-0000-000000000000'
]", sw.ToString());
        }

        [Fact]
        public void Culture()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            writer.DateFormatString = "yyyy tt";
            writer.Culture = new CultureInfo("en-NZ");
            writer.QuoteChar = '\'';

            writer.WriteStartArray();

            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

            writer.WriteEnd();

            StringAssert.Equal(@"[
  '2000 a.m.',
  '2000 a.m.'
]", sw.ToString());
        }
#endif

        [Fact]
        public void CompareNewStringEscapingWithOld()
        {
            Console.WriteLine("Started");

            char c = (char)0;

            do
            {
                if (c % 1000 == 0)
                    Console.WriteLine("Position: " + (int)c);

                StringWriter swNew = new StringWriter();
                char[] buffer = null;
                JavaScriptUtils.WriteEscapedJavaScriptString(swNew, c.ToString(), '"', true, JavaScriptUtils.DoubleQuoteCharEscapeFlags, StringEscapeHandling.Default, ref buffer);

                StringWriter swOld = new StringWriter();
                WriteEscapedJavaScriptStringOld(swOld, c.ToString(), '"', true);

                string newText = swNew.ToString();
                string oldText = swOld.ToString();

                if (newText != oldText)
                    throw new Exception("Difference for char '{0}' (value {1}). Old text: {2}, New text: {3}".FormatWith(CultureInfo.InvariantCulture, c, (int)c, oldText, newText));

                c++;
            } while (c != char.MaxValue);

            Console.WriteLine("Finished");
        }

        private const string EscapedUnicodeText = "!";

        private static void WriteEscapedJavaScriptStringOld(TextWriter writer, string s, char delimiter, bool appendDelimiters)
        {
            // leading delimiter
            if (appendDelimiters)
                writer.Write(delimiter);

            if (s != null)
            {
                char[] chars = null;
                char[] unicodeBuffer = null;
                int lastWritePosition = 0;

                for (int i = 0; i < s.Length; i++)
                {
                    var c = s[i];

                    // don't escape standard text/numbers except '\' and the text delimiter
                    if (c >= ' ' && c < 128 && c != '\\' && c != delimiter)
                        continue;

                    string escapedValue;

                    switch (c)
                    {
                        case '\t':
                            escapedValue = @"\t";
                            break;
                        case '\n':
                            escapedValue = @"\n";
                            break;
                        case '\r':
                            escapedValue = @"\r";
                            break;
                        case '\f':
                            escapedValue = @"\f";
                            break;
                        case '\b':
                            escapedValue = @"\b";
                            break;
                        case '\\':
                            escapedValue = @"\\";
                            break;
                        case '\u0085': // Next Line
                            escapedValue = @"\u0085";
                            break;
                        case '\u2028': // Line Separator
                            escapedValue = @"\u2028";
                            break;
                        case '\u2029': // Paragraph Separator
                            escapedValue = @"\u2029";
                            break;
                        case '\'':
                            // this charater is being used as the delimiter
                            escapedValue = @"\'";
                            break;
                        case '"':
                            // this charater is being used as the delimiter
                            escapedValue = "\\\"";
                            break;
                        default:
                            if (c <= '\u001f')
                            {
                                if (unicodeBuffer == null)
                                    unicodeBuffer = new char[6];

                                StringUtils.ToCharAsUnicode(c, unicodeBuffer);

                                // slightly hacky but it saves multiple conditions in if test
                                escapedValue = EscapedUnicodeText;
                            }
                            else
                            {
                                escapedValue = null;
                            }
                            break;
                    }

                    if (escapedValue == null)
                        continue;

                    if (i > lastWritePosition)
                    {
                        if (chars == null)
                            chars = s.ToCharArray();

                        // write unchanged chars before writing escaped text
                        writer.Write(chars, lastWritePosition, i - lastWritePosition);
                    }

                    lastWritePosition = i + 1;
                    if (!string.Equals(escapedValue, EscapedUnicodeText))
                        writer.Write(escapedValue);
                    else
                        writer.Write(unicodeBuffer);
                }

                if (lastWritePosition == 0)
                {
                    // no escaped text, write entire string
                    writer.Write(s);
                }
                else
                {
                    if (chars == null)
                        chars = s.ToCharArray();

                    // write remaining text
                    writer.Write(chars, lastWritePosition, s.Length - lastWritePosition);
                }
            }

            // trailing delimiter
            if (appendDelimiters)
                writer.Write(delimiter);
        }

        [Fact]
        public void CustomJsonTextWriterTests()
        {
            StringWriter sw = new StringWriter();
            CustomJsonTextWriter writer = new CustomJsonTextWriter(sw) { Formatting = Formatting.Indented };
            writer.WriteStartObject();
            Assert.Equal(WriteState.Object, writer.WriteState);
            writer.WritePropertyName("Property1");
            Assert.Equal(WriteState.Property, writer.WriteState);
            Assert.Equal("Property1", writer.Path);
            writer.WriteNull();
            Assert.Equal(WriteState.Object, writer.WriteState);
            writer.WriteEndObject();
            Assert.Equal(WriteState.Start, writer.WriteState);

            StringAssert.Equal(@"{{{
  ""1ytreporP"": NULL!!!
}}}", sw.ToString());
        }

        [Fact]
        public void QuoteDictionaryNames()
        {
            var d = new Dictionary<string, int>
            {
                { "a", 1 },
            };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var serializer = JsonSerializer.Create(jsonSerializerSettings);
            using (var stringWriter = new StringWriter())
            {
                using (var writer = new JsonTextWriter(stringWriter) { QuoteName = false })
                {
                    serializer.Serialize(writer, d);
                    writer.Close();
                }

                StringAssert.Equal(@"{
  a: 1
}", stringWriter.ToString());
            }
        }

        [Fact]
        public void WriteComments()
        {
            string json = @"//comment*//*hi*/
{//comment
Name://comment
true//comment after true" + StringUtils.CarriageReturn + @"
,//comment after comma" + StringUtils.CarriageReturnLineFeed + @"
""ExpiryDate""://comment" + StringUtils.LineFeed + @"
new
" + StringUtils.LineFeed +
                  @"Constructor
(//comment
null//comment
),
        ""Price"": 3.99,
        ""Sizes"": //comment
[//comment

          ""Small""//comment
]//comment
}//comment 
//comment 1 ";

            JsonTextReader r = new JsonTextReader(new StringReader(json));

            StringWriter sw = new StringWriter();
            JsonTextWriter w = new JsonTextWriter(sw);
            w.Formatting = Formatting.Indented;

            w.WriteToken(r, true);

            StringAssert.Equal(@"/*comment*//*hi*/*/{/*comment*/
  ""Name"": /*comment*/ true/*comment after true*//*comment after comma*/,
  ""ExpiryDate"": /*comment*/ new Constructor(
    /*comment*/,
    null
    /*comment*/
  ),
  ""Price"": 3.99,
  ""Sizes"": /*comment*/ [
    /*comment*/
    ""Small""
    /*comment*/
  ]/*comment*/
}/*comment *//*comment 1 */", sw.ToString());
        }
    }

    public class CustomJsonTextWriter : JsonTextWriter
    {
        private readonly TextWriter _writer;

        public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter)
        {
            _writer = textWriter;
        }

        public override void WritePropertyName(string name)
        {
            WritePropertyName(name, true);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            SetWriteState(JsonToken.PropertyName, name);

            if (QuoteName)
                _writer.Write(QuoteChar);

            _writer.Write(new string(name.ToCharArray().Reverse().ToArray()));

            if (QuoteName)
                _writer.Write(QuoteChar);

            _writer.Write(':');
        }

        public override void WriteNull()
        {
            SetWriteState(JsonToken.Null, null);

            _writer.Write("NULL!!!");
        }

        public override void WriteStartObject()
        {
            SetWriteState(JsonToken.StartObject, null);

            _writer.Write("{{{");
        }

        public override void WriteEndObject()
        {
            SetWriteState(JsonToken.EndObject, null);
        }

        protected override void WriteEnd(JsonToken token)
        {
            if (token == JsonToken.EndObject)
                _writer.Write("}}}");
            else
                base.WriteEnd(token);
        }
    }

#if !(PORTABLE || ASPNETCORE50 || NETFX_CORE)
    public struct ConvertibleInt : IConvertible
    {
        private readonly int _value;

        public ConvertibleInt(int value)
        {
            _value = value;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(int))
                return _value;

            throw new Exception("Type not supported: " + conversionType.FullName);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
#endif
}