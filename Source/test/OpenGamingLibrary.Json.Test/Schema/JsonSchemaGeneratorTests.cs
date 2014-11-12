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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Json.Utilities;
using OpenGamingLibrary.Json.Schema;
using OpenGamingLibrary.Json.Linq;
using Extensions = OpenGamingLibrary.Json.Schema.Extensions;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;

namespace OpenGamingLibrary.Json.Test.Schema
{
    
    public class JsonSchemaGeneratorTests : TestFixtureBase
    {
        [Fact]
        public void Generate_GenericDictionary()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Dictionary<string, List<string>>));

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": [
      ""array"",
      ""null""
    ],
    ""items"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    }
  }
}", json);

            Dictionary<string, List<string>> value = new Dictionary<string, List<string>>
            {
                { "HasValue", new List<string>() { "first", "second", null } },
                { "NoValue", null }
            };

            string valueJson = JsonConvert.SerializeObject(value, Formatting.Indented);
            JObject o = JObject.Parse(valueJson);

            Assert.True(o.IsValid(schema));
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void Generate_DefaultValueAttributeTestClass()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(DefaultValueAttributeTestClass));

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""description"": ""DefaultValueAttributeTestClass description!"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""TestField1"": {
      ""required"": true,
      ""type"": ""integer"",
      ""default"": 21
    },
    ""TestProperty1"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""TestProperty1Value""
    }
  }
}", json);
        }
#endif

        [Fact]
        public void Generate_Person()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Person));

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""Person"",
  ""title"": ""Title!"",
  ""description"": ""JsonObjectAttribute description!"",
  ""type"": ""object"",
  ""properties"": {
    ""Name"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""BirthDate"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastModified"": {
      ""required"": true,
      ""type"": ""string""
    }
  }
}", json);
        }

        [Fact]
        public void Generate_UserNullable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(UserNullable));

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""type"": ""object"",
  ""properties"": {
    ""Id"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""FName"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""LName"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""RoleId"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""NullableRoleId"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""NullRoleId"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""Active"": {
      ""required"": true,
      ""type"": [
        ""boolean"",
        ""null""
      ]
    }
  }
}", json);
        }

        [Fact]
        public void Generate_RequiredMembersClass()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(RequiredMembersClass));

            Assert.Equal(JsonSchemaType.String, schema.Properties["FirstName"].Type);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["MiddleName"].Type);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["LastName"].Type);
            Assert.Equal(JsonSchemaType.String, schema.Properties["BirthDate"].Type);
        }

        [Fact]
        public void Generate_Store()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            JsonSchema schema = generator.Generate(typeof(Store));

            Assert.Equal(11, schema.Properties.Count);

            JsonSchema productArraySchema = schema.Properties["product"];
            JsonSchema productSchema = productArraySchema.Items[0];

            Assert.Equal(4, productSchema.Properties.Count);
        }

        [Fact]
        public void MissingSchemaIdHandlingTest()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(Store));
            Assert.Equal(null, schema.Id);

            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            schema = generator.Generate(typeof(Store));
            Assert.Equal(typeof(Store).FullName, schema.Id);

            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
            schema = generator.Generate(typeof(Store));
            Assert.Equal(typeof(Store).AssemblyQualifiedName, schema.Id);
        }

        [Fact]
        public void CircularReferenceError()
        {
            AssertException.Throws<Exception>(() =>
            {
                JsonSchemaGenerator generator = new JsonSchemaGenerator();
                generator.Generate(typeof(CircularReferenceClass));
            }, @"Unresolved circular reference for type 'OpenGamingLibrary.Json.Test.TestObjects.CircularReferenceClass'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.");
        }

        [Fact]
        public void CircularReferenceWithTypeNameId()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(CircularReferenceClass), true);

            Assert.Equal(JsonSchemaType.String, schema.Properties["Name"].Type);
            Assert.Equal(typeof(CircularReferenceClass).FullName, schema.Id);
            Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
            Assert.Equal(schema, schema.Properties["Child"]);
        }

        [Fact]
        public void CircularReferenceWithExplicitId()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(CircularReferenceWithIdClass));

            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["Name"].Type);
            Assert.Equal("MyExplicitId", schema.Id);
            Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
            Assert.Equal(schema, schema.Properties["Child"]);
        }

        [Fact]
        public void GenerateSchemaForType()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Type));

            Assert.Equal(JsonSchemaType.String, schema.Type);

            string json = JsonConvert.SerializeObject(typeof(Version), Formatting.Indented);

            JValue v = new JValue(json);
            Assert.True(v.IsValid(schema));
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void GenerateSchemaForISerializable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Exception));

            Assert.Equal(JsonSchemaType.Object, schema.Type);
            Assert.Equal(true, schema.AllowAdditionalProperties);
            Assert.Equal(null, schema.Properties);
        }
