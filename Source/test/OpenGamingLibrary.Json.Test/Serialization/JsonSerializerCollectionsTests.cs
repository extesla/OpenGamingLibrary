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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Test.TestObjects;
using Xunit;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class JsonSerializerCollectionsTests : TestFixtureBase
    {
        [Fact]
        public void MultiDObjectArray()
        {
            object[,] myOtherArray =
            {
                { new KeyValuePair<string, double>("my value", 0.8), "foobar" },
                { true, 0.4d },
                { 0.05f, 6 }
            };

            string myOtherArrayAsString = JsonConvert.SerializeObject(myOtherArray, Formatting.Indented);

            Assert.Equal(@"[
  [
    {
      ""Key"": ""my value"",
      ""Value"": 0.8
    },
    ""foobar""
  ],
  [
    true,
    0.4
  ],
  [
    0.05,
    6
  ]
]", myOtherArrayAsString);

            JObject o = JObject.Parse(@"{
              ""Key"": ""my value"",
              ""Value"": 0.8
            }");

            object[,] myOtherResult = JsonConvert.DeserializeObject<object[,]>(myOtherArrayAsString);
            Assert.True(JToken.DeepEquals(o, (JToken)myOtherResult[0, 0]));
            Assert.Equal("foobar", myOtherResult[0, 1]);

            Assert.Equal(true, myOtherResult[1, 0]);
            Assert.Equal(0.4, myOtherResult[1, 1]);

            Assert.Equal(0.05, myOtherResult[2, 0]);
            Assert.Equal(6, myOtherResult[2, 1]);
        }

        public class EnumerableClass<T> : IEnumerable<T>
        {
            private readonly IList<T> _values;

            public EnumerableClass(IEnumerable<T> values)
            {
                _values = new List<T>(values);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Fact]
        public void DeserializeIEnumerableFromConstructor()
        {
            string json = @"[
  1,
  2,
  null
]";

            var result = JsonConvert.DeserializeObject<EnumerableClass<int?>>(json);

            Assert.Equal(3, result.Count());
            Assert.Equal(1, result.ElementAt(0));
            Assert.Equal(2, result.ElementAt(1));
            Assert.Equal(null, result.ElementAt(2));
        }

        public class EnumerableClassFailure<T> : IEnumerable<T>
        {
            private readonly IList<T> _values;

            public EnumerableClassFailure()
            {
                _values = new List<T>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Fact]
        public void DeserializeIEnumerableFromConstructor_Failure()
        {
            string json = @"[
  ""One"",
  ""II"",
  ""3""
]";

            AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<EnumerableClassFailure<string>>(json), "Cannot create and populate list type OpenGamingLibrary.Json.Test.Serialization.JsonSerializerCollectionsTests+EnumerableClassFailure`1[System.String]. Path '', line 1, position 1.");
        }

        public class PrivateDefaultCtorList<T> : List<T>
        {
            private PrivateDefaultCtorList()
            {
            }
        }

        [Fact]
        public void DeserializePrivateListCtor()
        {
            AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]"), "Unable to find a constructor to use for type OpenGamingLibrary.Json.Test.Serialization.JsonSerializerCollectionsTests+PrivateDefaultCtorList`1[System.Int32]. Path '', line 1, position 1.");

            var list = JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]", new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });

            Assert.Equal(2, list.Count);
        }

        public class PrivateDefaultCtorWithIEnumerableCtorList<T> : List<T>
        {
            private PrivateDefaultCtorWithIEnumerableCtorList()
            {
            }

            public PrivateDefaultCtorWithIEnumerableCtorList(IEnumerable<T> values)
                : base(values)
            {
                Add(default(T));
            }
        }

        [Fact]
        public void DeserializePrivateListConstructor()
        {
            var list = JsonConvert.DeserializeObject<PrivateDefaultCtorWithIEnumerableCtorList<int>>("[1,2]");

            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(0, list[2]);
        }

        [Fact]
        public void DeserializeNonIsoDateDictionaryKey()
        {
            Dictionary<DateTime, string> d = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(@"{""04/28/2013 00:00:00"":""test""}");

            Assert.Equal(1, d.Count);

            DateTime key = DateTime.Parse("04/28/2013 00:00:00", CultureInfo.InvariantCulture);
            Assert.Equal("test", d[key]);
        }

        [Fact]
        public void DeserializeNonGenericList()
        {
            IList l = JsonConvert.DeserializeObject<IList>("['string!']");

            Assert.Equal(typeof(List<object>), l.GetType());
            Assert.Equal(1, l.Count);
            Assert.Equal("string!", l[0]);
        }

        [Fact]
        public void TestEscapeDictionaryStrings()
        {
            const string s = @"host\user";
            string serialized = JsonConvert.SerializeObject(s);
            Assert.Equal(@"""host\\user""", serialized);

            Dictionary<int, object> d1 = new Dictionary<int, object>();
            d1.Add(5, s);
            Assert.Equal(@"{""5"":""host\\user""}", JsonConvert.SerializeObject(d1));

            Dictionary<string, object> d2 = new Dictionary<string, object>();
            d2.Add(s, 5);
            Assert.Equal(@"{""host\\user"":5}", JsonConvert.SerializeObject(d2));
        }

        public class GenericListTestClass
        {
            public List<string> GenericList { get; set; }

            public GenericListTestClass()
            {
                GenericList = new List<string>();
            }
        }

        [Fact]
        public void DeserializeExistingGenericList()
        {
            GenericListTestClass c = new GenericListTestClass();
            c.GenericList.Add("1");
            c.GenericList.Add("2");

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            GenericListTestClass newValue = JsonConvert.DeserializeObject<GenericListTestClass>(json);
            Assert.Equal(2, newValue.GenericList.Count);
            Assert.Equal(typeof(List<string>), newValue.GenericList.GetType());
        }

        [Fact]
        public void DeserializeSimpleKeyValuePair()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("key1", "value1"));
            list.Add(new KeyValuePair<string, string>("key2", "value2"));

            string json = JsonConvert.SerializeObject(list);

            Assert.Equal(@"[{""Key"":""key1"",""Value"":""value1""},{""Key"":""key2"",""Value"":""value2""}]", json);

            List<KeyValuePair<string, string>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
            Assert.Equal(2, result.Count);
            Assert.Equal("key1", result[0].Key);
            Assert.Equal("value1", result[0].Value);
            Assert.Equal("key2", result[1].Key);
            Assert.Equal("value2", result[1].Value);
        }

        [Fact]
        public void DeserializeComplexKeyValuePair()
        {
            DateTime dateTime = new DateTime(2000, 12, 1, 23, 1, 1, DateTimeKind.Utc);

            List<KeyValuePair<string, WagePerson>> list = new List<KeyValuePair<string, WagePerson>>();
            list.Add(new KeyValuePair<string, WagePerson>("key1", new WagePerson
            {
                BirthDate = dateTime,
                Department = "Department1",
                LastModified = dateTime,
                HourlyWage = 1
            }));
            list.Add(new KeyValuePair<string, WagePerson>("key2", new WagePerson
            {
                BirthDate = dateTime,
                Department = "Department2",
                LastModified = dateTime,
                HourlyWage = 2
            }));

            string json = JsonConvert.SerializeObject(list, Formatting.Indented);

            StringAssert.Equal(@"[
  {
    ""Key"": ""key1"",
    ""Value"": {
      ""HourlyWage"": 1.0,
      ""Name"": null,
      ""BirthDate"": ""2000-12-01T23:01:01Z"",
      ""LastModified"": ""2000-12-01T23:01:01Z""
    }
  },
  {
    ""Key"": ""key2"",
    ""Value"": {
      ""HourlyWage"": 2.0,
      ""Name"": null,
      ""BirthDate"": ""2000-12-01T23:01:01Z"",
      ""LastModified"": ""2000-12-01T23:01:01Z""
    }
  }
]", json);

            List<KeyValuePair<string, WagePerson>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, WagePerson>>>(json);
            Assert.Equal(2, result.Count);
            Assert.Equal("key1", result[0].Key);
            Assert.Equal(1, result[0].Value.HourlyWage);
            Assert.Equal("key2", result[1].Key);
            Assert.Equal(2, result[1].Value.HourlyWage);
        }

        [Fact]
        public void StringListAppenderConverterTest()
        {
            Movie p = new Movie();
            p.ReleaseCountries = new List<string> { "Existing" };

            JsonConvert.PopulateObject("{'ReleaseCountries':['Appended']}", p, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringListAppenderConverter() }
            });

            Assert.Equal(2, p.ReleaseCountries.Count);
            Assert.Equal("Existing", p.ReleaseCountries[0]);
            Assert.Equal("Appended", p.ReleaseCountries[1]);
        }

        [Fact]
        public void StringAppenderConverterTest()
        {
            Movie p = new Movie();
            p.Name = "Existing,";

            JsonConvert.PopulateObject("{'Name':'Appended'}", p, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringAppenderConverter() }
            });

            Assert.Equal("Existing,Appended", p.Name);
        }

        [Fact]
        public void DeserializeIDictionary()
        {
            IDictionary dictionary = JsonConvert.DeserializeObject<IDictionary>("{'name':'value!'}");
            Assert.Equal(1, dictionary.Count);
            Assert.Equal("value!", dictionary["name"]);
        }

        [Fact]
        public void DeserializeIList()
        {
            IList list = JsonConvert.DeserializeObject<IList>("['1', 'two', 'III']");
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void NullableValueGenericDictionary()
        {
            IDictionary<string, int?> v1 = new Dictionary<string, int?>
            {
                { "First", 1 },
                { "Second", null },
                { "Third", 3 }
            };

            string json = JsonConvert.SerializeObject(v1, Formatting.Indented);

            StringAssert.Equal(@"{
  ""First"": 1,
  ""Second"": null,
  ""Third"": 3
}", json);

            IDictionary<string, int?> v2 = JsonConvert.DeserializeObject<IDictionary<string, int?>>(json);
            Assert.Equal(3, v2.Count);
            Assert.Equal(1, v2["First"]);
            Assert.Equal(null, v2["Second"]);
            Assert.Equal(3, v2["Third"]);
        }

        [Fact]
        public void DeserializeKeyValuePairArray()
        {
            string json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

            IList<KeyValuePair<string, IList<string>>> values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json);

            Assert.Equal(2, values.Count);
            Assert.Equal("aaa", values[0].Key);
            Assert.Equal(2, values[0].Value.Count);
            Assert.Equal("1", values[0].Value[0]);
            Assert.Equal("2", values[0].Value[1]);
            Assert.Equal("bbb", values[1].Key);
            Assert.Equal(2, values[1].Value.Count);
            Assert.Equal("3", values[1].Value[0]);
            Assert.Equal("4", values[1].Value[1]);
        }

        [Fact]
        public void DeserializeNullableKeyValuePairArray()
        {
            string json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, null, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

            IList<KeyValuePair<string, IList<string>>?> values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>?>>(json);

            Assert.Equal(3, values.Count);
            Assert.Equal("aaa", values[0].Value.Key);
            Assert.Equal(2, values[0].Value.Value.Count);
            Assert.Equal("1", values[0].Value.Value[0]);
            Assert.Equal("2", values[0].Value.Value[1]);
            Assert.Equal(null, values[1]);
            Assert.Equal("bbb", values[2].Value.Key);
            Assert.Equal(2, values[2].Value.Value.Count);
            Assert.Equal("3", values[2].Value.Value[0]);
            Assert.Equal("4", values[2].Value.Value[1]);
        }

        [Fact]
        public void DeserializeNullToNonNullableKeyValuePairArray()
        {
            string json = @"[ null ]";

			AssertException.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json); }, "Cannot convert null value to KeyValuePair. Path '[0]', line 1, position 6.");
        }

        [Fact]
        public void SerializeArray2D()
        {
            Array2D aa = new Array2D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[,] { { 1, 1 }, { 1, 2 }, { 2, 1 }, { 2, 2 } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}", json);
        }

        [Fact]
        public void SerializeArray3D()
        {
            Array3D aa = new Array3D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[, ,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}", json);
        }

        [Fact]
        public void SerializeArray3DWithConverter()
        {
            Array3DWithConverter aa = new Array3DWithConverter();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[, ,] { { { 1, 1, 1 }, { 1, 1, 2 } }, { { 1, 2, 1 }, { 1, 2, 2 } }, { { 2, 1, 1 }, { 2, 1, 2 } }, { { 2, 2, 1 }, { 2, 2, 2 } } };

            string json = JsonConvert.SerializeObject(aa, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Before"": ""Before!"",
  ""Coordinates"": [
    [
      [
        1.0,
        1.0,
        1.0
      ],
      [
        1.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        1.0,
        2.0,
        1.0
      ],
      [
        1.0,
        2.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        1.0,
        1.0
      ],
      [
        2.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        2.0,
        1.0
      ],
      [
        2.0,
        2.0,
        2.0
      ]
    ]
  ],
  ""After"": ""After!""
}", json);
        }

        [Fact]
        public void DeserializeArray3DWithConverter()
        {
            string json = @"{
  ""Before"": ""Before!"",
  ""Coordinates"": [
    [
      [
        1.0,
        1.0,
        1.0
      ],
      [
        1.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        1.0,
        2.0,
        1.0
      ],
      [
        1.0,
        2.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        1.0,
        1.0
      ],
      [
        2.0,
        1.0,
        2.0
      ]
    ],
    [
      [
        2.0,
        2.0,
        1.0
      ],
      [
        2.0,
        2.0,
        2.0
      ]
    ]
  ],
  ""After"": ""After!""
}";

            Array3DWithConverter aa = JsonConvert.DeserializeObject<Array3DWithConverter>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(4, aa.Coordinates.GetLength(0));
            Assert.Equal(2, aa.Coordinates.GetLength(1));
            Assert.Equal(3, aa.Coordinates.GetLength(2));
            Assert.Equal(1, aa.Coordinates[0, 0, 0]);
            Assert.Equal(2, aa.Coordinates[1, 1, 1]);
        }

        [Fact]
        public void DeserializeArray2D()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}";

            Array2D aa = JsonConvert.DeserializeObject<Array2D>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(4, aa.Coordinates.GetLength(0));
            Assert.Equal(2, aa.Coordinates.GetLength(1));
            Assert.Equal(1, aa.Coordinates[0, 0]);
            Assert.Equal(2, aa.Coordinates[1, 1]);

            string after = JsonConvert.SerializeObject(aa);

            Assert.Equal(json, after);
        }

        [Fact]
        public void DeserializeArray2D_WithTooManyItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2,3],[2,1],[2,2]],""After"":""After!""}";

            AssertException.Throws<Exception>(() => JsonConvert.DeserializeObject<Array2D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Fact]
        public void DeserializeArray2D_WithTooFewItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1],[2,1],[2,2]],""After"":""After!""}";

            AssertException.Throws<Exception>(() => JsonConvert.DeserializeObject<Array2D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Fact]
        public void DeserializeArray3D()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(4, aa.Coordinates.GetLength(0));
            Assert.Equal(2, aa.Coordinates.GetLength(1));
            Assert.Equal(3, aa.Coordinates.GetLength(2));
            Assert.Equal(1, aa.Coordinates[0, 0, 0]);
            Assert.Equal(2, aa.Coordinates[1, 1, 1]);

            string after = JsonConvert.SerializeObject(aa);

            Assert.Equal(json, after);
        }

        [Fact]
        public void DeserializeArray3D_WithTooManyItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            AssertException.Throws<Exception>(() => JsonConvert.DeserializeObject<Array3D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Fact]
        public void DeserializeArray3D_WithBadItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],{}]],""After"":""After!""}";

            AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Array3D>(json), "Unexpected token when deserializing multidimensional array: StartObject. Path 'Coordinates[3][1]', line 1, position 99.");
        }

        [Fact]
        public void DeserializeArray3D_WithTooFewItems()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

            AssertException.Throws<Exception>(() => JsonConvert.DeserializeObject<Array3D>(json), "Cannot deserialize non-cubical array as multidimensional array.");
        }

        [Fact]
        public void SerializeEmpty3DArray()
        {
            Array3D aa = new Array3D();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new int[0, 0, 0];

            string json = JsonConvert.SerializeObject(aa);

            Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}", json);
        }

        [Fact]
        public void DeserializeEmpty3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(0, aa.Coordinates.GetLength(0));
            Assert.Equal(0, aa.Coordinates.GetLength(1));
            Assert.Equal(0, aa.Coordinates.GetLength(2));

            string after = JsonConvert.SerializeObject(aa);

            Assert.Equal(json, after);
        }

        [Fact]
        public void DeserializeIncomplete3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/[1/*hi*/,/*hi*/1/*hi*/,1]/*hi*/,/*hi*/[1,1";

            AssertException.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
        }

        [Fact]
        public void DeserializeIncompleteNotTopLevel3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/";

            AssertException.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
        }

        [Fact]
        public void DeserializeNull3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":null,""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(null, aa.Coordinates);

            string after = JsonConvert.SerializeObject(aa);

            Assert.Equal(json, after);
        }

        [Fact]
        public void DeserializeSemiEmpty3DArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[]],""After"":""After!""}";

            Array3D aa = JsonConvert.DeserializeObject<Array3D>(json);

            Assert.Equal("Before!", aa.Before);
            Assert.Equal("After!", aa.After);
            Assert.Equal(1, aa.Coordinates.GetLength(0));
            Assert.Equal(0, aa.Coordinates.GetLength(1));
            Assert.Equal(0, aa.Coordinates.GetLength(2));

            string after = JsonConvert.SerializeObject(aa);

            Assert.Equal(json, after);
        }

        [Fact]
        public void SerializeReferenceTracked3DArray()
        {
            Event1 e1 = new Event1
            {
                EventName = "EventName!"
            };
            Event1[,] array1 = new[,] { { e1, e1 }, { e1, e1 } };
            IList<Event1[,]> values1 = new List<Event1[,]>
            {
                array1,
                array1
            };

            string json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });

            StringAssert.Equal(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""$id"": ""2"",
      ""$values"": [
        [
          {
            ""$id"": ""3"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$ref"": ""3""
          }
        ],
        [
          {
            ""$ref"": ""3""
          },
          {
            ""$ref"": ""3""
          }
        ]
      ]
    },
    {
      ""$ref"": ""2""
    }
  ]
}", json);
        }

        [Fact]
        public void SerializeTypeName3DArray()
        {
            Event1 e1 = new Event1
            {
                EventName = "EventName!"
            };
            Event1[,] array1 = new[,] { { e1, e1 }, { e1, e1 } };
            IList<Event1[,]> values1 = new List<Event1[,]>
            {
                array1,
                array1
            };

            string json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

            StringAssert.Equal(@"{
  ""$type"": ""System.Collections.Generic.List`1[[OpenGamingLibrary.Json.Test.TestObjects.Event1[,], OpenGamingLibrary.Json.Test]], mscorlib"",
  ""$values"": [
    {
      ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1[,], OpenGamingLibrary.Json.Test"",
      ""$values"": [
        [
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ]
      ]
    },
    {
      ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1[,], OpenGamingLibrary.Json.Test"",
      ""$values"": [
        [
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ],
        [
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          },
          {
            ""$type"": ""OpenGamingLibrary.Json.Test.TestObjects.Event1, OpenGamingLibrary.Json.Test"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }
        ]
      ]
    }
  ]
}", json);

            IList<Event1[,]> values2 = (IList<Event1[,]>)JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Assert.Equal(2, values2.Count);
            Assert.Equal("EventName!", values2[0][0, 0].EventName);
        }

        [Fact]
        public void PrimitiveValuesInObjectArray()
        {
            string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",null],""type"":""rpc"",""tid"":2}";

            ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

            Assert.Equal("Router", o.Action);
            Assert.Equal("Navigate", o.Method);
            Assert.Equal(2, o.Data.Length);
            Assert.Equal("dashboard", o.Data[0]);
            Assert.Equal(null, o.Data[1]);
        }

        [Fact]
        public void ComplexValuesInObjectArray()
        {
            string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",[""id"", 1, ""teststring"", ""test""],{""one"":1}],""type"":""rpc"",""tid"":2}";

            ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

            Assert.Equal("Router", o.Action);
            Assert.Equal("Navigate", o.Method);
            Assert.Equal(3, o.Data.Length);
            Assert.Equal("dashboard", o.Data[0]);
            Assert.IsType(typeof(JArray), o.Data[1]);
            Assert.Equal(4, ((JArray)o.Data[1]).Count);
            Assert.IsType(typeof(JObject), o.Data[2]);
            Assert.Equal(1, ((JObject)o.Data[2]).Count);
            Assert.Equal(1, (int)((JObject)o.Data[2])["one"]);
        }

