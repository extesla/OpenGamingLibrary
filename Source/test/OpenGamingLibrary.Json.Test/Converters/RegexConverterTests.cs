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
using System.IO;
using System.Text.RegularExpressions;
using OpenGamingLibrary.Json.Bson;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Utilities;
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
using OpenGamingLibrary.Json.Test.TestObjects;

namespace OpenGamingLibrary.Json.Test.Converters
{
    
    public class RegexConverterTests : TestFixtureBase
    {
        public class RegexTestClass
        {
            public Regex Regex { get; set; }
        }

        [Fact]
        public void SerializeToText()
        {
            Regex regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            string json = JsonConvert.SerializeObject(regex, Formatting.Indented, new RegexConverter());

            StringAssert.Equal(@"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}", json);
        }

        [Fact]
        public void SerializeCamelCaseAndStringEnums()
        {
            Regex regex = new Regex("abc", RegexOptions.IgnoreCase);

            string json = JsonConvert.SerializeObject(regex, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = { new RegexConverter(), new StringEnumConverter() { CamelCaseText = true } },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            StringAssert.Equal(@"{
  ""pattern"": ""abc"",
  ""options"": ""ignoreCase""
}", json);
        }

        [Fact]
        public void DeserializeCamelCaseAndStringEnums()
        {
            string json = @"{
  ""pattern"": ""abc"",
  ""options"": ""ignoreCase""
}";

            Regex regex = JsonConvert.DeserializeObject<Regex>(json, new JsonSerializerSettings
            {
                Converters = { new RegexConverter() }
            });

            Assert.Equal("abc", regex.ToString());
            Assert.Equal(RegexOptions.IgnoreCase, regex.Options);
        }

        [Fact]
        public void DeserializeISerializeRegexJson()
        {
            string json = @"{
                        ""Regex"": {
                          ""pattern"": ""(hi)"",
                          ""options"": 5,
                          ""matchTimeout"": -10000
                        }
                      }";

            RegexTestClass r = JsonConvert.DeserializeObject<RegexTestClass>(json);

            Assert.Equal("(hi)", r.Regex.ToString());
            Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, r.Regex.Options);
        }

        [Fact]
        public void SerializeToBson()
        {
            Regex regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new RegexConverter());

            serializer.Serialize(writer, new RegexTestClass { Regex = regex });

            string expected = "13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00";
            string bson = BytesToHex(ms.ToArray());

            Assert.Equal(expected, bson);
        }

        [Fact]
        public void DeserializeFromText()
        {
            string json = @"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}";

            Regex newRegex = JsonConvert.DeserializeObject<Regex>(json, new RegexConverter());
            Assert.Equal("abc", newRegex.ToString());
            Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, newRegex.Options);
        }

        [Fact]
        public void DeserializeFromBson()
        {
            MemoryStream ms = new MemoryStream(HexToBytes("13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00"));
            BsonReader reader = new BsonReader(ms);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new RegexConverter());

            RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

            Assert.Equal("abc", c.Regex.ToString());
            Assert.Equal(RegexOptions.IgnoreCase, c.Regex.Options);
        }

        [Fact]
        public void ConvertEmptyRegexBson()
        {
            Regex regex = new Regex(string.Empty);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new RegexConverter());

            serializer.Serialize(writer, new RegexTestClass { Regex = regex });

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);
            serializer.Converters.Add(new RegexConverter());

            RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

            Assert.Equal("", c.Regex.ToString());
            Assert.Equal(RegexOptions.None, c.Regex.Options);
        }

        [Fact]
        public void ConvertRegexWithAllOptionsBson()
        {
            Regex regex = new Regex(
                "/",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new RegexConverter());

            serializer.Serialize(writer, new RegexTestClass { Regex = regex });

            string expected = "14-00-00-00-0B-52-65-67-65-78-00-2F-00-69-6D-73-75-78-00-00";
            string bson = BytesToHex(ms.ToArray());

            Assert.Equal(expected, bson);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);
            serializer.Converters.Add(new RegexConverter());

            RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

            Assert.Equal("/", c.Regex.ToString());
            Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture, c.Regex.Options);
        }

        [Fact]
        public void ConvertEmptyRegexJson()
        {
            Regex regex = new Regex("");

            string json = JsonConvert.SerializeObject(new RegexTestClass { Regex = regex }, Formatting.Indented, new RegexConverter());

            StringAssert.Equal(@"{
  ""Regex"": {
    ""Pattern"": """",
    ""Options"": 0
  }
}", json);

            RegexTestClass newRegex = JsonConvert.DeserializeObject<RegexTestClass>(json, new RegexConverter());
            Assert.Equal("", newRegex.Regex.ToString());
            Assert.Equal(RegexOptions.None, newRegex.Regex.Options);
        }
    }
}