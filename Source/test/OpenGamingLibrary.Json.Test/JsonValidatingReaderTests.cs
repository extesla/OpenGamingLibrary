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
#if NET40
using System.Numerics;
#endif
using System.Text;
using Xunit;
using System.Xml;
using System.Xml.Schema;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Schema;
using OpenGamingLibrary.Json.Utilities;
using ValidationEventArgs = OpenGamingLibrary.Json.Schema.ValidationEventArgs;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test
{
    
    public class JsonValidatingReaderTests : TestFixtureBase
    {
        [Fact]
        public void CheckInnerReader()
        {
            string json = "{'name':'James','hobbies':['pie','cake']}";
            JsonReader reader = new JsonTextReader(new StringReader(json));

            JsonValidatingReader validatingReader = new JsonValidatingReader(reader);
            Assert.Equal(reader, validatingReader.Reader);
        }

        [Fact]
        public void ValidateTypes()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":
    {
      ""type"":""array"",
      ""items"": {""type"":""string""}
    }
  }
}";

            string json = @"{'name':""James"",'hobbies':[""pie"",'cake']}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            JsonSchema schema = JsonSchema.Parse(schemaJson);
            reader.Schema = schema;
            Assert.Equal(schema, reader.Schema);
            Assert.Equal(0, reader.Depth);
            Assert.Equal("", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);
            Assert.Equal("", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("name", reader.Value.ToString());
            Assert.Equal("name", reader.Path);
            Assert.Equal(1, reader.Depth);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("James", reader.Value.ToString());
            Assert.Equal(typeof(string), reader.ValueType);
            Assert.Equal('"', reader.QuoteChar);
            Assert.Equal("name", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("hobbies", reader.Value.ToString());
            Assert.Equal('\'', reader.QuoteChar);
            Assert.Equal("hobbies", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);
            Assert.Equal("hobbies", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("pie", reader.Value.ToString());
            Assert.Equal('"', reader.QuoteChar);
            Assert.Equal("hobbies[0]", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("cake", reader.Value.ToString());
            Assert.Equal("hobbies[1]", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
            Assert.Equal("hobbies", reader.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
            Assert.Equal("", reader.Path);

            Assert.False(reader.Read());

            Assert.Null(validationEventArgs);
        }

        [Fact]
        public void ValidateUnrestrictedArray()
        {
            string schemaJson = @"{
  ""type"":""array""
}";

            string json = "['pie','cake',['nested1','nested2'],{'nestedproperty1':1.1,'nestedproperty2':[null]}]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("pie", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("cake", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("nested1", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("nested2", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("nestedproperty1", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(1.1, reader.Value);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("nestedproperty2", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.Null(validationEventArgs);
        }

        [Fact]
        public void StringLessThanMinimumLength()
        {
            string schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":50,
}";

            string json = "'pie'";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("String 'pie' is less than minimum length of 5. Line 1, position 5.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void StringGreaterThanMaximumLength()
        {
            string schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":10
}";

            string json = "'The quick brown fox jumps over the lazy dog.'";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 10. Line 1, position 46.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void StringIsNotInEnum()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""string"",
    ""enum"":[""one"",""two""]
  },
  ""maxItems"":3
}";

            string json = "['one','two','THREE']";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(@"Value ""THREE"" is not defined in enum. Line 1, position 20.", validationEventArgs.Message);
            Assert.Equal("[2]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void StringDoesNotMatchPattern()
        {
            string schemaJson = @"{
  ""type"":""string"",
  ""pattern"":""foo""
}";

            string json = "'The quick brown fox jumps over the lazy dog.'";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' does not match regex pattern 'foo'. Line 1, position 46.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerGreaterThanMaximumValue()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            string json = "10";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal("Integer 10 exceeds maximum value of 5. Line 1, position 2.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);

            Assert.NotNull(validationEventArgs);
        }

#if NET40
        [Fact]
        public void IntegerGreaterThanMaximumValue_BigInteger()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            string json = "99999999999999999999999999999999999999999999999999999999999999999999";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal("Integer 99999999999999999999999999999999999999999999999999999999999999999999 exceeds maximum value of 5. Line 1, position 68.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerLessThanMaximumValue_BigInteger()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

            JValue v = new JValue(new BigInteger(1));

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            v.Validate(JsonSchema.Parse(schemaJson), (sender, args) => { validationEventArgs = args; });

            Assert.NotNull(validationEventArgs);
            Assert.Equal("Integer 1 is less than minimum value of 5.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);
        }
#endif

        [Fact]
        public void ThrowExceptionWhenNoValidationEventHandler()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

                JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader("10")));
                reader.Schema = JsonSchema.Parse(schemaJson);

                Assert.True(reader.Read());
            }, "Integer 10 exceeds maximum value of 5. Line 1, position 2.");
        }

        [Fact]
        public void IntegerLessThanMinimumValue()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

            string json = "1";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal("Integer 1 is less than minimum value of 5. Line 1, position 1.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerIsNotInEnum()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer"",
    ""enum"":[1,2]
  },
  ""maxItems"":3
}";

            string json = "[1,2,3]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(@"Value 3 is not defined in enum. Line 1, position 6.", validationEventArgs.Message);
            Assert.Equal("[2]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void FloatGreaterThanMaximumValue()
        {
            string schemaJson = @"{
  ""type"":""number"",
  ""maximum"":5
}";

            string json = "10.0";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal("Float 10.0 exceeds maximum value of 5. Line 1, position 4.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void FloatLessThanMinimumValue()
        {
            string schemaJson = @"{
  ""type"":""number"",
  ""minimum"":5
}";

            string json = "1.1";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal("Float 1.1 is less than minimum value of 5. Line 1, position 3.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void FloatIsNotInEnum()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""enum"":[1.1,2.2]
  },
  ""maxItems"":3
}";

            string json = "[1.1,2.2,3.0]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(@"Value 3.0 is not defined in enum. Line 1, position 12.", validationEventArgs.Message);
            Assert.Equal("[2]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void FloatDivisibleBy()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":0.1
  }
}";

            string json = "[1.1,2.2,4.001]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(@"Float 4.001 is not evenly divisible by 0.1. Line 1, position 14.", validationEventArgs.Message);
            Assert.Equal("[2]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

#if NET40
        [Fact]
        public void BigIntegerDivisibleBy_Success()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":2
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999998]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Null(validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void BigIntegerDivisibleBy_Failure()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":2
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999999]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 2. Line 1, position 58.", validationEventArgs.Message);
            Assert.Equal("[0]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void BigIntegerDivisibleBy_Fraction()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":1.1
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999999]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.NotNull(validationEventArgs);
            Assert.Equal(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 1.1. Line 1, position 58.", validationEventArgs.Message);
            Assert.Equal("[0]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void BigIntegerDivisibleBy_FractionWithZeroValue()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":1.1
  }
}";

            JArray a = new JArray(new JValue(new BigInteger(0)));

            ValidationEventArgs validationEventArgs = null;

            a.Validate(JsonSchema.Parse(schemaJson), (sender, args) => { validationEventArgs = args; });

            Assert.Null(validationEventArgs);
        }
