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
using Xunit;
using OpenGamingLibrary.Json.Schema;
using System.IO;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Schema
{
    
    public class JsonSchemaBuilderTests : TestFixtureBase
    {
        [Fact]
        public void Simple()
        {
            const string json = @"
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

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("A person", schema.Description);
            Assert.Equal(JsonSchemaType.Object, schema.Type);

            Assert.Equal(2, schema.Properties.Count);

            Assert.Equal(JsonSchemaType.String, schema.Properties["name"].Type);
            Assert.Equal(JsonSchemaType.Array, schema.Properties["hobbies"].Type);
            Assert.Equal(JsonSchemaType.String, schema.Properties["hobbies"].Items[0].Type);
        }

        [Fact]
        public void MultipleTypes()
        {
            const string json = @"{
  ""description"":""Age"",
  ""type"":[""string"", ""integer""]
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Age", schema.Description);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Integer, schema.Type);
        }

        [Fact]
        public void MultipleItems()
        {
            const string json = @"{
  ""description"":""MultipleItems"",
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""array""}]
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("MultipleItems", schema.Description);
            Assert.Equal(JsonSchemaType.String, schema.Items[0].Type);
            Assert.Equal(JsonSchemaType.Array, schema.Items[1].Type);
        }

        [Fact]
        public void AdditionalProperties()
        {
            const string json = @"{
  ""description"":""AdditionalProperties"",
  ""type"":[""string"", ""integer""],
  ""additionalProperties"":{""type"":[""object"", ""boolean""]}
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("AdditionalProperties", schema.Description);
            Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Boolean, schema.AdditionalProperties.Type);
        }

        [Fact]
        public void Required()
        {
            const string json = @"{
  ""description"":""Required"",
  ""required"":true
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Required", schema.Description);
            Assert.Equal(true, schema.Required);
        }

        [Fact]
        public void ExclusiveMinimumExclusiveMaximum()
        {
            const string json = @"{
  ""exclusiveMinimum"":true,
  ""exclusiveMaximum"":true
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(true, schema.ExclusiveMinimum);
            Assert.Equal(true, schema.ExclusiveMaximum);
        }

        [Fact]
        public void ReadOnly()
        {
            const string json = @"{
  ""description"":""ReadOnly"",
  ""readonly"":true
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("ReadOnly", schema.Description);
            Assert.Equal(true, schema.ReadOnly);
        }

        [Fact]
        public void Hidden()
        {
            const string json = @"{
  ""description"":""Hidden"",
  ""hidden"":true
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Hidden", schema.Description);
            Assert.Equal(true, schema.Hidden);
        }

        [Fact]
        public void Id()
        {
            const string json = @"{
  ""description"":""Id"",
  ""id"":""testid""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Id", schema.Description);
            Assert.Equal("testid", schema.Id);
        }

        [Fact]
        public void Title()
        {
            const string json = @"{
  ""description"":""Title"",
  ""title"":""testtitle""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Title", schema.Description);
            Assert.Equal("testtitle", schema.Title);
        }

        [Fact]
        public void Pattern()
        {
            const string json = @"{
  ""description"":""Pattern"",
  ""pattern"":""testpattern""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Pattern", schema.Description);
            Assert.Equal("testpattern", schema.Pattern);
        }

        [Fact]
        public void Format()
        {
            const string json = @"{
  ""description"":""Format"",
  ""format"":""testformat""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Format", schema.Description);
            Assert.Equal("testformat", schema.Format);
        }

        [Fact]
        public void Requires()
        {
            const string json = @"{
  ""description"":""Requires"",
  ""requires"":""PurpleMonkeyDishwasher""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Requires", schema.Description);
            Assert.Equal("PurpleMonkeyDishwasher", schema.Requires);
        }

        [Fact]
        public void MinimumMaximum()
        {
            const string json = @"{
  ""description"":""MinimumMaximum"",
  ""minimum"":1.1,
  ""maximum"":1.2,
  ""minItems"":1,
  ""maxItems"":2,
  ""minLength"":5,
  ""maxLength"":50,
  ""divisibleBy"":3,
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("MinimumMaximum", schema.Description);
            Assert.Equal(1.1, schema.Minimum);
            Assert.Equal(1.2, schema.Maximum);
            Assert.Equal(1, schema.MinimumItems);
            Assert.Equal(2, schema.MaximumItems);
            Assert.Equal(5, schema.MinimumLength);
            Assert.Equal(50, schema.MaximumLength);
            Assert.Equal(3, schema.DivisibleBy);
        }

        [Fact]
        public void DisallowSingleType()
        {
            const string json = @"{
  ""description"":""DisallowSingleType"",
  ""disallow"":""string""
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("DisallowSingleType", schema.Description);
            Assert.Equal(JsonSchemaType.String, schema.Disallow);
        }

        [Fact]
        public void DisallowMultipleTypes()
        {
            const string json = @"{
  ""description"":""DisallowMultipleTypes"",
  ""disallow"":[""string"",""number""]
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("DisallowMultipleTypes", schema.Description);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Float, schema.Disallow);
        }

        [Fact]
        public void DefaultPrimitiveType()
        {
            const string json = @"{
  ""description"":""DefaultPrimitiveType"",
  ""default"":1.1
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("DefaultPrimitiveType", schema.Description);
            Assert.Equal(1.1, (double)schema.Default);
        }

        [Fact]
        public void DefaultComplexType()
        {
            const string json = @"{
  ""description"":""DefaultComplexType"",
  ""default"":{""pie"":true}
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("DefaultComplexType", schema.Description);
            Assert.True(JToken.DeepEquals(JObject.Parse(@"{""pie"":true}"), schema.Default));
        }

        [Fact]
        public void Enum()
        {
            const string json = @"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""enum"":[""string"",""object"",""array"",""boolean"",""number"",""integer"",""null"",""any""]
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("Type", schema.Description);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Array, schema.Type);

            Assert.Equal(8, schema.Enum.Count);
            Assert.Equal("string", (string)schema.Enum[0]);
            Assert.Equal("any", (string)schema.Enum[schema.Enum.Count - 1]);
        }

        [Fact]
        public void CircularReference()
        {
            const string json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""CircularReferenceArray""}
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("CircularReference", schema.Description);
            Assert.Equal("CircularReferenceArray", schema.Id);
            Assert.Equal(JsonSchemaType.Array, schema.Type);

            Assert.Equal(schema, schema.Items[0]);
        }

        [Fact]
        public void UnresolvedReference()
        {
            AssertException.Throws<Exception>(() =>
            {
                const string json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""MyUnresolvedReference""}
}";

                var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));
            }, @"Could not resolve schema reference 'MyUnresolvedReference'.");
        }

        [Fact]
        public void PatternProperties()
        {
            const string json = @"{
  ""patternProperties"": {
    ""[abc]"": { ""id"":""Blah"" }
  }
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.NotNull(schema.PatternProperties);
            Assert.Equal(1, schema.PatternProperties.Count);
            Assert.Equal("Blah", schema.PatternProperties["[abc]"].Id);
        }

        [Fact]
        public void AdditionalItems()
        {
            const string json = @"{
    ""items"": [],
    ""additionalItems"": {""type"": ""integer""}
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.NotNull(schema.AdditionalItems);
            Assert.Equal(JsonSchemaType.Integer, schema.AdditionalItems.Type);
            Assert.Equal(true, schema.AllowAdditionalItems);
        }

        [Fact]
        public void DisallowAdditionalItems()
        {
            const string json = @"{
    ""items"": [],
    ""additionalItems"": false
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Null(schema.AdditionalItems);
            Assert.Equal(false, schema.AllowAdditionalItems);
        }

        [Fact]
        public void AllowAdditionalItems()
        {
            const string json = @"{
    ""items"": {},
    ""additionalItems"": false
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Null(schema.AdditionalItems);
            Assert.Equal(false, schema.AllowAdditionalItems);
        }

        [Fact]
        public void Location()
        {
            const string json = @"{
  ""properties"":{
    ""foo"":{
      ""type"":""array"",
      ""items"":[
        {
          ""type"":""integer""
        },
        {
          ""properties"":{
            ""foo"":{
              ""type"":""integer""
            }
          }
        }
      ]
    }
  }
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal("#", schema.Location);
            Assert.Equal("#/properties/foo", schema.Properties["foo"].Location);
            Assert.Equal("#/properties/foo/items/1/properties/foo", schema.Properties["foo"].Items[1].Properties["foo"].Location);
        }

        [Fact]
        public void ReferenceBackwardsLocation()
        {
            const string json = @"{
  ""properties"": {
    ""foo"": {""type"": ""integer""},
    ""bar"": {""$ref"": ""#/properties/foo""}
  }
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Fact]
        public void ReferenceForwardsLocation()
        {
            const string json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/properties/foo""},
    ""foo"": {""type"": ""integer""}
  }
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Fact]
        public void ReferenceNonStandardLocation()
        {
            const string json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/common/foo""},
    ""foo"": {""$ref"": ""#/common/foo""}
  },
  ""common"": {
    ""foo"": {""type"": ""integer""}
  }
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Fact]
        public void EscapedReferences()
        {
            const string json = @"{
            ""tilda~field"": {""type"": ""integer""},
            ""slash/field"": {""type"": ""object""},
            ""percent%field"": {""type"": ""array""},
            ""properties"": {
                ""tilda"": {""$ref"": ""#/tilda~0field""},
                ""slash"": {""$ref"": ""#/slash~1field""},
                ""percent"": {""$ref"": ""#/percent%25field""}
            }
        }";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(JsonSchemaType.Integer, schema.Properties["tilda"].Type);
            Assert.Equal(JsonSchemaType.Object, schema.Properties["slash"].Type);
            Assert.Equal(JsonSchemaType.Array, schema.Properties["percent"].Type);
        }

        [Fact]
        public void ReferencesArray()
        {
            const string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/1/prop""}
            }
        }";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.Equal(JsonSchemaType.Integer, schema.Properties["array"].Type);
            Assert.Equal(JsonSchemaType.Object, schema.Properties["arrayprop"].Type);
        }

        [Fact]
        public void ReferencesIndexTooBig()
        {
            // JsonException : Could not resolve schema reference '#/array/10'.

            const string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/10""}
            }
        }";

            AssertException.Throws<JsonException>(() =>
            {
                var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                builder.Read(new JsonTextReader(new StringReader(json)));
            }, "Could not resolve schema reference '#/array/10'.");
        }

        [Fact]
        public void ReferencesIndexNegative()
        {
            const string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/-1""}
            }
        }";

            AssertException.Throws<JsonException>(() =>
            {
                var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                builder.Read(new JsonTextReader(new StringReader(json)));
            }, "Could not resolve schema reference '#/array/-1'.");
        }

        [Fact]
        public void ReferencesIndexNotInteger()
        {
            const string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/one""}
            }
        }";

            AssertException.Throws<JsonException>(() =>
            {
                var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                builder.Read(new JsonTextReader(new StringReader(json)));
            }, "Could not resolve schema reference '#/array/one'.");
        }
    }
}