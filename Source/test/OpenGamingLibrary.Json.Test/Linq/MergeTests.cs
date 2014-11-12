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
#if NET20
using OpenGamingLibrary.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;
using OpenGamingLibrary.Json.Linq;
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

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class MergeTests : TestFixtureBase
    {
        [Fact]
        public void MergeObjectProperty()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Property1 = 1
            });
            var right = (JObject)JToken.FromObject(new
            {
                Property2 = 2
            });

            left.Merge(right);

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Property1"": 1,
  ""Property2"": 2
}", json);
        }

        [Fact]
        public void MergeChildObject()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Property1 = new { SubProperty1 = 1 }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Property1 = new { SubProperty2 = 2 }
            });

            left.Merge(right);

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  }
}", json);
        }

        [Fact]
        public void MergeMismatchedTypesRoot()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Property1 = new { SubProperty1 = 1 }
            });
            var right = (JArray)JToken.FromObject(new object[]
                {
                    new { Property1 = 1 },
                    new { Property1 = 1 }
                });

            left.Merge(right);

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1
  }
}", json);
        }

        [Fact]
        public void MergeMultipleObjects()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Property1 = new { SubProperty1 = 1 }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Property1 = new { SubProperty2 = 2 },
                Property2 = 2
            });

            left.Merge(right);

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  },
  ""Property2"": 2
}", json);
        }

        [Fact]
        public void MergeArray()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new
                    {
                        Property1 = new
                        {
                            Property1 = 1,
                            Property2 = 2,
                            Property3 = 3,
                            Property4 = 4,
                            Property5 = (object)null
                        }
                    },
                    new { },
                    3,
                    null,
                    5,
                    null
                }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new
                    {
                        Property1 = new
                        {
                            Property1 = (object)null,
                            Property2 = 3,
                            Property3 = new
                            {

                            },
                            Property5 = (object)null
                        }
                    },
                    null,
                    null,
                    4,
                    5.1,
                    null,
                    new
                    {
                        Property1 = 1
                    }
                }
            });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge
            });

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": {
        ""Property1"": 1,
        ""Property2"": 3,
        ""Property3"": {},
        ""Property4"": 4,
        ""Property5"": null
      }
    },
    {},
    3,
    4,
    5.1,
    null,
    {
      ""Property1"": 1
    }
  ]
}", json);
        }

        [Fact]
        public void ConcatArray()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new { Property1 = 1 },
                    new { Property1 = 1 }
                }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new { Property1 = 1 },
                    new { Property2 = 2 },
                    new { Property3 = 3 }
                }
            });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat
            });

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
        }

        [Fact]
        public void MergeMismatchingTypesInArray()
        {
            var left = (JArray)JToken.FromObject(new object[]
                {
                    true,
                    null,
                    new { Property1 = 1 },
                    new object[] { 1 },
                    new { Property1 = 1 },
                    1,
                    new object[] { 1 }
                });
            var right = (JArray)JToken.FromObject(new object[]
                {
                    1,
                    5,
                    new object[] { 1 },
                    new { Property1 = 1 },
                    true,
                    new { Property1 = 1 },
                    null
                });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge
            });

            string json = left.ToString();

            StringAssert.Equal(@"[
  1,
  5,
  {
    ""Property1"": 1
  },
  [
    1
  ],
  {
    ""Property1"": 1
  },
  {
    ""Property1"": 1
  },
  [
    1
  ]
]", json);
        }

        [Fact]
        public void MergeMismatchingTypesInObject()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Property1 = new object[]
                {
                    1
                },
                Property2 = new object[]
                {
                    1
                },
                Property3 = true,
                Property4 = true
            });
            var right = (JObject)JToken.FromObject(new
            {
                Property1 = new { Nested = true },
                Property2 = true,
                Property3 = new object[]
                {
                    1
                },
                Property4 = (object)null
            });

            left.Merge(right);

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Property1"": {
    ""Nested"": true
  },
  ""Property2"": true,
  ""Property3"": [
    1
  ],
  ""Property4"": true
}", json);
        }

        [Fact]
        public void MergeArrayOverwrite_Nested()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    1,
                    2,
                    3
                }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    4,
                    5
                }
            });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace
            });

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Array1"": [
    4,
    5
  ]
}", json);
        }

        [Fact]
        public void MergeArrayOverwrite_Root()
        {
            var left = (JArray)JToken.FromObject(new object[]
            {
                1,
                2,
                3
            });
            var right = (JArray)JToken.FromObject(new object[]
            {
                4,
                5
            });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace
            });

            string json = left.ToString();

            StringAssert.Equal(@"[
  4,
  5
]", json);
        }

        [Fact]
        public void UnionArrays()
        {
            var left = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new { Property1 = 1 },
                    new { Property1 = 1 }
                }
            });
            var right = (JObject)JToken.FromObject(new
            {
                Array1 = new object[]
                {
                    new { Property1 = 1 },
                    new { Property2 = 2 },
                    new { Property3 = 3 }
                }
            });

            left.Merge(right, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            string json = left.ToString();

            StringAssert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
        }

        [Fact]
        public void MergeJProperty()
        {
            JProperty p1 = new JProperty("p1", 1);
            JProperty p2 = new JProperty("p2", 2);

            p1.Merge(p2);
            Assert.Equal(2, (int)p1.Value);

            JProperty p3 = new JProperty("p3");

            p1.Merge(p3);
            Assert.Equal(2, (int)p1.Value);

            JProperty p4 = new JProperty("p4", null);

            p1.Merge(p4);
            Assert.Equal(2, (int)p1.Value);
        }

        [Fact]
        public void MergeJConstructor()
        {
            JConstructor c1 = new JConstructor("c1", new[] { 1, 2 });
            JConstructor c2 = new JConstructor("c2", new[] { 3, 4 });

            c1.Merge(c2);
            Assert.Equal("c2", c1.Name);
            Assert.Equal(new[] { 1, 2, 3, 4 }, c1.Select(i => (int)i));

            JConstructor c3 = new JConstructor();
            c1.Merge(c3);
            Assert.Equal("c2", c1.Name);

            JConstructor c4 = new JConstructor("c4", new[] { 5, 6 });
            c1.Merge(c4, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace
            });
            Assert.Equal("c4", c1.Name);
            Assert.Equal(new[] { 5, 6 }, c1.Select(i => (int)i));
        }

        [Fact]
        public void MergeDefaultContainers()
        {
            JConstructor c = new JConstructor();
            c.Merge(new JConstructor());
            Assert.Equal(null, c.Name);
            Assert.Equal(0, c.Count);

            JObject o = new JObject();
            o.Merge(new JObject());
            Assert.Equal(0, o.Count);

            JArray a = new JArray();
            a.Merge(new JArray());
            Assert.Equal(0, a.Count);

            JProperty p = new JProperty("name1");
            p.Merge(new JProperty("name2"));
            Assert.Equal("name1", p.Name);
            Assert.Equal(0, p.Count);
        }

        [Fact]
        public void MergeNull()
        {
            JConstructor c = new JConstructor();
            c.Merge(null);
            Assert.Equal(null, c.Name);
            Assert.Equal(0, c.Count);

            JObject o = new JObject();
            o.Merge(null);
            Assert.Equal(0, o.Count);

            JArray a = new JArray();
            a.Merge(null);
            Assert.Equal(0, a.Count);

            JProperty p = new JProperty("name1");
            p.Merge(null);
            Assert.Equal("name1", p.Name);
            Assert.Equal(0, p.Count);
        }
    }
}