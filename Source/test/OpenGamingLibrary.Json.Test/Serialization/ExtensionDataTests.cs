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
using System.Text;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Test.TestObjects;
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

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class ExtensionDataTests : TestFixtureBase
    {
        public class CustomDictionary : IDictionary<string, object>
        {
            private readonly IDictionary<string, object> _inner = new Dictionary<string, object>();
 
            public void Add(string key, object value)
            {
                _inner.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                return _inner.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get { return _inner.Keys; }
            }

            public bool Remove(string key)
            {
                return _inner.Remove(key);
            }

            public bool TryGetValue(string key, out object value)
            {
                return _inner.TryGetValue(key, out value);
            }

            public ICollection<object> Values
            {
                get { return _inner.Values; }
            }

            public object this[string key]
            {
                get { return _inner[key]; }
                set { _inner[key] = value; }
            }

            public void Add(KeyValuePair<string, object> item)
            {
                _inner.Add(item);
            }

            public void Clear()
            {
                _inner.Clear();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                return _inner.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                _inner.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _inner.Count; }
            }

            public bool IsReadOnly
            {
                get { return _inner.IsReadOnly; }
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                return _inner.Remove(item);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return _inner.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _inner.GetEnumerator();
            }
        }

        public class Example
        {
            public Example()
            {
                Data = new CustomDictionary();
            }

            [JsonExtensionData]
            public IDictionary<string, object> Data { get; private set; }
        }

        [Fact]
        public void DataBagDoesNotInheritFromDictionaryClass()
        {
            Example e = new Example();
            e.Data.Add("extensionData1", new int[] { 1, 2, 3 });

            string json = JsonConvert.SerializeObject(e, Formatting.Indented);

            Console.WriteLine(json);

            Example e2 = JsonConvert.DeserializeObject<Example>(json);

            JArray o1 = (JArray)e2.Data["extensionData1"];

            Assert.Equal(JTokenType.Array, o1.Type);
            Assert.Equal(3, o1.Count);
            Assert.Equal(1, (int)o1[0]);
            Assert.Equal(2, (int)o1[1]);
            Assert.Equal(3, (int)o1[2]);
        }

        public class ExtensionDataDeserializeWithNonDefaultConstructor
        {
            public ExtensionDataDeserializeWithNonDefaultConstructor(string name)
            {
                Name = name;
            }

            [JsonExtensionData]
            public IDictionary<string, JToken> _extensionData;

            public string Name { get; set; }
        }

        [Fact]
        public void ExtensionDataDeserializeWithNonDefaultConstructorTest()
        {
            ExtensionDataDeserializeWithNonDefaultConstructor c = new ExtensionDataDeserializeWithNonDefaultConstructor("Name!");
            c._extensionData = new Dictionary<string, JToken>
            {
                { "Key!", "Value!" }
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Name"": ""Name!"",
  ""Key!"": ""Value!""
}", json);

            var c2 = JsonConvert.DeserializeObject<ExtensionDataDeserializeWithNonDefaultConstructor>(json);

            Assert.Equal("Name!", c2.Name);
            Assert.NotNull(c2._extensionData);
            Assert.Equal(1, c2._extensionData.Count);
            Assert.Equal("Value!", (string)c2._extensionData["Key!"]);
        }

        [Fact]
        public void ExtensionDataWithNull()
        {
            string json = @"{
              'TaxRate': 0.125,
              'a':null
            }";

            var invoice = JsonConvert.DeserializeObject<ExtendedObject>(json);

            Assert.Equal(JTokenType.Null, invoice._additionalData["a"].Type);
            Assert.Equal(typeof(double), ((JValue)invoice._additionalData["TaxRate"]).Value.GetType());

            string result = JsonConvert.SerializeObject(invoice);

            Assert.Equal(@"{""TaxRate"":0.125,""a"":null}", result);
        }

        [Fact]
        public void ExtensionDataFloatParseHandling()
        {
            string json = @"{
              'TaxRate': 0.125,
              'a':null
            }";

            var invoice = JsonConvert.DeserializeObject<ExtendedObject>(json, new JsonSerializerSettings
            {
                FloatParseHandling = FloatParseHandling.Decimal
            });

            Assert.Equal(typeof(decimal), ((JValue)invoice._additionalData["TaxRate"]).Value.GetType());
        }

#pragma warning disable 649
        class ExtendedObject
        {
            [JsonExtensionData]
            internal IDictionary<string, JToken> _additionalData;
        }
#pragma warning restore 649

