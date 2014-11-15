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
using System.Globalization;
#if NET40
using System.Numerics;
#endif
using OpenGamingLibrary.Json.Linq;
using System.Text;
using System.IO;
using System.Xml;
using OpenGamingLibrary.Json;
using OpenGamingLibrary.Json.Utilities;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;

namespace OpenGamingLibrary.Json.Test
{
    
    public class JsonTextReaderTest : TestFixtureBase
    {
        [Fact]
        public void ReadSingleQuoteInsideDoubleQuoteString()
        {
            string json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
            jsonTextReader.Read();
            jsonTextReader.Read();
            jsonTextReader.Read();

            Assert.Equal(@"Forest's Bakery And Cafe", jsonTextReader.Value);
        }

        [Fact]
        public void ReadMultilineString()
        {
            string json = @"""first line
second line
third line""";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.String, jsonTextReader.TokenType);

            Assert.Equal(@"first line
second line
third line", jsonTextReader.Value);
        }

#if NET40
        [Fact]
        public void ReadBigInteger()
        {
            string json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333,
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);
            Assert.Equal(typeof(BigInteger), jsonTextReader.ValueType);
            Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.False(jsonTextReader.Read());

            JObject o = JObject.Parse(json);
            var i = (BigInteger)((JValue)o["ChildId"]).Value;
            Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), i);
        }
