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
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using OpenGamingLibrary.Json;
using System.IO;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Test.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JTokenReaderTest : TestFixtureBase
    {
        [Fact]
        public void ErrorTokenIndex()
        {
            JObject json = JObject.Parse(@"{""IntList"":[1, ""two""]}");

            AssertException.Throws<Exception>(() =>
            {
                JsonSerializer serializer = new JsonSerializer();

                serializer.Deserialize<TraceTestObject>(json.CreateReader());
            }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");
        }

#if !NET20
        [Fact]
        public void YahooFinance()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                IJsonLineInfo lineInfo = jsonReader;

                jsonReader.Read();
                Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
                Assert.Equal(false, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test1", jsonReader.Value);
                Assert.Equal(false, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(JsonToken.Date, jsonReader.TokenType);
                Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
                Assert.Equal(false, lineInfo.HasLineInfo());
                Assert.Equal(0, lineInfo.LinePosition);
                Assert.Equal(0, lineInfo.LineNumber);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test2", jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Date, jsonReader.TokenType);
                Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test3", jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.String, jsonReader.TokenType);
                Assert.Equal("Test3Value", jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test4", jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Null, jsonReader.TokenType);
                Assert.Equal(null, jsonReader.Value);

                Assert.True(jsonReader.Read());
                Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

                Assert.False(jsonReader.Read());
                Assert.Equal(JsonToken.None, jsonReader.TokenType);
            }

            using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
            {
                Assert.True(jsonReader.Read());
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test2", jsonReader.Value);

                Assert.True(jsonReader.Read());
                Assert.Equal(JsonToken.Date, jsonReader.TokenType);
                Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                Assert.False(jsonReader.Read());
                Assert.Equal(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Fact]
        public void ReadAsDateTimeOffsetBadString()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }

        [Fact]
        public void ReadAsDateTimeOffsetBoolean()
        {
            string json = @"{""Offset"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
        }

        [Fact]
        public void ReadAsDateTimeOffsetString()
        {
            string json = @"{""Offset"":""2012-01-24T03:50Z""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
        }
#endif

        [Fact]
        public void ReadLineInfo()
        {
            string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            StringReader sr = new StringReader(input);

            JObject o = JObject.Parse(input);

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                IJsonLineInfo lineInfo = jsonReader;

                Assert.Equal(jsonReader.TokenType, JsonToken.None);
                Assert.Equal(0, lineInfo.LineNumber);
                Assert.Equal(0, lineInfo.LinePosition);
                Assert.Equal(false, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
                Assert.Equal(1, lineInfo.LineNumber);
                Assert.Equal(1, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.Equal(jsonReader.Value, "CPU");
                Assert.Equal(2, lineInfo.LineNumber);
                Assert.Equal(7, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal(jsonReader.Value, "Intel");
                Assert.Equal(2, lineInfo.LineNumber);
                Assert.Equal(15, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.Equal(jsonReader.Value, "Drives");
                Assert.Equal(3, lineInfo.LineNumber);
                Assert.Equal(10, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
                Assert.Equal(3, lineInfo.LineNumber);
                Assert.Equal(12, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal(jsonReader.Value, "DVD read/writer");
                Assert.Equal(4, lineInfo.LineNumber);
                Assert.Equal(22, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
                Assert.Equal(5, lineInfo.LineNumber);
                Assert.Equal(30, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
                Assert.Equal(3, lineInfo.LineNumber);
                Assert.Equal(12, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
                Assert.Equal(1, lineInfo.LineNumber);
                Assert.Equal(1, lineInfo.LinePosition);
                Assert.Equal(true, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.None);
            }
        }

        [Fact]
        public void ReadBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            JObject o =
                new JObject(
                    new JProperty("Test1", data)
                    );

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                jsonReader.Read();
                Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test1", jsonReader.Value);

                byte[] readBytes = jsonReader.ReadAsBytes();
                Assert.Equal(data, readBytes);

                Assert.True(jsonReader.Read());
                Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

                Assert.False(jsonReader.Read());
                Assert.Equal(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Fact]
        public void ReadBytesFailure()
        {
            AssertException.Throws<JsonReaderException>(() =>
            {
                JObject o =
                    new JObject(
                        new JProperty("Test1", 1)
                        );

                using (JTokenReader jsonReader = new JTokenReader(o))
                {
                    jsonReader.Read();
                    Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

                    jsonReader.Read();
                    Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                    Assert.Equal("Test1", jsonReader.Value);

                    jsonReader.ReadAsBytes();
                }
            }, "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
        }

        public class HasBytes
        {
            public byte[] Bytes { get; set; }
        }

        [Fact]
        public void ReadBytesFromString()
        {
            var bytes = new HasBytes { Bytes = new byte[] { 1, 2, 3, 4 } };
            var json = JsonConvert.SerializeObject(bytes);

            TextReader textReader = new StringReader(json);
            JsonReader jsonReader = new JsonTextReader(textReader);

            var jToken = JToken.ReadFrom(jsonReader);

            jsonReader = new JTokenReader(jToken);

            var result2 = (HasBytes)JsonSerializer.Create(null)
                .Deserialize(jsonReader, typeof(HasBytes));

            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result2.Bytes);
        }

        [Fact]
        public void ReadBytesFromEmptyString()
        {
            var bytes = new HasBytes { Bytes = new byte[0] };
            var json = JsonConvert.SerializeObject(bytes);

            TextReader textReader = new StringReader(json);
            JsonReader jsonReader = new JsonTextReader(textReader);

            var jToken = JToken.ReadFrom(jsonReader);

            jsonReader = new JTokenReader(jToken);

            var result2 = (HasBytes)JsonSerializer.Create(null)
                .Deserialize(jsonReader, typeof(HasBytes));

            Assert.Equal(new byte[0], result2.Bytes);
        }

        public class ReadAsBytesTestObject
        {
            public byte[] Data;
        }

        [Fact]
        public void ReadAsBytesNull()
        {
            JsonSerializer s = new JsonSerializer();

            JToken nullToken = JToken.ReadFrom(new JsonTextReader(new StringReader("{ Data: null }")));
            ReadAsBytesTestObject x = s.Deserialize<ReadAsBytesTestObject>(new JTokenReader(nullToken));
            Assert.Null(x.Data);
        }

        [Fact]
        public void DeserializeByteArrayWithTypeNameHandling()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            string json = JsonConvert.SerializeObject(test, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            JObject o = JObject.Parse(json);

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            using (JsonReader nodeReader = o.CreateReader())
            {
                // Get exception here
                TestObject newObject = (TestObject)serializer.Deserialize(nodeReader);

                Assert.Equal("Test", newObject.Name);
                Assert.Equal(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }

        [Fact]
        public void DeserializeStringInt()
        {
            string json = @"{
  ""PreProperty"": ""99"",
  ""PostProperty"": ""-1""
}";

            JObject o = JObject.Parse(json);

            JsonSerializer serializer = new JsonSerializer();

            using (JsonReader nodeReader = o.CreateReader())
            {
                MyClass c = serializer.Deserialize<MyClass>(nodeReader);

                Assert.Equal(99, c.PreProperty);
                Assert.Equal(-1, c.PostProperty);
            }
        }

        [Fact]
        public void ReadAsDecimalInt()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(1m, reader.Value);
        }

        [Fact]
        public void ReadAsInt32Int()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(typeof(int), reader.ValueType);
            Assert.Equal(1, reader.Value);
        }

        [Fact]
        public void ReadAsInt32BadString()
        {
            string json = @"{""Name"":""hi""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
        }

        [Fact]
        public void ReadAsInt32Boolean()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Fact]
        public void ReadAsDecimalString()
        {
            string json = @"{""Name"":""1.1""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(1.1m, reader.Value);
        }

        [Fact]
        public void ReadAsDecimalBadString()
        {
            string json = @"{""Name"":""blah""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
        }

        [Fact]
        public void ReadAsDecimalBoolean()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Fact]
        public void ReadAsDecimalNull()
        {
            string json = @"{""Name"":null}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal(null, reader.ValueType);
            Assert.Equal(null, reader.Value);
        }
    }
}