#endif

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void GenerateSchemaForDBNull()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(DBNull));

            Assert.Equal(JsonSchemaType.Null, schema.Type);
        }

        public class CustomDirectoryInfoMapper : DefaultContractResolver
        {
            public CustomDirectoryInfoMapper()
                : base(true)
            {
            }

            protected override JsonContract CreateContract(Type objectType)
            {
                if (objectType == typeof(DirectoryInfo))
                    return base.CreateObjectContract(objectType);

                return base.CreateContract(objectType);
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                JsonPropertyCollection c = new JsonPropertyCollection(type);
                c.AddRange(properties.Where(m => m.PropertyName != "Root"));

                return c;
            }
        }

        [Fact]
        public void GenerateSchemaForDirectoryInfo()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            generator.ContractResolver = new CustomDirectoryInfoMapper
            {
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50)
                IgnoreSerializableAttribute = true
#endif
            };

            JsonSchema schema = generator.Generate(typeof(DirectoryInfo), true);

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""System.IO.DirectoryInfo"",
  ""required"": true,
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""Name"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""Parent"": {
      ""$ref"": ""System.IO.DirectoryInfo""
    },
    ""Exists"": {
      ""required"": true,
      ""type"": ""boolean""
    },
    ""FullName"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""Extension"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""CreationTime"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""CreationTimeUtc"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastAccessTime"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastAccessTimeUtc"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastWriteTime"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastWriteTimeUtc"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""Attributes"": {
      ""required"": true,
      ""type"": ""integer""
    }
  }
}", json);

            DirectoryInfo temp = new DirectoryInfo(@"c:\temp");

            JTokenWriter jsonWriter = new JTokenWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IsoDateTimeConverter());
            serializer.ContractResolver = new CustomDirectoryInfoMapper
            {
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50)
                IgnoreSerializableInterface = true
#endif
            };
            serializer.Serialize(jsonWriter, temp);

            List<string> errors = new List<string>();
            jsonWriter.Token.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.Equal(0, errors.Count);
        }
#endif

        [Fact]
        public void GenerateSchemaCamelCase()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            generator.ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
                IgnoreSerializableAttribute = true
#endif
            };

            JsonSchema schema = generator.Generate(typeof(Version), true);

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""System.Version"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""major"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""minor"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""build"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""revision"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""majorRevision"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""minorRevision"": {
      ""required"": true,
      ""type"": ""integer""
    }
  }
}", json);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void GenerateSchemaSerializable()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();
            generator.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

            JsonSchema schema = generator.Generate(typeof(Version), true);

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""System.Version"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""_Major"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""_Minor"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""_Build"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""_Revision"": {
      ""required"": true,
      ""type"": ""integer""
    }
  }
}", json);

            JTokenWriter jsonWriter = new JTokenWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            serializer.Serialize(jsonWriter, new Version(1, 2, 3, 4));

            List<string> errors = new List<string>();
            jsonWriter.Token.Validate(schema, (sender, args) => errors.Add(args.Message));

            Assert.Equal(0, errors.Count);

            StringAssert.Equal(@"{
  ""_Major"": 1,
  ""_Minor"": 2,
  ""_Build"": 3,
  ""_Revision"": 4
}", jsonWriter.Token.ToString());

            Version version = jsonWriter.Token.ToObject<Version>(serializer);
            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Build);
            Assert.Equal(4, version.Revision);
        }
