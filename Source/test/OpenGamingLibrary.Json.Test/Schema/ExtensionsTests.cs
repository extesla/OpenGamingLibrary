﻿// Copyright (c) 2007 James Newton-King
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
using Xunit;
using OpenGamingLibrary.Json.Schema;
using OpenGamingLibrary.Json.Linq;
using System.IO;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Schema
{
    
    public class ExtensionsTests : TestFixtureBase
    {
        [Fact]
        public void IsValid()
        {
            JsonSchema schema = JsonSchema.Parse("{'type':'integer'}");
            JToken stringToken = JToken.FromObject("pie");
            JToken integerToken = JToken.FromObject(1);

            IList<string> errorMessages;
            Assert.Equal(true, integerToken.IsValid(schema));
            Assert.Equal(true, integerToken.IsValid(schema, out errorMessages));
            Assert.Equal(0, errorMessages.Count);

            Assert.Equal(false, stringToken.IsValid(schema));
            Assert.Equal(false, stringToken.IsValid(schema, out errorMessages));
            Assert.Equal(1, errorMessages.Count);
            Assert.Equal("Invalid type. Expected Integer but got String.", errorMessages[0]);
        }

        [Fact]
        public void ValidateWithEventHandler()
        {
            JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
            JToken stringToken = JToken.FromObject("pie lol");

            List<string> errors = new List<string>();
            stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
            Assert.Equal(0, errors.Count);

            stringToken = JToken.FromObject("pie");

            stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
            Assert.Equal(1, errors.Count);

            Assert.Equal("String 'pie' does not match regex pattern 'lol'.", errors[0]);
        }

        [Fact]
        public void ValidateWithOutEventHandlerFailure()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
                JToken stringToken = JToken.FromObject("pie");
                stringToken.Validate(schema);
            }, @"String 'pie' does not match regex pattern 'lol'.");
        }

        [Fact]
        public void ValidateWithOutEventHandlerSuccess()
        {
            JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
            JToken stringToken = JToken.FromObject("pie lol");
            stringToken.Validate(schema);
        }

        [Fact]
        public void ValidateFailureWithOutLineInfoBecauseOfEndToken()
        {
            // changed in 6.0.6 to now include line info!
            JsonSchema schema = JsonSchema.Parse("{'properties':{'lol':{'required':true}}}");
            JObject o = JObject.Parse("{}");

            List<string> errors = new List<string>();
            o.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.Equal("Required properties are missing from object: lol. Line 1, position 1.", errors[0]);
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void ValidateRequiredFieldsWithLineInfo()
        {
            JsonSchema schema = JsonSchema.Parse("{'properties':{'lol':{'type':'string'}}}");
            JObject o = JObject.Parse("{'lol':1}");

            List<string> errors = new List<string>();
            o.Validate(schema, (sender, args) => errors.Add(args.Path + " - " + args.Message));

            Assert.Equal("lol - Invalid type. Expected String but got Integer. Line 1, position 8.", errors[0]);
            Assert.Equal("1", o.SelectToken("lol").ToString());
            Assert.Equal(1, errors.Count);
        }

        [Fact]
        public void Blog()
        {
            string schemaJson = @"
{
  ""description"": ""A person schema"",
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

            //JsonSchema schema;

            //using (JsonTextReader reader = new JsonTextReader(new StringReader(schemaJson)))
            //{
            //  JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            //  schema = builder.Parse(reader);
            //}

            JsonSchema schema = JsonSchema.Parse(schemaJson);

            JObject person = JObject.Parse(@"{
        ""name"": ""James"",
        ""hobbies"": ["".NET"", ""Blogging"", ""Reading"", ""Xbox"", ""LOLCATS""]
      }");

            bool valid = person.IsValid(schema);
            // true
        }

        private void GenerateSchemaAndSerializeFromType<T>(T value)
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
            JsonSchema typeSchema = generator.Generate(typeof(T));
            string schema = typeSchema.ToString();

            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            JToken token = JToken.ReadFrom(new JsonTextReader(new StringReader(json)));

            List<string> errors = new List<string>();

            token.Validate(typeSchema, (sender, args) => { errors.Add(args.Message); });

			if (errors.Count > 0) {
				Assert.True(false);
				// TODO: Assert.Fail("Schema generated for type '{0}' is not valid." + Environment.NewLine + string.Join(Environment.NewLine, errors.ToArray()), typeof(T));
			}
        }

        [Fact]
        public void GenerateSchemaAndSerializeFromTypeTests()
        {
            GenerateSchemaAndSerializeFromType(new List<string> { "1", "Two", "III" });
            GenerateSchemaAndSerializeFromType(new List<int> { 1 });
            GenerateSchemaAndSerializeFromType(new Version("1.2.3.4"));
            GenerateSchemaAndSerializeFromType(new Store());
            GenerateSchemaAndSerializeFromType(new Person());
            GenerateSchemaAndSerializeFromType(new PersonRaw());
            GenerateSchemaAndSerializeFromType(new CircularReferenceClass() { Name = "I'm required" });
            GenerateSchemaAndSerializeFromType(new CircularReferenceWithIdClass());
            GenerateSchemaAndSerializeFromType(new ClassWithArray());
            GenerateSchemaAndSerializeFromType(new ClassWithGuid());
            GenerateSchemaAndSerializeFromType(new NullableDateTimeTestClass());
            GenerateSchemaAndSerializeFromType(new object());
            GenerateSchemaAndSerializeFromType(1);
            GenerateSchemaAndSerializeFromType("Hi");
            GenerateSchemaAndSerializeFromType(new DateTime(2000, 12, 29, 23, 59, 0, DateTimeKind.Utc));
            GenerateSchemaAndSerializeFromType(TimeSpan.FromTicks(1000000));
            GenerateSchemaAndSerializeFromType(new JsonPropertyWithHandlingValues());
        }

        [Fact]
        public void UndefinedPropertyOnNoPropertySchema()
        {
            JsonSchema schema = JsonSchema.Parse(@"{
  ""description"": ""test"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
  }
}");

            JObject o = JObject.Parse("{'g':1}");

            List<string> errors = new List<string>();
            o.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.Equal(1, errors.Count);
            Assert.Equal("Property 'g' has not been defined and the schema does not allow additional properties. Line 1, position 5.", errors[0]);
        }

        [Fact]
        public void ExclusiveMaximum_Int()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = new JsonSchema();
                schema.Maximum = 10;
                schema.ExclusiveMaximum = true;

                JValue v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 equals maximum value of 10 and exclusive maximum is true.");
        }

        [Fact]
        public void ExclusiveMaximum_Float()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = new JsonSchema();
                schema.Maximum = 10.1;
                schema.ExclusiveMaximum = true;

                JValue v = new JValue(10.1);
                v.Validate(schema);
            }, "Float 10.1 equals maximum value of 10.1 and exclusive maximum is true.");
        }

        [Fact]
        public void ExclusiveMinimum_Int()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = new JsonSchema();
                schema.Minimum = 10;
                schema.ExclusiveMinimum = true;

                JValue v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 equals minimum value of 10 and exclusive minimum is true.");
        }

        [Fact]
        public void ExclusiveMinimum_Float()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = new JsonSchema();
                schema.Minimum = 10.1;
                schema.ExclusiveMinimum = true;

                JValue v = new JValue(10.1);
                v.Validate(schema);
            }, "Float 10.1 equals minimum value of 10.1 and exclusive minimum is true.");
        }

        [Fact]
        public void DivisibleBy_Int()
        {
            AssertException.Throws<JsonSchemaException>(() =>
            {
                JsonSchema schema = new JsonSchema();
                schema.DivisibleBy = 3;

                JValue v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 is not evenly divisible by 3.");
        }

        [Fact]
        public void DivisibleBy_Approx()
        {
            JsonSchema schema = new JsonSchema();
            schema.DivisibleBy = 0.01;

            JValue v = new JValue(20.49);
            v.Validate(schema);
        }

        [Fact]
        public void UniqueItems_SimpleUnique()
        {
            JsonSchema schema = new JsonSchema();
            schema.UniqueItems = true;

            JArray a = new JArray(1, 2, 3);
            Assert.True(a.IsValid(schema));
        }

        [Fact]
        public void UniqueItems_SimpleDuplicate()
        {
            JsonSchema schema = new JsonSchema();
            schema.UniqueItems = true;

            JArray a = new JArray(1, 2, 3, 2, 2);
            IList<string> errorMessages;
            Assert.False(a.IsValid(schema, out errorMessages));
            Assert.Equal(2, errorMessages.Count);
            Assert.Equal("Non-unique array item at index 3.", errorMessages[0]);
            Assert.Equal("Non-unique array item at index 4.", errorMessages[1]);
        }

        [Fact]
        public void UniqueItems_ComplexDuplicate()
        {
            JsonSchema schema = new JsonSchema();
            schema.UniqueItems = true;

            JArray a = new JArray(1, new JObject(new JProperty("value", "value!")), 3, 2, new JObject(new JProperty("value", "value!")), 4, 2, new JObject(new JProperty("value", "value!")));
            IList<string> errorMessages;
            Assert.False(a.IsValid(schema, out errorMessages));
            Assert.Equal(3, errorMessages.Count);
            Assert.Equal("Non-unique array item at index 4.", errorMessages[0]);
            Assert.Equal("Non-unique array item at index 6.", errorMessages[1]);
            Assert.Equal("Non-unique array item at index 7.", errorMessages[2]);
        }

        [Fact]
        public void UniqueItems_NestedDuplicate()
        {
            JsonSchema schema = new JsonSchema();
            schema.UniqueItems = true;
            schema.Items = new List<JsonSchema>
            {
                new JsonSchema
                {
                    UniqueItems = true
                }
            };
            schema.PositionalItemsValidation = false;

            JArray a = new JArray(
                new JArray(1, 2),
                new JArray(1, 1),
                new JArray(3, 4),
                new JArray(1, 2),
                new JArray(1, 1)
                );
            IList<string> errorMessages;
            Assert.False(a.IsValid(schema, out errorMessages));
            Assert.Equal(4, errorMessages.Count);
            Assert.Equal("Non-unique array item at index 1.", errorMessages[0]);
            Assert.Equal("Non-unique array item at index 3.", errorMessages[1]);
            Assert.Equal("Non-unique array item at index 1.", errorMessages[2]);
            Assert.Equal("Non-unique array item at index 4.", errorMessages[3]);
        }

        [Fact]
        public void Enum_Properties()
        {
            JsonSchema schema = new JsonSchema();
            schema.Properties = new Dictionary<string, JsonSchema>
            {
                {
                    "bar",
                    new JsonSchema
                    {
                        Enum = new List<JToken>
                        {
                            new JValue(1),
                            new JValue(2)
                        }
                    }
                }
            };

            JObject o = new JObject(
                new JProperty("bar", 1)
                );
            IList<string> errorMessages;
            Assert.True(o.IsValid(schema, out errorMessages));
            Assert.Equal(0, errorMessages.Count);

            o = new JObject(
                new JProperty("bar", 3)
                );
            Assert.False(o.IsValid(schema, out errorMessages));
            Assert.Equal(1, errorMessages.Count);
        }

        [Fact]
        public void UniqueItems_Property()
        {
            JsonSchema schema = new JsonSchema();
            schema.Properties = new Dictionary<string, JsonSchema>
            {
                {
                    "bar",
                    new JsonSchema
                    {
                        UniqueItems = true
                    }
                }
            };

            JObject o = new JObject(
                new JProperty("bar", new JArray(1, 2, 3, 3))
                );
            IList<string> errorMessages;
            Assert.False(o.IsValid(schema, out errorMessages));
            Assert.Equal(1, errorMessages.Count);
        }

        [Fact]
        public void Items_Positional()
        {
            JsonSchema schema = new JsonSchema();
            schema.Items = new List<JsonSchema>
            {
                new JsonSchema { Type = JsonSchemaType.Object },
                new JsonSchema { Type = JsonSchemaType.Integer }
            };
            schema.PositionalItemsValidation = true;

            JArray a = new JArray(new JObject(), 1);
            IList<string> errorMessages;
            Assert.True(a.IsValid(schema, out errorMessages));
            Assert.Equal(0, errorMessages.Count);
        }
    }
}