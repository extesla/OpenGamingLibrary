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
using System.Runtime.Serialization;
using Xunit;
using System.Linq;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using System.Reflection;
using OpenGamingLibrary.Json.Utilities;
using System.Globalization;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly char _startingWithChar;

        public DynamicContractResolver(char startingWithChar)
            : base(false)
        {
            _startingWithChar = startingWithChar;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that start with the specified character
            properties =
                properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

            return properties;
        }
    }

    public class EscapedPropertiesContractResolver : DefaultContractResolver
    {
        public string PropertyPrefix { get; set; }
        public string PropertySuffix { get; set; }

        protected internal override string ResolvePropertyName(string propertyName)
        {
            return base.ResolvePropertyName(PropertyPrefix + propertyName + PropertySuffix);
        }
    }

    public class Book
    {
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string AuthorName { get; set; }
        public int AuthorAge { get; set; }
        public string AuthorCountry { get; set; }
    }

    public class IPersonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(Employee))
                objectType = typeof(IPerson);

            return base.CreateContract(objectType);
        }
    }

    public class AddressWithDataMember
    {
#if !NET20
        [DataMember(Name = "CustomerAddress1")]
#endif
            public string AddressLine1 { get; set; }
    }

    
    public class ContractResolverTests : TestFixtureBase
    {
        [Fact]
        public void JsonPropertyDefaultValue()
        {
            JsonProperty p = new JsonProperty();

            Assert.Equal(null, p.GetResolvedDefaultValue());
            Assert.Equal(null, p.DefaultValue);

            p.PropertyType = typeof(int);

            Assert.Equal(0 , p.GetResolvedDefaultValue());
            Assert.Equal(null, p.DefaultValue);

            p.PropertyType = typeof(DateTime);

            Assert.Equal(new DateTime(), p.GetResolvedDefaultValue());
            Assert.Equal(null, p.DefaultValue);

            p.PropertyType = null;

            Assert.Equal(null, p.GetResolvedDefaultValue());
            Assert.Equal(null, p.DefaultValue);

            p.PropertyType = typeof(CompareOptions);

            Assert.Equal(CompareOptions.None, (CompareOptions)p.GetResolvedDefaultValue());
            Assert.Equal(null, p.DefaultValue);
        }

        [Fact]
        public void ListInterface()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(IList<int>));

            Assert.True(contract.IsInstantiable);
            Assert.Equal(typeof(List<int>), contract.CreatedType);
            Assert.NotNull(contract.DefaultCreator);
        }

        [Fact]
        public void AbstractTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(AbstractTestClass));

            Assert.False(contract.IsInstantiable);
            Assert.Null(contract.DefaultCreator);
            Assert.Null(contract.OverrideCreator);

			AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractTestClass>(@"{Value:'Value!'}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type OpenGamingLibrary.Json.Test.Serialization.AbstractTestClass. Type is an interface or abstract class and cannot be instantiated. Path 'Value', line 1, position 7.");

            contract.DefaultCreator = () => new AbstractImplementationTestClass();

            var o = JsonConvert.DeserializeObject<AbstractTestClass>(@"{Value:'Value!'}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal("Value!", o.Value);
        }

        [Fact]
        public void AbstractListTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(AbstractListTestClass<int>));

            Assert.False(contract.IsInstantiable);
            Assert.Null(contract.DefaultCreator);
            Assert.False(contract.HasParametrizedCreator);

            AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type OpenGamingLibrary.Json.Test.Serialization.AbstractListTestClass`1[System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path '', line 1, position 1.");

            contract.DefaultCreator = () => new AbstractImplementationListTestClass<int>();

            var l = JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal(2, l.Count);
            Assert.Equal(1, l[0]);
            Assert.Equal(2, l[1]);
        }

        public class CustomList<T> : List<T>
        {   
        }

        [Fact]
        public void ListInterfaceDefaultCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(IList<int>));

            Assert.True(contract.IsInstantiable);
            Assert.NotNull(contract.DefaultCreator);

            contract.DefaultCreator = () => new CustomList<int>();

            var l = JsonConvert.DeserializeObject<IList<int>>(@"[1,2,3]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal(typeof(CustomList<int>), l.GetType());
            Assert.Equal(3, l.Count);
            Assert.Equal(1, l[0]);
            Assert.Equal(2, l[1]);
            Assert.Equal(3, l[2]);
        }

        public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
        }

        [Fact]
        public void DictionaryInterfaceDefaultCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonDictionaryContract)resolver.ResolveContract(typeof(IDictionary<string, int>));

            Assert.True(contract.IsInstantiable);
            Assert.NotNull(contract.DefaultCreator);

            contract.DefaultCreator = () => new CustomDictionary<string, int>();

            var d = JsonConvert.DeserializeObject<IDictionary<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal(typeof(CustomDictionary<string, int>), d.GetType());
            Assert.Equal(2, d.Count);
            Assert.Equal(1, d["key1"]);
            Assert.Equal(2, d["key2"]);
        }

        [Fact]
        public void AbstractDictionaryTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonDictionaryContract)resolver.ResolveContract(typeof(AbstractDictionaryTestClass<string, int>));

            Assert.False(contract.IsInstantiable);
            Assert.Null(contract.DefaultCreator);
            Assert.False(contract.HasParametrizedCreator);

			AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type OpenGamingLibrary.Json.Test.Serialization.AbstractDictionaryTestClass`2[System.String,System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path 'key1', line 1, position 6.");

            contract.DefaultCreator = () => new AbstractImplementationDictionaryTestClass<string, int>();

            var d = JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal(2, d.Count);
            Assert.Equal(1, d["key1"]);
            Assert.Equal(2, d["key2"]);
        }

        [Fact]
        public void SerializeWithEscapedPropertyName()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertySuffix = @"-'-""-"
                    }
                });

            Assert.Equal(@"{""AddressLine1-'-\""-"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            reader.Read();

            Assert.Equal(@"AddressLine1-'-""-", reader.Value);
        }

        [Fact]
        public void SerializeWithHtmlEscapedPropertyName()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertyPrefix = "<b>",
                        PropertySuffix = "</b>"
                    },
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                });

            Assert.Equal(@"{""\u003cb\u003eAddressLine1\u003c/b\u003e"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            reader.Read();

            Assert.Equal(@"<b>AddressLine1</b>", reader.Value);
        }

        [Fact]
        public void CalculatingPropertyNameEscapedSkipping()
        {
            JsonProperty p = new JsonProperty { PropertyName = "abc" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "123" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "._-" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "!@#" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "$%^" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "?*(" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = ")_+" };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "=:," };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = null };
            Assert.True(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "&" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "<" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = ">" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "'" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = @"""" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = Environment.NewLine };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\0" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\n" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\v" };
            Assert.False(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\u00B9" };
            Assert.False(p._skipPropertyNameEscape);
        }

#if !NET20
        [Fact]
        public void DeserializeDataMemberClassWithNoDataContract()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(AddressWithDataMember));

            Assert.Equal("AddressLine1", contract.Properties[0].PropertyName);
        }
#endif

        [Fact]
        public void ResolveProperties_IgnoreStatic()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(NumberFormatInfo));

            Assert.False(contract.Properties.Any(c => c.PropertyName == "InvariantInfo"));
        }

        [Fact]
        public void ParametrizedCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(PublicParametizedConstructorWithPropertyNameConflictWithAttribute));

            Assert.Null(contract.DefaultCreator);
            Assert.NotNull(contract.ParametrizedCreator);

			ConstructorInfo info = typeof(PublicParametizedConstructorWithPropertyNameConflictWithAttribute).GetConstructor(new[] { typeof(string) });
			// TODO: Assert.Equal(info, contract.ParametrizedCreator);
            Assert.Equal(1, contract.CreatorParameters.Count);
            Assert.Equal("name", contract.CreatorParameters[0].PropertyName);
			contract.ParametrizedCreator = null;
            Assert.Null(contract.ParametrizedCreator);
        }

        [Fact]
        public void OverrideCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParamatrizedConstructorsJsonConstructor));

            Assert.Null(contract.DefaultCreator);
            Assert.NotNull(contract.OverrideCreator);

			ConstructorInfo info = typeof(MultipleParamatrizedConstructorsJsonConstructor).GetConstructor(new[] {
				typeof(string),
				typeof(int)
			});
			// TODO: Assert.Equal(info, contract.OverrideCreator);
            Assert.Equal(2, contract.CreatorParameters.Count);
            Assert.Equal("Value", contract.CreatorParameters[0].PropertyName);
            Assert.Equal("Age", contract.CreatorParameters[1].PropertyName);

            contract.OverrideCreator = null;
            Assert.Null(contract.OverrideCreator);
        }

        [Fact]
        public void CustomOverrideCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParamatrizedConstructorsJsonConstructor));

            bool ensureCustomCreatorCalled = false;

            contract.OverrideCreator = args =>
            {
                ensureCustomCreatorCalled = true;
                return new MultipleParamatrizedConstructorsJsonConstructor((string) args[0], (int) args[1]);
            };

			Assert.Null(contract.OverrideCreator);

            var o = JsonConvert.DeserializeObject<MultipleParamatrizedConstructorsJsonConstructor>("{Value:'value!', Age:1}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.Equal("value!", o.Value);
            Assert.Equal(1, o.Age);
            Assert.True(ensureCustomCreatorCalled);
        }

        [Fact]
        public void SerializeInterface()
        {
            var employee = new Employee
            {
                BirthDate = new DateTime(1977, 12, 30, 1, 1, 1, DateTimeKind.Utc),
                FirstName = "Maurice",
                LastName = "Moss",
                Department = "IT",
                JobTitle = "Support"
            };

            string iPersonJson = JsonConvert.SerializeObject(employee, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new IPersonContractResolver() });

            StringAssert.Equal(@"{
  ""FirstName"": ""Maurice"",
  ""LastName"": ""Moss"",
  ""BirthDate"": ""1977-12-30T01:01:01Z""
}", iPersonJson);
        }

        [Fact]
        public void SingleTypeWithMultipleContractResolvers()
        {
            Book book = new Book
            {
                BookName = "The Gathering Storm",
                BookPrice = 16.19m,
                AuthorName = "Brandon Sanderson",
                AuthorAge = 34,
                AuthorCountry = "United States of America"
            };

            string startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

            // {
            //   "AuthorName": "Brandon Sanderson",
            //   "AuthorAge": 34,
            //   "AuthorCountry": "United States of America"
            // }

            string startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

            // {
            //   "BookName": "The Gathering Storm",
            //   "BookPrice": 16.19
            // }

            StringAssert.Equal(@"{
  ""AuthorName"": ""Brandon Sanderson"",
  ""AuthorAge"": 34,
  ""AuthorCountry"": ""United States of America""
}", startingWithA);

            StringAssert.Equal(@"{
  ""BookName"": ""The Gathering Storm"",
  ""BookPrice"": 16.19
}", startingWithB);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
#pragma warning disable 618
        [Fact]
        public void SerializeCompilerGeneratedMembers()
        {
            var structTest = new StructTest
            {
                IntField = 1,
                IntProperty = 2,
                StringField = "Field",
                StringProperty = "Property"
            };

            DefaultContractResolver skipCompilerGeneratedResolver = new DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            };

            string skipCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = skipCompilerGeneratedResolver });

            StringAssert.Equal(@"{
  ""StringField"": ""Field"",
  ""IntField"": 1,
  ""StringProperty"": ""Property"",
  ""IntProperty"": 2
}", skipCompilerGeneratedJson);

            DefaultContractResolver includeCompilerGeneratedResolver = new DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                SerializeCompilerGeneratedMembers = true
            };

            string includeCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = includeCompilerGeneratedResolver });

            StringAssert.Equal(@"{
  ""StringField"": ""Field"",
  ""IntField"": 1,
  ""<StringProperty>k__BackingField"": ""Property"",
  ""<IntProperty>k__BackingField"": 2,
  ""StringProperty"": ""Property"",
  ""IntProperty"": 2
}", includeCompilerGeneratedJson);
        }
#pragma warning restore 618
#endif
    }
}