#endif

        [Fact]
        public void IntValidForNumber()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number""
  }
}";

            string json = "[1]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.Null(validationEventArgs);
        }

        [Fact]
        public void NullNotInEnum()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""null"",
    ""enum"":[]
  },
  ""maxItems"":3
}";

            string json = "[null]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal(@"Value null is not defined in enum. Line 1, position 5.", validationEventArgs.Message);
            Assert.Equal("[0]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void BooleanNotInEnum()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""boolean"",
    ""enum"":[true]
  },
  ""maxItems"":3
}";

            string json = "[true,false]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Boolean, reader.TokenType);
            Assert.Equal(@"Value false is not defined in enum. Line 1, position 11.", validationEventArgs.Message);
            Assert.Equal("[1]", validationEventArgs.Path);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void ArrayCountGreaterThanMaximumItems()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

            string json = "[null,null,null,null]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
            Assert.Equal("Array item count 4 exceeds maximum count of 3. Line 1, position 21.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void ArrayCountLessThanMinimumItems()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

            string json = "[null]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
            Assert.Equal("Array item count 1 is less than minimum count of 2. Line 1, position 6.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void InvalidDataType()
        {
            string schemaJson = @"{
  ""type"":""string"",
  ""minItems"":2,
  ""maxItems"":3,
  ""items"":{}
}";

            string json = "[null,null,null,null]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);
            Assert.Equal(@"Invalid type. Expected String but got Array. Line 1, position 1.", validationEventArgs.Message);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void StringDisallowed()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""disallow"":[""number""]
  },
  ""maxItems"":3
}";

            string json = "['pie',1.1]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Float, reader.TokenType);
            Assert.Equal(@"Type Float is disallowed. Line 1, position 10.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void MissingRequiredProperties()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string"",""required"":true},
    ""age"":{""type"":""integer"",""required"":true}
  }
}";

            string json = "{'name':'James'}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("name", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("James", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);
            Assert.Equal("Required properties are missing from object: hobbies, age. Line 1, position 16.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void MissingNonRequiredProperties()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string"",""required"":true},
    ""hobbies"":{""type"":""string"",""required"":false},
    ""age"":{""type"":""integer""}
  }
}";

            string json = "{'name':'James'}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("name", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("James", reader.Value.ToString());
            Assert.Null(validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.Null(validationEventArgs);
        }

        [Fact]
        public void DisableAdditionalProperties()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

            string json = "{'name':'James','additionalProperty1':null,'additionalProperty2':null}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("name", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("James", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("additionalProperty1", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal(null, reader.Value);
            Assert.Equal("Property 'additionalProperty1' has not been defined and the schema does not allow additional properties. Line 1, position 38.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("additionalProperty2", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal(null, reader.Value);
            Assert.Equal("Property 'additionalProperty2' has not been defined and the schema does not allow additional properties. Line 1, position 65.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.NotNull(validationEventArgs);
        }

        [Fact]
        public void ExtendsStringGreaterThanMaximumLength()
        {
            string schemaJson = @"{
  ""extends"":{
    ""type"":""string"",
    ""minLength"":5,
    ""maxLength"":10
  },
  ""maxLength"":9
}";

            List<string> errors = new List<string>();
            string json = "'The quick brown fox jumps over the lazy dog.'";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) =>
            {
                validationEventArgs = args;
                errors.Add(validationEventArgs.Message);
            };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal(1, errors.Count);
            Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 9. Line 1, position 46.", errors[0]);

            Assert.NotNull(validationEventArgs);
        }

        private JsonSchema GetExtendedSchema()
        {
            string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string"",""required"":true}
  },
  ""additionalProperties"":{}
}";

            string second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""string"",""required"":true}
  },
  ""additionalProperties"":false
}";

            JsonSchemaResolver resolver = new JsonSchemaResolver();
            JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
            JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

            return secondSchema;
        }

        [Fact]
        public void ExtendsDisallowAdditionProperties()
        {
            string json = "{'firstproperty':'blah','secondproperty':'blah2','additional':'blah3','additional2':'blah4'}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = GetExtendedSchema();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("firstproperty", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("blah", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("secondproperty", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("blah2", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("additional", reader.Value.ToString());
            Assert.Equal("Property 'additional' has not been defined and the schema does not allow additional properties. Line 1, position 62.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("blah3", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("additional2", reader.Value.ToString());
            Assert.Equal("Property 'additional2' has not been defined and the schema does not allow additional properties. Line 1, position 84.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("blah4", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ExtendsMissingRequiredProperties()
        {
            string json = "{}";

            List<string> errors = new List<string>();

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { errors.Add(args.Message); };
            reader.Schema = GetExtendedSchema();

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.Equal(1, errors.Count);
            Assert.Equal("Required properties are missing from object: secondproperty, firstproperty. Line 1, position 2.", errors[0]);
        }

        [Fact]
        public void NoAdditionalItems()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""integer""}],
  ""additionalItems"": false
}";

            string json = @"[1, 'a', null]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal("Invalid type. Expected String but got Integer. Line 1, position 2.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("Invalid type. Expected Integer but got String. Line 1, position 7.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Null, reader.TokenType);
            Assert.Equal("Index 3 has not been defined and the schema does not allow additional items. Line 1, position 13.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void PatternPropertiesNoAdditionalProperties()
        {
            string schemaJson = @"{
  ""type"":""object"",
  ""patternProperties"": {
     ""hi"": {""type"":""string""},
     ""ho"": {""type"":""string""}
  },
  ""additionalProperties"": false
}";

            string json = @"{
  ""hi"": ""A string!"",
  ""hide"": ""A string!"",
  ""ho"": 1,
  ""hey"": ""A string!""
}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal("Invalid type. Expected String but got Integer. Line 4, position 10.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("Property 'hey' has not been defined and the schema does not allow additional properties. Line 5, position 9.", validationEventArgs.Message);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void ExtendedComplex()
        {
            string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string""},
    ""secondproperty"":{""type"":""string"",""maxLength"":10},
    ""thirdproperty"":{
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""type"":""string"",""maxLength"":10,""minLength"":7}
      }
    }
  },
  ""additionalProperties"":{}
}";

            string second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""any""},
    ""thirdproperty"":{
      ""extends"":{
        ""properties"":
        {
          ""thirdproperty_firstproperty"":{""maxLength"":9,""minLength"":6,""pattern"":""hi2u""}
        },
        ""additionalProperties"":{""maxLength"":9,""minLength"":6,""enum"":[""one"",""two""]}
      },
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""pattern"":""hi""}
      },
      ""additionalProperties"":{""type"":""string"",""enum"":[""two"",""three""]}
    },
    ""fourthproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

            JsonSchemaResolver resolver = new JsonSchemaResolver();
            JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
            JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

            JsonSchemaModelBuilder modelBuilder = new JsonSchemaModelBuilder();

            string json = @"{
  'firstproperty':'blahblahblahblahblahblah',
  'secondproperty':'secasecasecasecaseca',
  'thirdproperty':{
    'thirdproperty_firstproperty':'aaa',
    'additional':'three'
  }
}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;
            List<string> errors = new List<string>();

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) =>
            {
                validationEventArgs = args;
                errors.Add(validationEventArgs.Path + " - " + validationEventArgs.Message);
            };
            reader.Schema = secondSchema;

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("firstproperty", reader.Value.ToString());
            Assert.Equal(null, validationEventArgs);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("blahblahblahblahblahblah", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("secondproperty", reader.Value.ToString());

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("secasecasecasecaseca", reader.Value.ToString());
            Assert.Equal(1, errors.Count);
            Assert.Equal("secondproperty - String 'secasecasecasecaseca' exceeds maximum length of 10. Line 3, position 42.", errors[0]);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("thirdproperty", reader.Value.ToString());
            Assert.Equal(1, errors.Count);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);
            Assert.Equal(1, errors.Count);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("thirdproperty_firstproperty", reader.Value.ToString());
            Assert.Equal(1, errors.Count);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("aaa", reader.Value.ToString());
            Assert.Equal(4, errors.Count);
            Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' is less than minimum length of 7. Line 5, position 40.", errors[1]);
            Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi'. Line 5, position 40.", errors[2]);
            Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi2u'. Line 5, position 40.", errors[3]);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);
            Assert.Equal("additional", reader.Value.ToString());
            Assert.Equal(4, errors.Count);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);
            Assert.Equal("three", reader.Value.ToString());
            Assert.Equal(5, errors.Count);
            Assert.Equal("thirdproperty.additional - String 'three' is less than minimum length of 6. Line 6, position 25.", errors[4]);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void DuplicateErrorsTest()
        {
            string schema = @"{
  ""id"":""ErrorDemo.Database"",
  ""properties"":{
    ""ErrorDemoDatabase"":{
      ""type"":""object"",
      ""required"":true,
      ""properties"":{
        ""URL"":{
          ""type"":""string"",
          ""required"":true
        },
        ""Version"":{
          ""type"":""string"",
          ""required"":true
        },
        ""Date"":{
          ""type"":""string"",
          ""format"":""date-time"",
          ""required"":true
        },
        ""MACLevels"":{
          ""type"":""object"",
          ""required"":true,
          ""properties"":{
            ""MACLevel"":{
              ""type"":""array"",
              ""required"":true,
              ""items"":[
                {
                  ""required"":true,
                  ""properties"":{
                    ""IDName"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""Order"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""IDDesc"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""IsActive"":{
                      ""type"":""string"",
                      ""required"":true
                    }
                  }
                }
              ]
            }
          }
        }
      }
    }
  }
}";

            string json = @"{
  ""ErrorDemoDatabase"":{
    ""URL"":""localhost:3164"",
    ""Version"":""1.0"",
    ""Date"":""6.23.2010, 9:35:18.121"",
    ""MACLevels"":{
      ""MACLevel"":[
        {
          ""@IDName"":""Developer"",
          ""Order"":""0"",
          ""IDDesc"":""DeveloperDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Technician"",
          ""Order"":""1"",
          ""IDDesc"":""TechnicianDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Administrator"",
          ""Order"":""2"",
          ""IDDesc"":""AdministratorDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""PowerUser"",
          ""Order"":""3"",
          ""IDDesc"":""PowerUserDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Operator"",
          ""Order"":""4"",
          ""IDDesc"":""OperatorDesc"",
          ""IsActive"":""True""
        }
      ]
    }
  }
}";

            IList<ValidationEventArgs> validationEventArgs = new List<ValidationEventArgs>();

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs.Add(args); };
            reader.Schema = JsonSchema.Parse(schema);

            while (reader.Read())
            {
            }

            Assert.Equal(1, validationEventArgs.Count);
        }

        [Fact]
        public void ReadAsBytes()
        {
            JsonSchema s = new JsonSchemaGenerator().Generate(typeof(byte[]));

            byte[] data = Encoding.UTF8.GetBytes("Hello world");

            JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"""" + Convert.ToBase64String(data) + @"""")))
            {
                Schema = s
            };
            byte[] bytes = reader.ReadAsBytes();

            Assert.Equal(data, bytes);
        }

        [Fact]
        public void ReadAsInt32()
        {
            JsonSchema s = new JsonSchemaGenerator().Generate(typeof(int));

            JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"1")))
            {
                Schema = s
            };
            int? i = reader.ReadAsInt32();

            Assert.Equal(1, i);
        }

        [Fact]
        public void ReadAsInt32Failure()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema s = new JsonSchemaGenerator().Generate(typeof(int));
                s.Maximum = 2;

                JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"5")))
                {
                    Schema = s
                };
                reader.ReadAsInt32();
            }, "Integer 5 exceeds maximum value of 2. Line 1, position 1.");
        }

        [Fact]
        public void ReadAsDecimal()
        {
            JsonSchema s = new JsonSchemaGenerator().Generate(typeof(decimal));

            JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"1.5")))
            {
                Schema = s
            };
            decimal? d = reader.ReadAsDecimal();

            Assert.Equal(1.5m, d);
        }

        [Fact]
        public void ReadAsDecimalFailure()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema s = new JsonSchemaGenerator().Generate(typeof(decimal));
                s.DivisibleBy = 1;

                JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"5.5")))
                {
                    Schema = s
                };
                reader.ReadAsDecimal();
            }, "Float 5.5 is not evenly divisible by 1. Line 1, position 3.");
        }

        [Fact]
        public void ReadAsInt32FromSerializer()
        {
            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader("[1,2,3]")));
            reader.Schema = new JsonSchemaGenerator().Generate(typeof(int[]));
            int[] values = new JsonSerializer().Deserialize<int[]>(reader);

            Assert.Equal(3, values.Length);
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
            Assert.Equal(3, values[2]);
        }

        [Fact]
        public void ReadAsInt32InArray()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer""
  },
  ""maxItems"":1
}";

            string json = "[1,2]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            reader.Read();
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.EndArray, reader.TokenType);
            Assert.Equal("Array item count 2 exceeds maximum count of 1. Line 1, position 5.", validationEventArgs.Message);
            Assert.Equal("", validationEventArgs.Path);
        }

        [Fact]
        public void ReadAsInt32InArrayIncomplete()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer""
  },
  ""maxItems"":1
}";

            string json = "[1,2";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            reader.Read();
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, reader.TokenType);
            Assert.Equal(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.Equal(JsonToken.None, reader.TokenType);
            Assert.Equal(null, validationEventArgs);
        }
    }
}