#if !(NETFX_CORE || ASPNETCORE50)
        [Fact]
        public void SerializeArrayAsArrayList()
        {
            string jsonText = @"[3, ""somestring"",[1,2,3],{}]";
            ArrayList o = JsonConvert.DeserializeObject<ArrayList>(jsonText);

            Assert.Equal(4, o.Count);
            Assert.Equal(3, ((JArray)o[2]).Count);
            Assert.Equal(0, ((JObject)o[3]).Count);
        }
#endif

        [Fact]
        public void SerializeMemberGenericList()
        {
            Name name = new Name("The Idiot in Next To Me");

            PhoneNumber p1 = new PhoneNumber("555-1212");
            PhoneNumber p2 = new PhoneNumber("444-1212");

            name.pNumbers.Add(p1);
            name.pNumbers.Add(p2);

            string json = JsonConvert.SerializeObject(name, Formatting.Indented);

            StringAssert.Equal(@"{
  ""personsName"": ""The Idiot in Next To Me"",
  ""pNumbers"": [
    {
      ""phoneNumber"": ""555-1212""
    },
    {
      ""phoneNumber"": ""444-1212""
    }
  ]
}", json);

            Name newName = JsonConvert.DeserializeObject<Name>(json);

            Assert.Equal("The Idiot in Next To Me", newName.personsName);

            // not passed in as part of the constructor but assigned to pNumbers property
            Assert.Equal(2, newName.pNumbers.Count);
            Assert.Equal("555-1212", newName.pNumbers[0].phoneNumber);
            Assert.Equal("444-1212", newName.pNumbers[1].phoneNumber);
        }

        [Fact]
        public void CustomCollectionSerialization()
        {
            ProductCollection collection = new ProductCollection()
            {
                new Product() { Name = "Test1" },
                new Product() { Name = "Test2" },
                new Product() { Name = "Test3" }
            };

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            StringWriter sw = new StringWriter();

            jsonSerializer.Serialize(sw, collection);

            Assert.Equal(@"[{""Name"":""Test1"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test2"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test3"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null}]",
                sw.GetStringBuilder().ToString());

            ProductCollection collectionNew = (ProductCollection)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(sw.GetStringBuilder().ToString())), typeof(ProductCollection));

            Assert.Equal(collection, collectionNew);
        }

        [Fact]
        public void GenericCollectionInheritance()
        {
            string json;

            GenericClass<GenericItem<string>, string> foo1 = new GenericClass<GenericItem<string>, string>();
            foo1.Items.Add(new GenericItem<string> { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo1 });
            Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);

            GenericClass<NonGenericItem, string> foo2 = new GenericClass<NonGenericItem, string>();
            foo2.Items.Add(new NonGenericItem { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo2 });
            Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);

            NonGenericClass foo3 = new NonGenericClass();
            foo3.Items.Add(new NonGenericItem { Value = "Hello" });

            json = JsonConvert.SerializeObject(new { selectList = foo3 });
            Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);
        }

        [Fact]
        public void InheritedListSerialize()
        {
            Article a1 = new Article("a1");
            Article a2 = new Article("a2");

            ArticleCollection articles1 = new ArticleCollection();
            articles1.Add(a1);
            articles1.Add(a2);

            string jsonText = JsonConvert.SerializeObject(articles1);

            ArticleCollection articles2 = JsonConvert.DeserializeObject<ArticleCollection>(jsonText);

            Assert.Equal(articles1.Count, articles2.Count);
            Assert.Equal(articles1[0].Name, articles2[0].Name);
        }

        [Fact]
        public void ReadOnlyCollectionSerialize()
        {
            ReadOnlyCollection<int> r1 = new ReadOnlyCollection<int>(new int[] { 0, 1, 2, 3, 4 });

            string jsonText = JsonConvert.SerializeObject(r1);

            Assert.Equal("[0,1,2,3,4]", jsonText);

            ReadOnlyCollection<int> r2 = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>(jsonText);

            Assert.Equal(r1, r2);
        }

        [Fact]
        public void SerializeGenericList()
        {
            Product p1 = new Product
            {
                Name = "Product 1",
                Price = 99.95m,
                ExpiryDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            };
            Product p2 = new Product
            {
                Name = "Product 2",
                Price = 12.50m,
                ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            };

            List<Product> products = new List<Product>();
            products.Add(p1);
            products.Add(p2);

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);
            //[
            //  {
            //    "Name": "Product 1",
            //    "ExpiryDate": "\/Date(978048000000)\/",
            //    "Price": 99.95,
            //    "Sizes": null
            //  },
            //  {
            //    "Name": "Product 2",
            //    "ExpiryDate": "\/Date(1248998400000)\/",
            //    "Price": 12.50,
            //    "Sizes": null
            //  }
            //]

            StringAssert.Equal(@"[
  {
    ""Name"": ""Product 1"",
    ""ExpiryDate"": ""2000-12-29T00:00:00Z"",
    ""Price"": 99.95,
    ""Sizes"": null
  },
  {
    ""Name"": ""Product 2"",
    ""ExpiryDate"": ""2009-07-31T00:00:00Z"",
    ""Price"": 12.50,
    ""Sizes"": null
  }
]", json);
        }

        [Fact]
        public void DeserializeGenericList()
        {
            string json = @"[
        {
          ""Name"": ""Product 1"",
          ""ExpiryDate"": ""\/Date(978048000000)\/"",
          ""Price"": 99.95,
          ""Sizes"": null
        },
        {
          ""Name"": ""Product 2"",
          ""ExpiryDate"": ""\/Date(1248998400000)\/"",
          ""Price"": 12.50,
          ""Sizes"": null
        }
      ]";

            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);

            Console.WriteLine(products.Count);
            // 2

            Product p1 = products[0];

            Console.WriteLine(p1.Name);
            // Product 1

            Assert.Equal(2, products.Count);
            Assert.Equal("Product 1", products[0].Name);
        }
    }


    public class Array2D
    {
        public string Before { get; set; }
        public int[,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3D
    {
        public string Before { get; set; }
        public int[, ,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3DWithConverter
    {
        public string Before { get; set; }

        [JsonProperty(ItemConverterType = typeof(IntToFloatConverter))]
        public int[, ,] Coordinates { get; set; }

        public string After { get; set; }
    }

    public class GenericItem<T>
    {
        public T Value { get; set; }
    }

    public class NonGenericItem : GenericItem<string>
    {
    }

    public class GenericClass<T, TValue> : IEnumerable<T>
        where T : GenericItem<TValue>, new()
    {
        public IList<T> Items { get; set; }

        public GenericClass()
        {
            Items = new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Items != null)
            {
                foreach (T item in Items)
                {
                    yield return item;
                }
            }
            else
                yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NonGenericClass : GenericClass<GenericItem<string>, string>
    {
    }

    public class StringListAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> existingStrings = (List<string>)existingValue;
            List<string> newStrings = new List<string>(existingStrings);

            reader.Read();

            while (reader.TokenType != JsonToken.EndArray)
            {
                string s = (string)reader.Value;
                newStrings.Add(s);

                reader.Read();
            }

            return newStrings;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>));
        }
    }

    public class StringAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string existingString = (string)existingValue;
            string newString = existingString + (string)reader.Value;

            return newString;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(string));
        }
    }
}