#endif

        [Fact]
        public void ReadIntegerWithError()
        {
            string json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

			AssertException.Throws<JsonReaderException>(() => { jsonTextReader.ReadAsInt32(); }, "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 53.");

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.False(jsonTextReader.Read());
        }

        [Fact]
        public void ReadIntegerWithErrorInArray()
        {
            string json = @"[
  333333333333333333333333333333333333333,
  3.3,
  ,
  0f
]";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.StartArray, jsonTextReader.TokenType);

            AssertException.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 42.");

            AssertException.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 6.");

            AssertException.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Error reading integer. Unexpected token: Undefined. Path '[2]', line 4, position 3.");

            AssertException.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 5.");

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.EndArray, jsonTextReader.TokenType);

            Assert.False(jsonTextReader.Read());
        }

        [Fact]
        public void ReadBytesWithError()
        {
            string json = @"{
    ChildId: '123'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

            try
            {
                jsonTextReader.ReadAsBytes();
            }
            catch (FormatException)
            {
            }

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.False(jsonTextReader.Read());
        }

        [Fact]
        public void ReadBadMSDateAsString()
        {
            string json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
            Assert.Equal(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

            Assert.True(jsonTextReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.False(jsonTextReader.Read());
        }

        [Fact]
        public void ReadInvalidNonBase10Number()
        {
            string json = "0aq2dun13.hod";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

            reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
        }

        [Fact]
        public void ThrowErrorWhenParsingUnquoteStringThatStartsWithNE()
        {
            const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected content while parsing JSON. Path 'u', line 1, position 27.");
        }

        [Fact]
        public void FloatParseHandling()
        {
            string json = "[1.0,1,9.9,1E-06]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(1.0m, reader.Value);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(1L, reader.Value);
            Assert.Equal(typeof(long), reader.ValueType);
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(9.9m, reader.Value);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(Convert.ToDecimal(1E-06), reader.Value);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void FloatParseHandling_NaN()
        {
            string json = "[NaN]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => reader.Read(), "Cannot read NaN as a decimal.");
        }

        [Fact]
        public void UnescapeDoubleQuotes()
        {
            string json = @"{""recipe_id"":""12"",""recipe_name"":""Apocalypse Leather Armors"",""recipe_text"":""#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \""Enhanced Leather Armor Boots\"" \""85644\"" )\r\n<img src=rdb:\/\/13264>\r\n\r\n#L \""Hacker Tool\"" \""87814\""\r\n<img src=rdb:\/\/99282>\r\n\r\n#L \""Clanalizer\"" \""208313\""\r\n<img src=rdb:\/\/156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \""Hacked Leather Armor Boots\"" \""245979\"" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \""Apocalypse Leather Armor Boots\"" \""245966\"" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \""Apocalypse Leather Armor Boots\"" \""245967\""\r\n#L \""Apocalypse Leather Armor Gloves\"" \""245969\""\r\n#L \""Apocalypse Leather Armor Helmet\"" \""245975\""\r\n#L \""Apocalypse Leather Armor Pants\"" \""245971\""\r\n#L \""Apocalypse Leather Armor Sleeves\"" \""245973\""\r\n#L \""Apocalypse Leather Body Armor\"" \""245965\""\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n"",""recipe_author"":null}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("recipe_text", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.Equal("#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \"Enhanced Leather Armor Boots\" \"85644\" )\r\n<img src=rdb://13264>\r\n\r\n#L \"Hacker Tool\" \"87814\"\r\n<img src=rdb://99282>\r\n\r\n#L \"Clanalizer\" \"208313\"\r\n<img src=rdb://156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb://13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \"Hacked Leather Armor Boots\" \"245979\" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb://13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \"Apocalypse Leather Armor Boots\" \"245966\" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \"Apocalypse Leather Armor Boots\" \"245967\"\r\n#L \"Apocalypse Leather Armor Gloves\" \"245969\"\r\n#L \"Apocalypse Leather Armor Helmet\" \"245975\"\r\n#L \"Apocalypse Leather Armor Pants\" \"245971\"\r\n#L \"Apocalypse Leather Armor Sleeves\" \"245973\"\r\n#L \"Apocalypse Leather Body Armor\" \"245965\"\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n", reader.Value);
        }

        [Fact]
        public void SurrogatePairValid()
        {
            string json = @"{ ""MATHEMATICAL ITALIC CAPITAL ALPHA"": ""\uD835\uDEE2"" }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.True(reader.Read());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            string s = reader.Value.ToString();
            Assert.Equal(2, s.Length);

            StringInfo stringInfo = new StringInfo(s);
            Assert.Equal(1, stringInfo.LengthInTextElements);
        }

        [Fact]
        public void SurrogatePairReplacement()
        {
            // existing good surrogate pair
            Assert.Equal("ABC \ud800\udc00 DEF", ReadString("ABC \\ud800\\udc00 DEF"));

            // invalid surrogates (two high back-to-back)
            Assert.Equal("ABC \ufffd\ufffd DEF", ReadString("ABC \\ud800\\ud800 DEF"));

            // invalid surrogates (two high back-to-back)
            Assert.Equal("ABC \ufffd\ufffd\u1234 DEF", ReadString("ABC \\ud800\\ud800\\u1234 DEF"));

            // invalid surrogates (three high back-to-back)
            Assert.Equal("ABC \ufffd\ufffd\ufffd DEF", ReadString("ABC \\ud800\\ud800\\ud800 DEF"));

            // invalid surrogates (high followed by a good surrogate pair)
            Assert.Equal("ABC \ufffd\ud800\udc00 DEF", ReadString("ABC \\ud800\\ud800\\udc00 DEF"));

            // invalid high surrogate at end of string
            Assert.Equal("ABC \ufffd", ReadString("ABC \\ud800"));

            // high surrogate not followed by low surrogate
            Assert.Equal("ABC \ufffd DEF", ReadString("ABC \\ud800 DEF"));

            // low surrogate not preceded by high surrogate
            Assert.Equal("ABC \ufffd\ufffd DEF", ReadString("ABC \\udc00\\ud800 DEF"));

            // make sure unencoded invalid surrogate characters don't make it through
            Assert.Equal("\ufffd\ufffd\ufffd", ReadString("\udc00\ud800\ud800"));

            Assert.Equal("ABC \ufffd\b", ReadString("ABC \\ud800\\b"));
            Assert.Equal("ABC \ufffd ", ReadString("ABC \\ud800 "));
            Assert.Equal("ABC \b\ufffd", ReadString("ABC \\b\\ud800"));
        }

        private string ReadString(string input)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(@"""" + input + @""""));

            JsonTextReader reader = new JsonTextReader(new StreamReader(ms));
            reader.Read();

            string s = (string)reader.Value;

            return s;
        }

        [Fact]
        public void CloseInput()
        {
            MemoryStream ms = new MemoryStream();
            JsonTextReader reader = new JsonTextReader(new StreamReader(ms));

            Assert.True(ms.CanRead);
            reader.Close();
            Assert.False(ms.CanRead);

            ms = new MemoryStream();
            reader = new JsonTextReader(new StreamReader(ms)) { CloseInput = false };

            Assert.True(ms.CanRead);
            reader.Close();
            Assert.True(ms.CanRead);
        }

        [Fact]
        public void YahooFinance()
        {
            string input = @"{
""matches"" : [
{""t"":""C"", ""n"":""Citigroup Inc."", ""e"":""NYSE"", ""id"":""662713""}
,{""t"":""CHL"", ""n"":""China Mobile Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""660998""}
,{""t"":""PTR"", ""n"":""PetroChina Company Limited (ADR)"", ""e"":""NYSE"", ""id"":""664536""}
,{""t"":""RIO"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""671472""}
,{""t"":""RIOPR"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""3512643""}
,{""t"":""CSCO"", ""n"":""Cisco Systems, Inc."", ""e"":""NASDAQ"", ""id"":""99624""}
,{""t"":""CVX"", ""n"":""Chevron Corporation"", ""e"":""NYSE"", ""id"":""667226""}
,{""t"":""TM"", ""n"":""Toyota Motor Corporation (ADR)"", ""e"":""NYSE"", ""id"":""655880""}
,{""t"":""JPM"", ""n"":""JPMorgan Chase \\x26 Co."", ""e"":""NYSE"", ""id"":""665639""}
,{""t"":""COP"", ""n"":""ConocoPhillips"", ""e"":""NYSE"", ""id"":""1691168""}
,{""t"":""LFC"", ""n"":""China Life Insurance Company Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""688679""}
,{""t"":""NOK"", ""n"":""Nokia Corporation (ADR)"", ""e"":""NYSE"", ""id"":""657729""}
,{""t"":""KO"", ""n"":""The Coca-Cola Company"", ""e"":""NYSE"", ""id"":""6550""}
,{""t"":""VZ"", ""n"":""Verizon Communications Inc."", ""e"":""NYSE"", ""id"":""664887""}
,{""t"":""AMX"", ""n"":""America Movil S.A.B de C.V. (ADR)"", ""e"":""NYSE"", ""id"":""665834""}],
""all"" : false
}
";

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(input)))
            {
                while (jsonReader.Read())
                {
                    Console.WriteLine(jsonReader.Value);
                }
            }
        }

        [Fact]
        public void ReadConstructor()
        {
            string json = @"{""DefaultConverter"":new Date(0, ""hi""),""MemberConverter"":""1970-01-01T00:00:00Z""}";

            JsonReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
            Assert.Equal("Date", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(0L, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal("hi", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal("MemberConverter", reader.Value);
        }

        [Fact]
        public void ParseAdditionalContent_Comma()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
],";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() =>
            {
                while (reader.Read())
                {
                }
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 2.");
        }

        [Fact]
        public void ParseAdditionalContent_Text()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
]content";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[2]);
#endif

            reader.Read();
            Assert.Equal(1, reader.LineNumber);

            reader.Read();
            Assert.Equal(2, reader.LineNumber);

            reader.Read();
            Assert.Equal(3, reader.LineNumber);

            reader.Read();
            Assert.Equal(4, reader.LineNumber);

            reader.Read();
            Assert.Equal(5, reader.LineNumber);

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 2.");
        }

        [Fact]
        public void ParseAdditionalContent_Whitespace()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
]   