#endif

        public enum SortTypeFlag
        {
            No = 0,
            Asc = 1,
            Desc = -1
        }

        public class X
        {
            public SortTypeFlag x;
        }

        [Fact]
        public void GenerateSchemaWithNegativeEnum()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();
            JsonSchema schema = jsonSchemaGenerator.Generate(typeof(X));

            string json = schema.ToString();

            StringAssert.Equal(@"{
  ""type"": ""object"",
  ""properties"": {
    ""x"": {
      ""required"": true,
      ""type"": ""integer"",
      ""enum"": [
        0,
        1,
        -1
      ]
    }
  }
}", json);
        }

        [Fact]
        public void CircularCollectionReferences()
        {
            Type type = typeof(Workspace);
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(type);

            // should succeed
            Assert.NotNull(jsonSchema);
        }

        [Fact]
        public void CircularReferenceWithMixedRequires()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(CircularReferenceClass));
            string json = jsonSchema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""OpenGamingLibrary.Json.Test.TestObjects.CircularReferenceClass"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""Name"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""Child"": {
      ""$ref"": ""OpenGamingLibrary.Json.Test.TestObjects.CircularReferenceClass""
    }
  }
}", json);
        }

        [Fact]
        public void JsonPropertyWithHandlingValues()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            jsonSchemaGenerator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(JsonPropertyWithHandlingValues));
            string json = jsonSchema.ToString();

            StringAssert.Equal(@"{
  ""id"": ""OpenGamingLibrary.Json.Test.TestObjects.JsonPropertyWithHandlingValues"",
  ""required"": true,
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""DefaultValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIncludeProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingPopulateProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIgnoreAndPopulateProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""NullValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""NullValueHandlingIncludeProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""ReferenceLoopHandlingErrorProperty"": {
      ""$ref"": ""OpenGamingLibrary.Json.Test.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingIgnoreProperty"": {
      ""$ref"": ""OpenGamingLibrary.Json.Test.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingSerializeProperty"": {
      ""$ref"": ""OpenGamingLibrary.Json.Test.TestObjects.JsonPropertyWithHandlingValues""
    }
  }
}", json);
        }

        [Fact]
        public void GenerateForNullableInt32()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();

            JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(NullableInt32TestClass));
            string json = jsonSchema.ToString();

            StringAssert.Equal(@"{
  ""type"": ""object"",
  ""properties"": {
    ""Value"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    }
  }
}", json);
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SortTypeFlagAsString
        {
            No = 0,
            Asc = 1,
            Desc = -1
        }

        public class Y
        {
            public SortTypeFlagAsString y;
        }

		[Fact(Skip="")]
        public void GenerateSchemaWithStringEnum()
        {
            JsonSchemaGenerator jsonSchemaGenerator = new JsonSchemaGenerator();
            JsonSchema schema = jsonSchemaGenerator.Generate(typeof(Y));

            string json = schema.ToString();

            // NOTE: This fails because the enum is serialized as an integer and not a string.
            // NOTE: There should exist a way to serialize the enum as lowercase strings.
            Assert.Equal(@"{
  ""type"": ""object"",
  ""properties"": {
    ""y"": {
      ""required"": true,
      ""type"": ""string"",
      ""enum"": [
        ""no"",
        ""asc"",
        ""desc""
      ]
    }
  }
}", json);
        }
    }

    public class NullableInt32TestClass
    {
        public int? Value { get; set; }
    }

    public class DMDSLBase
    {
        public String Comment;
    }

    public class Workspace : DMDSLBase
    {
        public ControlFlowItemCollection Jobs = new ControlFlowItemCollection();
    }

    public class ControlFlowItemBase : DMDSLBase
    {
        public String Name;
    }

    public class ControlFlowItem : ControlFlowItemBase //A Job
    {
        public TaskCollection Tasks = new TaskCollection();
        public ContainerCollection Containers = new ContainerCollection();
    }

    public class ControlFlowItemCollection : List<ControlFlowItem>
    {
    }

    public class Task : ControlFlowItemBase
    {
        public DataFlowTaskCollection DataFlowTasks = new DataFlowTaskCollection();
        public BulkInsertTaskCollection BulkInsertTask = new BulkInsertTaskCollection();
    }

    public class TaskCollection : List<Task>
    {
    }

    public class Container : ControlFlowItemBase
    {
        public ControlFlowItemCollection ContainerJobs = new ControlFlowItemCollection();
    }

    public class ContainerCollection : List<Container>
    {
    }

    public class DataFlowTask_DSL : ControlFlowItemBase
    {
    }

    public class DataFlowTaskCollection : List<DataFlowTask_DSL>
    {
    }

    public class SequenceContainer_DSL : Container
    {
    }

    public class BulkInsertTaskCollection : List<BulkInsertTask_DSL>
    {
    }

    public class BulkInsertTask_DSL
    {
    }
}