#pragma warning disable 169
        public class CustomerInvoice
        {
            // we're only modifing the tax rate
            public decimal TaxRate { get; set; }

            // everything else gets stored here
            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData;
        }
#pragma warning restore 169

        [Fact]
        public void ExtensionDataExample()
        {
            string json = @"{
              'HourlyRate': 150,
              'Hours': 40,
              'TaxRate': 0.125
            }";

            var invoice = JsonConvert.DeserializeObject<CustomerInvoice>(json);

            // increase tax to 15%
            invoice.TaxRate = 0.15m;

            string result = JsonConvert.SerializeObject(invoice);
            // {
            //   'TaxRate': 0.15,
            //   'HourlyRate': 150,
            //   'Hours': 40
            // }

            Assert.Equal(@"{""TaxRate"":0.15,""HourlyRate"":150,""Hours"":40}", result);
        }

        public class ExtensionDataTestClass
        {
            public string Name { get; set; }

            [JsonProperty("custom_name")]
            public string CustomName { get; set; }

            [JsonIgnore]
            public IList<int> Ignored { get; set; }

            public bool GetPrivate { get; internal set; }

            public bool GetOnly
            {
                get { return true; }
            }

            public readonly string Readonly = "Readonly";
            public IList<int> Ints { get; set; }

            [JsonExtensionData]
            internal IDictionary<string, JToken> ExtensionData { get; set; }

            public ExtensionDataTestClass()
            {
                Ints = new List<int> { 0 };
            }
        }

        public class JObjectExtensionDataTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData]
            public JObject ExtensionData { get; set; }
        }

        [Fact]
        public void RoundTripJObjectExtensionData()
        {
            JObjectExtensionDataTestClass c = new JObjectExtensionDataTestClass();
            c.Name = "Name!";
            c.ExtensionData = new JObject
            {
                { "one", 1 },
                { "two", "II" },
                { "three", new JArray(1, 1, 1) }
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            JObjectExtensionDataTestClass c2 = JsonConvert.DeserializeObject<JObjectExtensionDataTestClass>(json);

            Assert.Equal("Name!", c2.Name);
            Assert.True(JToken.DeepEquals(c.ExtensionData, c2.ExtensionData));
        }

        [Fact]
        public void ExtensionDataTest()
        {
            string json = @"{
  ""Ints"": [1,2,3],
  ""Ignored"": [1,2,3],
  ""Readonly"": ""Readonly"",
  ""Name"": ""Actually set!"",
  ""CustomName"": ""Wrong name!"",
  ""GetPrivate"": true,
  ""GetOnly"": true,
  ""NewValueSimple"": true,
  ""NewValueComplex"": [1,2,3]
}";

            ExtensionDataTestClass c = JsonConvert.DeserializeObject<ExtensionDataTestClass>(json);

            Assert.Equal("Actually set!", c.Name);
            Assert.Equal(4, c.Ints.Count);

            Assert.Equal("Readonly", (string)c.ExtensionData["Readonly"]);
            Assert.Equal("Wrong name!", (string)c.ExtensionData["CustomName"]);
            Assert.Equal(true, (bool)c.ExtensionData["GetPrivate"]);
            Assert.Equal(true, (bool)c.ExtensionData["GetOnly"]);
            Assert.Equal(true, (bool)c.ExtensionData["NewValueSimple"]);
            Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["NewValueComplex"]));
            Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["Ignored"]));

            Assert.Equal(7, c.ExtensionData.Count);
        }

        public class MultipleExtensionDataAttributesTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData]
            internal IDictionary<string, JToken> ExtensionData1 { get; set; }

            [JsonExtensionData]
            internal IDictionary<string, JToken> ExtensionData2 { get; set; }
        }

        public class ExtensionDataAttributesInheritanceTestClass : MultipleExtensionDataAttributesTestClass
        {
            [JsonExtensionData]
            internal IDictionary<string, JToken> ExtensionData0 { get; set; }
        }

        public class FieldExtensionDataAttributeTestClass
        {
            [JsonExtensionData]
            internal IDictionary<object, object> ExtensionData;
        }

        public class PublicExtensionDataAttributeTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData]
            public IDictionary<object, object> ExtensionData;
        }

        public class PublicExtensionDataAttributeTestClassWithNonDefaultConstructor
        {
            public string Name { get; set; }

            public PublicExtensionDataAttributeTestClassWithNonDefaultConstructor(string name)
            {
                Name = name;
            }

            [JsonExtensionData]
            public IDictionary<object, object> ExtensionData;
        }

        public class PublicNoReadExtensionDataAttributeTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData(ReadData = false)]
            public IDictionary<object, object> ExtensionData;
        }

        public class PublicNoWriteExtensionDataAttributeTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData(WriteData = false)]
            public IDictionary<object, object> ExtensionData;
        }

        public class PublicJTokenExtensionDataAttributeTestClass
        {
            public string Name { get; set; }

            [JsonExtensionData]
            public IDictionary<string, JToken> ExtensionData;
        }

        [Fact]
        public void DeserializeDirectoryAccount()
        {
            string json = @"{'DisplayName':'John Smith', 'SAMAccountName':'contoso\\johns'}";

            DirectoryAccount account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

            Assert.Equal("John Smith", account.DisplayName);
            Assert.Equal("contoso", account.Domain);
            Assert.Equal("johns", account.UserName);
        }

        [Fact]
        public void SerializePublicExtensionData()
        {
            string json = JsonConvert.SerializeObject(new PublicExtensionDataAttributeTestClass
            {
                Name = "Name!",
                ExtensionData = new Dictionary<object, object>
                {
                    { "Test", 1 }
                }
            });

            Assert.Equal(@"{""Name"":""Name!"",""Test"":1}", json);
        }

        [Fact]
        public void SerializePublicExtensionDataNull()
        {
            string json = JsonConvert.SerializeObject(new PublicExtensionDataAttributeTestClass
            {
                Name = "Name!"
            });

            Assert.Equal(@"{""Name"":""Name!""}", json);
        }

        [Fact]
        public void SerializePublicNoWriteExtensionData()
        {
            string json = JsonConvert.SerializeObject(new PublicNoWriteExtensionDataAttributeTestClass
            {
                Name = "Name!",
                ExtensionData = new Dictionary<object, object>
                {
                    { "Test", 1 }
                }
            });

            Assert.Equal(@"{""Name"":""Name!""}", json);
        }

        [Fact]
        public void DeserializeNoReadPublicExtensionData()
        {
            PublicNoReadExtensionDataAttributeTestClass c = JsonConvert.DeserializeObject<PublicNoReadExtensionDataAttributeTestClass>(@"{""Name"":""Name!"",""Test"":1}");

            Assert.Equal(null, c.ExtensionData);
        }

        [Fact]
        public void SerializePublicExtensionDataCircularReference()
        {
            var c = new PublicExtensionDataAttributeTestClass
            {
                Name = "Name!",
                ExtensionData = new Dictionary<object, object>
                {
                    { "Test", 1 }
                }
            };
            c.ExtensionData["Self"] = c;

            string json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });

            StringAssert.Equal(@"{
  ""$id"": ""1"",
  ""Name"": ""Name!"",
  ""Test"": 1,
  ""Self"": {
    ""$ref"": ""1""
  }
}", json);

            var c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            Assert.Equal("Name!", c2.Name);

            PublicExtensionDataAttributeTestClass bizzaroC2 = (PublicExtensionDataAttributeTestClass)c2.ExtensionData["Self"];

            Assert.Equal(c2, bizzaroC2);
            Assert.Equal(1, (long)bizzaroC2.ExtensionData["Test"]);
        }

        [Fact]
        public void DeserializePublicJTokenExtensionDataCircularReference()
        {
            string json = @"{
  ""$id"": ""1"",
  ""Name"": ""Name!"",
  ""Test"": 1,
  ""Self"": {
    ""$ref"": ""1""
  }
}";

            var c2 = JsonConvert.DeserializeObject<PublicJTokenExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            Assert.Equal("Name!", c2.Name);

            JObject bizzaroC2 = (JObject)c2.ExtensionData["Self"];

            Assert.Equal("Name!", (string)bizzaroC2["Name"]);
            Assert.Equal(1, (int)bizzaroC2["Test"]);

            Assert.Equal(1, (int)c2.ExtensionData["Test"]);
        }

        [Fact]
        public void DeserializePublicExtensionDataTypeNamdHandling()
        {
            string json = @"{
  ""$id"": ""1"",
  ""Name"": ""Name!"",
  ""Test"": 1,
  ""Self"": {
    ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.WagePerson, OpenGamingLibrary.Json.Test"",
    ""HourlyWage"": 2.0,
    ""Name"": null,
    ""BirthDate"": ""0001-01-01T00:00:00"",
    ""LastModified"": ""0001-01-01T00:00:00""
  }
}";

            PublicExtensionDataAttributeTestClass c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Assert.Equal("Name!", c2.Name);

            WagePerson bizzaroC2 = (WagePerson)c2.ExtensionData["Self"];

            Assert.Equal(2m, bizzaroC2.HourlyWage);
        }

        [Fact]
        public void DeserializePublicExtensionDataTypeNamdHandlingNonDefaultConstructor()
        {
            string json = @"{
  ""$id"": ""1"",
  ""Name"": ""Name!"",
  ""Test"": 1,
  ""Self"": {
    ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.WagePerson, OpenGamingLibrary.Json.Test"",
    ""HourlyWage"": 2.0,
    ""Name"": null,
    ""BirthDate"": ""0001-01-01T00:00:00"",
    ""LastModified"": ""0001-01-01T00:00:00""
  }
}";

            PublicExtensionDataAttributeTestClassWithNonDefaultConstructor c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClassWithNonDefaultConstructor>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Assert.Equal("Name!", c2.Name);

            WagePerson bizzaroC2 = (WagePerson)c2.ExtensionData["Self"];

            Assert.Equal(2m, bizzaroC2.HourlyWage);
        }

        [Fact]
        public void SerializePublicExtensionDataTypeNamdHandling()
        {
            PublicExtensionDataAttributeTestClass c = new PublicExtensionDataAttributeTestClass
            {
                Name = "Name!",
                ExtensionData = new Dictionary<object, object>
                {
                    {
                        "Test", new WagePerson
                        {
                            HourlyWage = 2.1m
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            });

            StringAssert.Equal(@"{
  ""$type"": ""OpenGamingLibrary.Json.Test.Serialization.ExtensionDataTests+PublicExtensionDataAttributeTestClass, OpenGamingLibrary.Json.Test"",
  ""Name"": ""Name!"",
  ""Test"": {
    ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.WagePerson, OpenGamingLibrary.Json.Test"",
    ""HourlyWage"": 2.1,
    ""Name"": null,
    ""BirthDate"": ""0001-01-01T00:00:00"",
    ""LastModified"": ""0001-01-01T00:00:00""
  }
}", json);
        }

        [Fact]
        public void DeserializePublicExtensionData()
        {
            string json = @"{
  'Name':'Name!',
  'NoMatch':'NoMatch!',
  'ExtensionData':{'HAI':true}
}";

            var c = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json);

            Assert.Equal("Name!", c.Name);
            Assert.Equal(2, c.ExtensionData.Count);

            Assert.Equal("NoMatch!", (string)c.ExtensionData["NoMatch"]);

            // the ExtensionData property is put into the extension data
            // inception
            var o = (JObject)c.ExtensionData["ExtensionData"];
            Assert.Equal(1, o.Count);
            Assert.True(JToken.DeepEquals(new JObject { { "HAI", true } }, o));
        }

        [Fact]
        public void FieldExtensionDataAttributeTest_Serialize()
        {
            FieldExtensionDataAttributeTestClass c = new FieldExtensionDataAttributeTestClass
            {
                ExtensionData = new Dictionary<object, object>()
            };

            string json = JsonConvert.SerializeObject(c);

            Assert.Equal("{}", json);
        }

        [Fact]
        public void FieldExtensionDataAttributeTest_Deserialize()
        {
            var c = JsonConvert.DeserializeObject<FieldExtensionDataAttributeTestClass>("{'first':1,'second':2}");

            Assert.Equal(2, c.ExtensionData.Count);
            Assert.Equal(1, (long)c.ExtensionData["first"]);
            Assert.Equal(2, (long)c.ExtensionData["second"]);
        }

        [Fact]
        public void MultipleExtensionDataAttributesTest()
        {
            var c = JsonConvert.DeserializeObject<MultipleExtensionDataAttributesTestClass>("{'first':[1],'second':[2]}");

            Assert.Equal(null, c.ExtensionData1);
            Assert.Equal(2, c.ExtensionData2.Count);
            Assert.Equal(1, (int)((JArray)c.ExtensionData2["first"])[0]);
            Assert.Equal(2, (int)((JArray)c.ExtensionData2["second"])[0]);
        }

        [Fact]
        public void ExtensionDataAttributesInheritanceTest()
        {
            var c = JsonConvert.DeserializeObject<ExtensionDataAttributesInheritanceTestClass>("{'first':1,'second':2}");

            Assert.Equal(null, c.ExtensionData1);
            Assert.Equal(null, c.ExtensionData2);
            Assert.Equal(2, c.ExtensionData0.Count);
            Assert.Equal(1, (int)c.ExtensionData0["first"]);
            Assert.Equal(2, (int)c.ExtensionData0["second"]);
        }
    }
}