";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
            }
        }

        [Fact]
        public void ParseAdditionalContent_WhitespaceThenText()
        {
            string json = @"'hi' a";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() =>
            {
                while (reader.Read())
                {
                }
            }, "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
        }

        [Fact]
        public void ReadingIndented()
        {
            string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            StringReader sr = new StringReader(input);

            using (JsonTextReader jsonReader = new JsonTextReader(sr))
            {
#if DEBUG
                jsonReader.SetCharBuffer(new char[5]);
#endif

                Assert.Equal(jsonReader.TokenType, JsonToken.None);
                Assert.Equal(0, jsonReader.LineNumber);
                Assert.Equal(0, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
                Assert.Equal(1, jsonReader.LineNumber);
                Assert.Equal(1, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.Equal(jsonReader.Value, "CPU");
                Assert.Equal(2, jsonReader.LineNumber);
                Assert.Equal(7, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(JsonToken.String, jsonReader.TokenType);
                Assert.Equal("Intel", jsonReader.Value);
                Assert.Equal(2, jsonReader.LineNumber);
                Assert.Equal(15, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.Equal(jsonReader.Value, "Drives");
                Assert.Equal(3, jsonReader.LineNumber);
                Assert.Equal(10, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
                Assert.Equal(3, jsonReader.LineNumber);
                Assert.Equal(12, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal(jsonReader.Value, "DVD read/writer");
                Assert.Equal(jsonReader.QuoteChar, '\'');
                Assert.Equal(4, jsonReader.LineNumber);
                Assert.Equal(22, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
                Assert.Equal(jsonReader.QuoteChar, '"');
                Assert.Equal(5, jsonReader.LineNumber);
                Assert.Equal(30, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
                Assert.Equal(6, jsonReader.LineNumber);
                Assert.Equal(4, jsonReader.LinePosition);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
                Assert.Equal(7, jsonReader.LineNumber);
                Assert.Equal(2, jsonReader.LinePosition);

                Assert.False(jsonReader.Read());
            }
        }

        [Fact]
        public void Depth()
        {
            string input = @"{
  value:'Purple',
  array:[1,2,new Date(1)],
  subobject:{prop:1,proparray:[1]}
}";

            StringReader sr = new StringReader(input);

            using (JsonReader reader = new JsonTextReader(sr))
            {
                Assert.Equal(0, reader.Depth);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.StartObject);
                Assert.Equal(0, reader.Depth);
                Assert.Equal("", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.PropertyName);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("value", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.String);
                Assert.Equal(reader.Value, @"Purple");
                Assert.Equal(reader.QuoteChar, '\'');
                Assert.Equal(1, reader.Depth);
                Assert.Equal("value", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.PropertyName);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("array", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.StartArray);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("array", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.Integer);
                Assert.Equal(1L, reader.Value);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("array[0]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.Integer);
                Assert.Equal(2L, reader.Value);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("array[1]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.StartConstructor);
                Assert.Equal("Date", reader.Value);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("array[2]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.Integer);
                Assert.Equal(1L, reader.Value);
                Assert.Equal(3, reader.Depth);
                Assert.Equal("array[2][0]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.EndConstructor);
                Assert.Equal(null, reader.Value);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("array[2]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.EndArray);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("array", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.PropertyName);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("subobject", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.StartObject);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("subobject", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.PropertyName);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("subobject.prop", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.Integer);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("subobject.prop", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.PropertyName);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("subobject.proparray", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.StartArray);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("subobject.proparray", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.Integer);
                Assert.Equal(3, reader.Depth);
                Assert.Equal("subobject.proparray[0]", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.EndArray);
                Assert.Equal(2, reader.Depth);
                Assert.Equal("subobject.proparray", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.EndObject);
                Assert.Equal(1, reader.Depth);
                Assert.Equal("subobject", reader.Path);

                reader.Read();
                Assert.Equal(reader.TokenType, JsonToken.EndObject);
                Assert.Equal(0, reader.Depth);
                Assert.Equal("", reader.Path);
            }
        }

        [Fact]
        public void NullTextReader()
        {
            AssertException.Throws<ArgumentNullException> (
                () => { new JsonTextReader (null); },
                new string[] { 
                    "Value cannot be null." + Environment.NewLine + "Parameter name: reader",
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: reader" // Mono
                });
        }

        [Fact]
        public void UnexpectedEndOfString()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'hi"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Fact]
        public void ReadLongString()
        {
            string s = new string('a', 10000);
            JsonReader reader = new JsonTextReader(new StringReader("'" + s + "'"));
            reader.Read();

            Assert.Equal(s, reader.Value);
        }

        [Fact]
        public void ReadLongJsonArray()
        {
            int valueCount = 10000;
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartArray();
            for (int i = 0; i < valueCount; i++)
            {
                writer.WriteValue(i);
            }
            writer.WriteEndArray();

            string json = sw.ToString();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            Assert.True(reader.Read());
            for (int i = 0; i < valueCount; i++)
            {
                Assert.True(reader.Read());
                Assert.Equal((long)i, reader.Value);
            }
            Assert.True(reader.Read());
            Assert.False(reader.Read());
        }

        [Fact]
        public void NullCharReading()
        {
            string json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("\0sd\0f\0", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("\0sd\0f\0", reader.Value);

            Assert.False(reader.Read());
        }

        [Fact]
        public void AppendCharsWhileReadingNull()
        {
            string json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[129]);
#endif

            for (int i = 0; i < 15; i++)
            {
                reader.Read();
            }

            reader.Read();
            Assert.Equal(JsonToken.Null, reader.TokenType);
        }

        [Fact]
        public void ReadInt32Overflow()
        {
            long i = int.MaxValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.Equal(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                long total = j + i;
                AssertException.Throws<JsonReaderException>(() =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    reader.ReadAsInt32();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.");
            }
        }

        [Fact]
        public void ReadInt32Overflow_Negative()
        {
            long i = int.MinValue;

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.Equal(typeof(long), reader.ValueType);
            Assert.Equal(i, reader.Value);

            for (int j = 1; j < 1000; j++)
            {
                long total = -j + i;
                AssertException.Throws<JsonReaderException>(() =>
                {
                    reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                    reader.ReadAsInt32();
                }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.");
            }
        }

#if NET40
        [Fact]
        public void ReadInt64Overflow()
        {
            BigInteger i = new BigInteger(long.MaxValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.Equal(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.Read();

                Assert.Equal(typeof(BigInteger), reader.ValueType);
            }
        }

        [Fact]
        public void ReadInt64Overflow_Negative()
        {
            BigInteger i = new BigInteger(long.MinValue);

            JsonTextReader reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
            reader.Read();
            Assert.Equal(typeof(long), reader.ValueType);

            for (int j = 1; j < 1000; j++)
            {
                BigInteger total = i + -j;

                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.Read();

                Assert.Equal(typeof(BigInteger), reader.ValueType);
            }
        }
#endif

        [Fact]
        public void AppendCharsWhileReadingNewLine()
        {
            string json = @"
{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"":
  {
    ""name"": {""type"":""string""},
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {""type"":""string""}
    }
  }
}
";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[129]);
#endif

            for (int i = 0; i < 14; i++)
            {
                Assert.True(reader.Read());
            }

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("type", reader.Value);
        }

        [Fact]
        public void ReadNullTerminatorStrings()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
            Assert.True(reader.Read());

            Assert.Equal("h\0i", reader.Value);
        }

        [Fact]
        public void UnexpectedEndOfHex()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing unicode character. Path '', line 1, position 4.");
        }

        [Fact]
        public void UnexpectedEndOfControlCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
        }

        [Fact]
        public void ReadBytesWithBadCharacter()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"true"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Error reading bytes. Unexpected token: Boolean. Path '', line 1, position 4.");
        }

        [Fact]
        public void ReadBytesWithUnexpectedEnd()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
        }

        [Fact]
        public void ReadBytesNoStartWithUnexpectedEnd()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
            Assert.True(reader.Read());

            Assert.Null(reader.ReadAsBytes());
            Assert.Equal(JsonToken.None, reader.TokenType);
        }

        [Fact]
        public void UnexpectedEndWhenParsingUnquotedProperty()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
            Assert.True(reader.Read());

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
        }

        [Fact]
        public void ReadNewLines()
        {
            string newLinesText = StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed + StringUtils.LineFeed + StringUtils.CarriageReturnLineFeed + " " + StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed;

            string json =
                newLinesText
                + "{" + newLinesText
                + "'" + newLinesText
                + "name1" + newLinesText
                + "'" + newLinesText
                + ":" + newLinesText
                + "[" + newLinesText
                + "new" + newLinesText
                + "Date" + newLinesText
                + "(" + newLinesText
                + "1" + newLinesText
                + "," + newLinesText
                + "null" + newLinesText
                + "/*" + newLinesText
                + "blah comment" + newLinesText
                + "*/" + newLinesText
                + ")" + newLinesText
                + "," + newLinesText
                + "1.1111" + newLinesText
                + "]" + newLinesText
                + "," + newLinesText
                + "name2" + newLinesText
                + ":" + newLinesText
                + "{" + newLinesText
                + "}" + newLinesText
                + "}" + newLinesText;

            int count = 0;
            StringReader sr = new StringReader(newLinesText);
            while (sr.ReadLine() != null)
            {
                count++;
            }

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
            Assert.True(reader.Read());
            Assert.Equal(7, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(31, reader.LineNumber);
            Assert.Equal(newLinesText + "name1" + newLinesText, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(37, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(55, reader.LineNumber);
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
            Assert.Equal("Date", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(61, reader.LineNumber);
            Assert.Equal(1L, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(73, reader.LineNumber);
            Assert.Equal(null, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(91, reader.LineNumber);
            Assert.Equal(newLinesText + "blah comment" + newLinesText, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(97, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(109, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(115, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(133, reader.LineNumber);
            Assert.Equal("name2", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(139, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(145, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(151, reader.LineNumber);
        }

        [Fact]
        public void ParsingQuotedPropertyWithControlCharacters()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{'hi\r\nbye':1}"));
            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("hi\r\nbye", reader.Value);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(1L, reader.Value);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadBytesFollowingNumberInArray()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"[1,'" + Convert.ToBase64String(helloWorldData) + @"']"));
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            byte[] data = reader.ReadAsBytes();
            Assert.Equal(helloWorldData, data);
            Assert.Equal(JsonToken.Bytes, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadBytesFollowingNumberInObject()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"{num:1,data:'" + Convert.ToBase64String(helloWorldData) + @"'}"));
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);
            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.True(reader.Read());
            byte[] data = reader.ReadAsBytes();
            Assert.Equal(helloWorldData, data);
            Assert.Equal(JsonToken.Bytes, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadingEscapedStrings()
        {
            string input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

            StringReader sr = new StringReader(input);

            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                Assert.Equal(0, jsonReader.Depth);

                jsonReader.Read();
                Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
                Assert.Equal(0, jsonReader.Depth);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal(1, jsonReader.Depth);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.String);
                Assert.Equal("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
                Assert.Equal('\'', jsonReader.QuoteChar);
                Assert.Equal(1, jsonReader.Depth);

                jsonReader.Read();
                Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);
                Assert.Equal(0, jsonReader.Depth);
            }
        }

        [Fact]
        public void ReadNewlineLastCharacter()
        {
            string input = @"{
  CPU: 'Intel',
  Drives: [ /* Com*ment */
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}" + '\n';

            object o = JsonConvert.DeserializeObject(input);
        }

        [Fact]
        public void ReadRandomJson()
        {
            string json = @"[
  true,
  {
    ""integer"": 99,
    ""string"": ""how now brown cow?"",
    ""array"": [
      0,
      1,
      2,
      3,
      4,
      {
        ""decimal"": 990.00990099
      },
      5
    ]
  },
  ""This is a string."",
  null,
  null
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
            }
        }

        [Fact]
        public void ParseIntegers()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1"));
            Assert.Equal(1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-1"));
            Assert.Equal(-1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("0"));
            Assert.Equal(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-0"));
            Assert.Equal(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
            Assert.Equal(int.MaxValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
            Assert.Equal(int.MinValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

            reader = new JsonTextReader(new StringReader("1.1"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

            reader = new JsonTextReader(new StringReader(""));
            Assert.Equal(null, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseDecimals()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.Equal(1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.Equal(-1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.Equal(0.0m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.Equal(0, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            reader.FloatParseHandling = Json.FloatParseHandling.Decimal;
            AssertException.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.Equal(0.000001m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader(""));
            Assert.Equal(null, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-"));
            AssertException.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseDoubles()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(-1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(-0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            AssertException.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(0.000001d, reader.Value);

            reader = new JsonTextReader(new StringReader(""));
            Assert.False(reader.Read());

            reader = new JsonTextReader(new StringReader("-"));
            AssertException.Throws<JsonReaderException>(() => reader.Read(), "Input string '-' is not a valid number. Path '', line 1, position 1.");
        }

        [Fact]
        public void WriteReadWrite()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw)
            {
                Formatting = Formatting.Indented
            })
            {
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(true);

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("integer");
                jsonWriter.WriteValue(99);
                jsonWriter.WritePropertyName("string");
                jsonWriter.WriteValue("how now brown cow?");
                jsonWriter.WritePropertyName("array");

                jsonWriter.WriteStartArray();
                for (int i = 0; i < 5; i++)
                {
                    jsonWriter.WriteValue(i);
                }

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("decimal");
                jsonWriter.WriteValue(990.00990099m);
                jsonWriter.WriteEndObject();

                jsonWriter.WriteValue(5);
                jsonWriter.WriteEndArray();

                jsonWriter.WriteEndObject();

                jsonWriter.WriteValue("This is a string.");
                jsonWriter.WriteNull();
                jsonWriter.WriteNull();
                jsonWriter.WriteEndArray();
            }

            string json = sb.ToString();

            JsonSerializer serializer = new JsonSerializer();

            object jsonObject = serializer.Deserialize(new JsonTextReader(new StringReader(json)));

            sb = new StringBuilder();
            sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw)
            {
                Formatting = Formatting.Indented
            })
            {
                serializer.Serialize(jsonWriter, jsonObject);
            }

            Assert.Equal(json, sb.ToString());
        }

        [Fact]
        public void FloatingPointNonFiniteNumbers()
        {
            string input = @"[
  NaN,
  Infinity,
  -Infinity
]";

            StringReader sr = new StringReader(input);

            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.Float);
                Assert.Equal(jsonReader.Value, double.NaN);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.Float);
                Assert.Equal(jsonReader.Value, double.PositiveInfinity);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.Float);
                Assert.Equal(jsonReader.Value, double.NegativeInfinity);

                jsonReader.Read();
                Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
            }
        }

        [Fact]
        public void LongStringTest()
        {
            int length = 20000;
            string json = @"[""" + new string(' ', length) + @"""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(typeof(string), reader.ValueType);
            Assert.Equal(20000, reader.Value.ToString().Length);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.False(reader.Read());
            Assert.Equal(JsonToken.None, reader.TokenType);
        }

        [Fact]
        public void EscapedUnicodeText()
        {
            string json = @"[""\u003c"",""\u5f20""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[2]);
#endif

            reader.Read();
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            reader.Read();
            Assert.Equal("<", reader.Value);

            reader.Read();
            Assert.Equal(24352, Convert.ToInt32(Convert.ToChar((string)reader.Value)));

            reader.Read();
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void ReadFloatingPointNumber()
        {
            string json =
                @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN,0e-10,0.25e-5,0.3e10]";

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(json)))
            {
                jsonReader.Read();
                Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(0.0, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(0.0, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(0.1, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(1.0, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(1.000001, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(1E-06, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(4.94065645841247E-324, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.PositiveInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.NegativeInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.NaN, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.MaxValue, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.MinValue, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.PositiveInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.NegativeInfinity, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(double.NaN, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(0d, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(0.0000025d, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.Float, jsonReader.TokenType);
                Assert.Equal(3000000000d, jsonReader.Value);

                jsonReader.Read();
                Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);
            }
        }

        [Fact]
        public void MissingColon()
        {
            string json = @"{
    ""A"" : true,
    ""B"" """;

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.Boolean, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 9.");
        }

        [Fact]
        public void ReadSingleBytes()
        {
            StringReader s = new StringReader(@"""SGVsbG8gd29ybGQu""");
            JsonTextReader reader = new JsonTextReader(s);

            byte[] data = reader.ReadAsBytes();
            Assert.NotNull(data);

            string text = Encoding.UTF8.GetString(data, 0, data.Length);
            Assert.Equal("Hello world.", text);
        }

        [Fact]
        public void ReadOctalNumber()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(250L, jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(250L, jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(250L, jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

            Assert.False(jsonReader.Read());
        }

        [Fact]
        public void ReadOctalNumberAsInt64()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

            jsonReader.Read();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(long), jsonReader.ValueType);
            Assert.Equal((long)250, (long)jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(long), jsonReader.ValueType);
            Assert.Equal((long)250, (long)jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(long), jsonReader.ValueType);
            Assert.Equal((long)250, (long)jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

            Assert.False(jsonReader.Read());
        }

        [Fact]
        public void ReadOctalNumberAsInt32()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

            jsonReader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(int), jsonReader.ValueType);
            Assert.Equal(250, jsonReader.Value);

            jsonReader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(int), jsonReader.ValueType);
            Assert.Equal(250, jsonReader.Value);

            jsonReader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
            Assert.Equal(typeof(int), jsonReader.ValueType);
            Assert.Equal(250, jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

            Assert.False(jsonReader.Read());
        }

        [Fact]
        public void ReadBadCharInArray()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[}"));

            reader.Read();

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
        }

        [Fact]
        public void ReadAsDecimalNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.Null(reader.ReadAsDecimal());
            Assert.Equal(JsonToken.None, reader.TokenType);
        }

        [Fact]
        public void ReadAsBytesNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.Null(reader.ReadAsBytes());
            Assert.Equal(JsonToken.None, reader.TokenType);
        }

        [Fact]
        public void ReadAsBytesNoContentWrappedObject()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading bytes. Path '', line 1, position 1.");
        }

#if !NET20
        [Fact]
        public void ReadAsDateTimeOffsetNoContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.Null(reader.ReadAsDateTimeOffset());
            Assert.Equal(JsonToken.None, reader.TokenType);
        }
#endif

        [Fact]
        public void ReadAsDecimalBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Error reading decimal. Unexpected token: StartConstructor. Path '', line 1, position 9.");
        }

        [Fact]
        public void ReadAsBytesBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Error reading bytes. Unexpected token: StartConstructor. Path '', line 1, position 9.");
        }

#if !NET20
        [Fact]
        public void ReadAsDateTimeOffsetBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"new Date()"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Error reading date. Unexpected token: StartConstructor. Path '', line 1, position 9.");
        }
#endif

        [Fact]
        public void ReadAsBytesIntegerArrayWithComments()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

            byte[] data = reader.ReadAsBytes();
            Assert.Equal(2, data.Length);
            Assert.Equal(1, data[0]);
            Assert.Equal(2, data[1]);
        }

        [Fact]
        public void ReadAsBytesIntegerArrayWithNoEnd()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
        }

        [Fact]
        public void ReadAsBytesArrayWithBadContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[1.0]"));

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
        }

        [Fact]
        public void ReadUnicode()
        {
            string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Message", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(@"Hi,I" + '\u0092' + "ve send you smth", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadHexidecimalWithAllLetters()
        {
            string json = @"{""text"":0xabcdef12345}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(11806310474565, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

#if !NET20
        [Fact]
        public void ReadAsDateTimeOffset()
        {
            string json = "{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetNegative()
        {
            string json = @"{""Offset"":""\/Date(946706400000-0600)\/""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetBadString()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }

        [Fact]
        public void ReadAsDateTimeOffsetHoursOnly()
        {
            string json = "{\"Offset\":\"\\/Date(946663200000+06)\\/\"}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetWithMinutes()
        {
            string json = @"{""Offset"":""\/Date(946708260000-0631)\/""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetIsoDate()
        {
            string json = @"{""Offset"":""2011-08-01T21:25Z""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.Equal(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetUnitedStatesDate()
        {
            string json = @"{""Offset"":""1/30/2011""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("en-US");

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

            DateTimeOffset dt = (DateTimeOffset)reader.Value;
            Assert.Equal(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDateTimeOffsetNewZealandDate()
        {
            string json = @"{""Offset"":""30/1/2011""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("en-NZ");

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, reader.TokenType);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

            DateTimeOffset dt = (DateTimeOffset)reader.Value;
            Assert.Equal(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }
#endif

        [Fact]
        public void ReadAsDecimalInt()
        {
            string json = @"{""Name"":1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

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
        public void ReadAsIntDecimal()
        {
            string json = @"{""Name"": 1.1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            AssertException.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
        }

        [Fact]
        public void MatchWithInsufficentCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nul"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 0, position 0.");
        }

        [Fact]
        public void MatchWithWrongCharacters()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nulz"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 0, position 0.");
        }

        [Fact]
        public void MatchWithNoTrailingSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"nullz"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 1, position 4.");
        }

        [Fact]
        public void UnclosedComment()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/* sdf"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 6.");
        }

        [Fact]
        public void BadCommentStart()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"/sdf"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
        }

        [Fact]
        public void ReadAsDecimal()
        {
            string json = @"{""decimal"":-7.92281625142643E+28}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            decimal? d = reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(-79228162514264300000000000000m, d);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadAsDecimalFrench()
        {
            string json = @"{""decimal"":""9,99""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("fr-FR");

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            decimal? d = reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(typeof(decimal), reader.ValueType);
            Assert.Equal(9.99m, d);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ReadBufferOnControlChar()
        {
            string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

            for (int i = 0; i < 13; i++)
            {
                reader.Read();
            }

            Assert.True(reader.Read());
            Assert.Equal(new DateTime(631136448000000000), reader.Value);
        }

        [Fact]
        public void ReadBufferOnEndComment()
        {
            string json = @"/*comment*/ { /*comment*/
        ""Name"": /*comment*/ ""Apple"" /*comment*/, /*comment*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment*/ [ /*comment*/
          ""Small"", /*comment*/
          ""Medium"" /*comment*/,
          /*comment*/ ""Large""
        /*comment*/ ] /*comment*/
      } /*comment*/";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[5]);
#endif

            for (int i = 0; i < 26; i++)
            {
                Assert.True(reader.Read());
            }

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ParseNullStringConstructor()
        {
            string json = "new Date\0()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.SetCharBuffer(new char[7]);
#endif

            Assert.True(reader.Read());
            Assert.Equal("Date", reader.Value);
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseLineFeedDelimitedConstructor()
        {
            string json = "new Date\n()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal("Date", reader.Value);
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseArrayWithMissingValues()
        {
            string json = "[,,, \n\r\n \0   \r  , ,    ]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Undefined, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Undefined, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Undefined, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Undefined, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Undefined, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void SupportMultipleContent()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"{'prop1':[1]} 1 2 ""name"" [][]null {}{} 1.1"));
            reader.SupportMultipleContent = true;

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ParseBooleanWithNoExtraContent()
        {
            string json = "[true ";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.False(reader.Read());
        }

        [Fact]
        public void ParseConstructorWithUnexpectedEnd()
        {
            string json = "new Dat";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing constructor. Path '', line 1, position 7.");
        }

        [Fact]
        public void ParseConstructorWithUnexpectedCharacter()
        {
            string json = "new Date !";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character while parsing constructor: !. Path '', line 1, position 9.");
        }

        [Fact]
        public void ParseObjectWithNoEnd()
        {
            string json = "{hi:1, ";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.False(reader.Read());
        }

        [Fact]
        public void ParseEmptyArray()
        {
            string json = "[]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void ParseEmptyObject()
        {
            string json = "{}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ParseIncompleteCommentSeparator()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("true/"));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing boolean value. Path '', line 1, position 4.");
        }

        [Fact]
        public void ParseEmptyConstructor()
        {
            string json = "new Date()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseConstructorWithBadCharacter()
        {
            string json = "new Date,()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            AssertException.Throws<JsonReaderException>(() => { Assert.True(reader.Read()); }, "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.");
        }

        [Fact]
        public void ParseContentDelimitedByNonStandardWhitespace()
        {
            string json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadContentDelimitedByComments()
        {
            string json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ""ExpiryDate"":/*comment*/new
" + StringUtils.LineFeed +
                          @"Date
(/*comment*/null/*comment*/),
        ""Price"": 3.99,
        ""Sizes"":/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Name", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(true, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("ExpiryDate", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
            Assert.Equal(5, reader.LineNumber);
            Assert.Equal("Date", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void SingleLineComments()
        {
            string json = @"//comment*//*hi*/
{//comment
Name://comment
true//comment after true" + StringUtils.CarriageReturn +
@",//comment after comma" + StringUtils.CarriageReturnLineFeed + 
@"""ExpiryDate""://comment"  + StringUtils.LineFeed + 
@"new " + StringUtils.LineFeed +
@"Date
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

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment*//*hi*/", reader.Value);
            Assert.Equal(2, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(2, reader.LineNumber);
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal(3, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Name", reader.Value);
            Assert.Equal(3, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal(4, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(true, reader.Value);
            Assert.Equal(4, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment after true", reader.Value);
            Assert.Equal(5, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment after comma", reader.Value);
            Assert.Equal(6, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("ExpiryDate", reader.Value);
            Assert.Equal(6, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal(7, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
            Assert.Equal(9, reader.LineNumber);
            Assert.Equal("Date", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal(10, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal(11, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
            Assert.Equal(11, reader.LineNumber);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Price", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Sizes", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment ", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment 1 ", reader.Value);

            Assert.False(reader.Read());
        }

        [Fact]
        public void JustSinglelineComment()
        {
            string json = @"//comment";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Comment, reader.TokenType);
            Assert.Equal("comment", reader.Value);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ErrorReadingComment()
        {
            string json = @"/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseOctalNumber()
        {
            string json = @"010";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(8m, reader.Value);
        }

        [Fact]
        public void ParseHexNumber()
        {
            string json = @"0x20";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(32m, reader.Value);
        }

        [Fact]
        public void ParseNumbers()
        {
            string json = @"[0,1,2 , 3]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void UnexpectedEndTokenWhenParsingOddEndToken()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
            Assert.True(reader.Read());
            Assert.True(reader.Read());

            AssertException.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
        }

        [Fact]
        public void ScientificNotation()
        {
            double d;

            d = Convert.ToDouble("6.0221418e23", CultureInfo.InvariantCulture);
            Console.WriteLine(d.ToString(new CultureInfo("fr-FR")));
            Console.WriteLine(d.ToString("0.#############################################################################"));

            //CultureInfo info = CultureInfo.GetCultureInfo("fr-FR");
            //Console.WriteLine(info.NumberFormat.NumberDecimalSeparator);

            string json = @"[0e-10,0E-10,0.25e-5,0.3e10,6.0221418e23]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            reader.Read();

            reader.Read();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0d, reader.Value);

            reader.Read();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0d, reader.Value);

            reader.Read();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0.0000025d, reader.Value);

            reader.Read();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(3000000000d, reader.Value);

            reader.Read();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(602214180000000000000000d, reader.Value);

            reader.Read();


            reader = new JsonTextReader(new StringReader(json));

            reader.Read();

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0m, reader.Value);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0m, reader.Value);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0.0000025m, reader.Value);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(3000000000m, reader.Value);

            reader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(602214180000000000000000m, reader.Value);

            reader.Read();
        }

        [Fact]
        public void MaxDepth()
        {
            string json = "[[]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.True(reader.Read());

            AssertException.Throws<JsonReaderException>(() => { Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
        }

        [Fact]
        public void MaxDepthDoesNotRecursivelyError()
        {
            string json = "[[[[]]],[[]]]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                MaxDepth = 1
            };

            Assert.True(reader.Read());
            Assert.Equal(0, reader.Depth);

            AssertException.Throws<JsonReaderException>(() => { Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
            Assert.Equal(1, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(2, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(3, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(3, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(2, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(1, reader.Depth);

            AssertException.Throws<JsonReaderException>(() => { Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
            Assert.Equal(1, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(2, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(2, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(1, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(0, reader.Depth);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ReadingFromSlowStream()
        {
            string json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.True(reader.Read());

            Assert.True(reader.Read());
            Assert.Equal(false, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(true, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(true, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(false, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("test!", reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(1.11d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(0.0000025d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(3000000000d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(602214180000000000000000d, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(reader.Value, "Purple\r \n monkey's:\tdishwasher");

            Assert.True(reader.Read());
        }

        [Fact]
        public void DateParseHandling()
        {
            string json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTime;

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.Equal(typeof(DateTime), reader.ValueType);
            Assert.True(reader.Read());
            Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.Equal(typeof(DateTime), reader.ValueType);
            Assert.True(reader.Read());

#if !NET20
            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTimeOffset;

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.True(reader.Read());
            Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.True(reader.Read());
#endif

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.None;

            Assert.True(reader.Read());
            Assert.True(reader.Read());
            Assert.Equal(@"1970-01-01T00:00:00Z", reader.Value);
            Assert.Equal(typeof(string), reader.ValueType);
            Assert.True(reader.Read());
            Assert.Equal(@"/Date(0)/", reader.Value);
            Assert.Equal(typeof(string), reader.ValueType);
            Assert.True(reader.Read());

#if !NET20
            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTime;

            Assert.True(reader.Read());
            reader.ReadAsDateTimeOffset();
            Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            reader.ReadAsDateTimeOffset();
            Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
            Assert.True(reader.Read());


            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Json.DateParseHandling.DateTimeOffset;

            Assert.True(reader.Read());
            reader.ReadAsDateTime();
            Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.Equal(typeof(DateTime), reader.ValueType);
            reader.ReadAsDateTime();
            Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.Equal(typeof(DateTime), reader.ValueType);
            Assert.True(reader.Read());
#endif
        }

        [Fact]
        public void ResetJsonTextReaderErrorCount()
        {
            ToggleReaderError toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
            JsonTextReader jsonTextReader = new JsonTextReader(toggleReaderError);

            Assert.True(jsonTextReader.Read());

            toggleReaderError.Error = true;

            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;

            Assert.True(jsonTextReader.Read());
            Assert.Equal("first", jsonTextReader.Value);

            toggleReaderError.Error = true;

            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;

            Assert.True(jsonTextReader.Read());
            Assert.Equal(1L, jsonTextReader.Value);

            toggleReaderError.Error = true;

            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
            AssertException.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

            toggleReaderError.Error = false;

            //a reader use to skip to the end after 3 errors in a row
            //Assert.False(jsonTextReader.Read());
        }

        [Fact]
        public void WriteReadBoundaryDecimals()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartArray();
            writer.WriteValue(decimal.MaxValue);
            writer.WriteValue(decimal.MinValue);
            writer.WriteEndArray();

            string json = sw.ToString();

            StringReader sr = new StringReader(json);
            JsonTextReader reader = new JsonTextReader(sr);

            Assert.True(reader.Read());

            decimal? max = reader.ReadAsDecimal();
            Assert.Equal(decimal.MaxValue, max);

            decimal? min = reader.ReadAsDecimal();
            Assert.Equal(decimal.MinValue, min);

            Assert.True(reader.Read());
        }

        [Fact]
        public void EscapedPathInExceptionMessage()
        {
            string json = @"{
  ""frameworks"": {
    ""aspnetcore50"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": !!! !!!
        }
      }
    }
  }
}";

            AssertException.Throws<JsonReaderException>(
                () =>
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader(json));
                    while (reader.Read())
                    {
                    }
                },
                "Unexpected character encountered while parsing value: !. Path 'frameworks.aspnetcore50.dependencies.['System.Xml.ReaderWriter'].source', line 6, position 21.");
        }
    }

    public class ToggleReaderError : TextReader
    {
        private readonly TextReader _inner;

        public bool Error { get; set; }

        public ToggleReaderError(TextReader inner)
        {
            _inner = inner;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (Error)
                throw new Exception("Read error");

            return _inner.Read(buffer, index, 1);
        }
    }

    public class SlowStream : Stream
    {
        private byte[] bytes;
        private int totalBytesRead;
        private int bytesPerRead;

        public SlowStream(byte[] content, int bytesPerRead)
        {
            bytes = content;
            totalBytesRead = 0;
            this.bytesPerRead = bytesPerRead;
        }

        public SlowStream(string content, Encoding encoding, int bytesPerRead)
            : this(encoding.GetBytes(content), bytesPerRead)
        {
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toReturn = Math.Min(count, bytesPerRead);
            toReturn = Math.Min(toReturn, bytes.Length - totalBytesRead);
            if (toReturn > 0)
            {
                Array.Copy(bytes, totalBytesRead, buffer, offset, toReturn);
            }

            totalBytesRead += toReturn;
            return toReturn;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}