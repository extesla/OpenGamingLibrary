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
#if NET40
using System.Numerics;
#endif
using OpenGamingLibrary.Json.Linq.JsonPath;
using OpenGamingLibrary.Json.Test.Bson;
using Xunit;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Xunit.Extensions;
using System.Linq;

namespace OpenGamingLibrary.Json.Test.Linq.JsonPath
{
    
    public class JPathExecuteTests : TestFixtureBase
    {
        [Fact]
        public void ParseWithEmptyArrayContent()
        {
            var json = @"{
    'controls': [
        {
            'messages': {
                'addSuggestion': {
                    'en-US': 'Add'
                }
            }
        },
        {
            'header': {
                'controls': []
            },
            'controls': [
                {
                    'controls': [
                        {
                            'defaultCaption': {
                                'en-US': 'Sort by'
                            },
                            'sortOptions': [
                                {
                                    'label': {
                                        'en-US': 'Name'
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
}";
            JObject jToken = JObject.Parse(json);
            IList<JToken> tokens = jToken.SelectTokens("$..en-US").ToList();

            Assert.Equal(3, tokens.Count);
            Assert.Equal("Add", (string)tokens[0]);
            Assert.Equal("Sort by", (string)tokens[1]);
            Assert.Equal("Name", (string)tokens[2]);
        }

        [Fact]
        public void SelectTokenAfterEmptyContainer()
        {
            string json = @"{
    'cont': [],
    'test': 'no one will find me'
}";

            JObject o = JObject.Parse(json);

            IList<JToken> results = o.SelectTokens("$..test").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal("no one will find me", (string)results[0]);
        }

        [Fact]
        public void EvaluatePropertyWithRequired()
        {
            string json = "{\"bookId\":\"1000\"}";
            JObject o = JObject.Parse(json);

            string bookId = (string)o.SelectToken("bookId", true);

            Assert.Equal("1000", bookId);
        }

        [Fact]
        public void EvaluateEmptyPropertyIndexer()
        {
            JObject o = new JObject(
                new JProperty("", 1));

            JToken t = o.SelectToken("['']");
            Assert.Equal(1, (int)t);
        }

        [Fact]
        public void EvaluateEmptyString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("");
            Assert.Equal(o, t);

            t = o.SelectToken("['']");
            Assert.Equal(null, t);
        }

        [Fact]
        public void EvaluateEmptyStringWithMatchingEmptyProperty()
        {
            JObject o = new JObject(
                new JProperty(" ", 1));

            JToken t = o.SelectToken("[' ']");
            Assert.Equal(1, (int)t);
        }

        [Fact]
        public void EvaluateWhitespaceString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken(" ");
            Assert.Equal(o, t);
        }

        [Fact]
        public void EvaluateDollarString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("$");
            Assert.Equal(o, t);
        }

        [Fact]
        public void EvaluateDollarTypeString()
        {
            JObject o = new JObject(
                new JProperty("$values", new JArray(1, 2, 3)));

            JToken t = o.SelectToken("$values[1]");
            Assert.Equal(2, (int)t);
        }

        [Fact]
        public void EvaluateSingleProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Blah");
            Assert.NotNull(t);
            Assert.Equal(JTokenType.Integer, t.Type);
            Assert.Equal(1, (int)t);
        }

        [Fact]
        public void EvaluateWildcardProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1),
                new JProperty("Blah2", 2));

            IList<JToken> t = o.SelectTokens("$.*").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.Equal(1, (int)t[0]);
            Assert.Equal(2, (int)t[1]);
        }

        [Fact]
        public void QuoteName()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("['Blah']");
            Assert.NotNull(t);
            Assert.Equal(JTokenType.Integer, t.Type);
            Assert.Equal(1, (int)t);
        }

        [Fact]
        public void EvaluateMissingProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Missing[1]");
            Assert.Null(t);
        }

        [Fact]
        public void EvaluateIndexerOnObject()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("[1]");
            Assert.Null(t);
        }

        [Fact]
        public void EvaluateIndexerOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            AssertException.Throws<JsonException>(() => { o.SelectToken("[1]", true); }, @"Index 1 not valid on JObject.");
        }

        [Fact]
        public void EvaluateWildcardIndexOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            AssertException.Throws<JsonException>(() => { o.SelectToken("[*]", true); }, @"Index * not valid on JObject.");
        }

        [Fact]
        public void EvaluateSliceOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            AssertException.Throws<JsonException>(() => { o.SelectToken("[:]", true); }, @"Array slice is not valid on JObject.");
        }

        [Fact]
        public void EvaluatePropertyOnArray()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("BlahBlah");
            Assert.Null(t);
        }

        [Fact]
        public void EvaluateMultipleResultsError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("[0, 1]"); }, @"Path returned multiple tokens.");
        }

        [Fact]
        public void EvaluatePropertyOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("BlahBlah", true); }, @"Property 'BlahBlah' not valid on JArray.");
        }

        [Fact]
        public void EvaluateNoResultsWithMultipleArrayIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("[9,10]", true); }, @"Index 9 outside the bounds of JArray.");
        }

        [Fact]
        public void EvaluateConstructorOutOfBoundsIndxerWithError()
        {
            JConstructor c = new JConstructor("Blah");

            AssertException.Throws<JsonException>(() => { c.SelectToken("[1]", true); }, @"Index 1 outside the bounds of JConstructor.");
        }

        [Fact]
        public void EvaluateConstructorOutOfBoundsIndxer()
        {
            JConstructor c = new JConstructor("Blah");

            Assert.Null(c.SelectToken("[1]"));
        }

        [Fact]
        public void EvaluateMissingPropertyWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            AssertException.Throws<JsonException>(() => { o.SelectToken("Missing", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [Fact]
        public void EvaluatePropertyWithoutError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JValue v = (JValue)o.SelectToken("Blah", true);
            Assert.Equal(1, v.Value);
        }

        [Fact]
        public void EvaluateMissingPropertyIndexWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            AssertException.Throws<JsonException>(() => { o.SelectToken("['Missing','Missing2']", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [Fact]
        public void EvaluateMultiPropertyIndexOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("['Missing','Missing2']", true); }, "Properties 'Missing', 'Missing2' not valid on JArray.");
        }

        [Fact]
        public void EvaluateArraySliceWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("[99:]", true); }, "Array slice of 99 to * returned no results.");

            AssertException.Throws<JsonException>(() => { a.SelectToken("[1:-19]", true); }, "Array slice of 1 to -19 returned no results.");

            AssertException.Throws<JsonException>(() => { a.SelectToken("[:-19]", true); }, "Array slice of * to -19 returned no results.");

            a = new JArray();

            AssertException.Throws<JsonException>(() => { a.SelectToken("[:]", true); }, "Array slice of * to * returned no results.");
        }

        [Fact]
        public void EvaluateOutOfBoundsIndxer()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("[1000].Ha");
            Assert.Null(t);
        }

        [Fact]
        public void EvaluateArrayOutOfBoundsIndxerWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            AssertException.Throws<JsonException>(() => { a.SelectToken("[1000].Ha", true); }, "Index 1000 outside the bounds of JArray.");
        }

        [Fact]
        public void EvaluateArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            JToken t = a.SelectToken("[1]");
            Assert.NotNull(t);
            Assert.Equal(JTokenType.Integer, t.Type);
            Assert.Equal(2, (int)t);
        }

        [Fact]
        public void EvaluateArraySlice()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);
            IList<JToken> t = null;

            t = a.SelectTokens("[-3:]").ToList();
            Assert.Equal(3, t.Count);
            Assert.Equal(7, (int)t[0]);
            Assert.Equal(8, (int)t[1]);
            Assert.Equal(9, (int)t[2]);

            t = a.SelectTokens("[-1:-2:-1]").ToList();
            Assert.Equal(1, t.Count);
            Assert.Equal(9, (int)t[0]);

            t = a.SelectTokens("[-2:-1]").ToList();
            Assert.Equal(1, t.Count);
            Assert.Equal(8, (int)t[0]);

            t = a.SelectTokens("[1:1]").ToList();
            Assert.Equal(0, t.Count);

            t = a.SelectTokens("[1:2]").ToList();
            Assert.Equal(1, t.Count);
            Assert.Equal(2, (int)t[0]);

            t = a.SelectTokens("[::-1]").ToList();
            Assert.Equal(9, t.Count);
            Assert.Equal(9, (int)t[0]);
            Assert.Equal(8, (int)t[1]);
            Assert.Equal(7, (int)t[2]);
            Assert.Equal(6, (int)t[3]);
            Assert.Equal(5, (int)t[4]);
            Assert.Equal(4, (int)t[5]);
            Assert.Equal(3, (int)t[6]);
            Assert.Equal(2, (int)t[7]);
            Assert.Equal(1, (int)t[8]);

            t = a.SelectTokens("[::-2]").ToList();
            Assert.Equal(5, t.Count);
            Assert.Equal(9, (int)t[0]);
            Assert.Equal(7, (int)t[1]);
            Assert.Equal(5, (int)t[2]);
            Assert.Equal(3, (int)t[3]);
            Assert.Equal(1, (int)t[4]);
        }

        [Fact]
        public void EvaluateWildcardArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            List<JToken> t = a.SelectTokens("[*]").ToList();
            Assert.NotNull(t);
            Assert.Equal(4, t.Count);
            Assert.Equal(1, (int)t[0]);
            Assert.Equal(2, (int)t[1]);
            Assert.Equal(3, (int)t[2]);
            Assert.Equal(4, (int)t[3]);
        }

        [Fact]
        public void EvaluateArrayMultipleIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4);

            IEnumerable<JToken> t = a.SelectTokens("[1,2,0]");
            Assert.NotNull(t);
            Assert.Equal(3, t.Count());
            Assert.Equal(2, (int)t.ElementAt(0));
            Assert.Equal(3, (int)t.ElementAt(1));
            Assert.Equal(1, (int)t.ElementAt(2));
        }

        [Fact]
        public void EvaluateScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.Equal(1, (int)t[0]);
            Assert.Equal(2, (int)t[1]);
        }

        [Fact]
        public void EvaluateWildcardScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.NotNull(t);
            Assert.Equal(5, t.Count);
            Assert.True(JToken.DeepEquals(a, t[0]));
            Assert.True(JToken.DeepEquals(o1, t[1]));
            Assert.Equal(1, (int)t[2]);
            Assert.True(JToken.DeepEquals(o2, t[3]));
            Assert.Equal(2, (int)t[4]);
        }

        [Fact]
        public void EvaluateScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.NotNull(t);
            Assert.Equal(4, t.Count);
            Assert.Equal(1, (int)t[0]);
            Assert.Equal(2, (int)t[1]);
            Assert.True(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[2]));
            Assert.True(JToken.DeepEquals(new JArray(3), t[3]));
        }

        [Fact]
        public void EvaluateWildcardScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.NotNull(t);
            Assert.Equal(9, t.Count);

            Assert.True(JToken.DeepEquals(a, t[0]));
            Assert.True(JToken.DeepEquals(o1, t[1]));
            Assert.Equal(1, (int)t[2]);
            Assert.True(JToken.DeepEquals(o2, t[3]));
            Assert.Equal(2, (int)t[4]);
            Assert.True(JToken.DeepEquals(o3, t[5]));
            Assert.True(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[6]));
            Assert.True(JToken.DeepEquals(new JArray(3), t[7]));
            Assert.Equal(3, (int)t[8]);
        }

        [Fact]
        public void EvaluateSinglePropertyReturningArray()
        {
            JObject o = new JObject(
                new JProperty("Blah", new[] { 1, 2, 3 }));

            JToken t = o.SelectToken("Blah");
            Assert.NotNull(t);
            Assert.Equal(JTokenType.Array, t.Type);

            t = o.SelectToken("Blah[2]");
            Assert.Equal(JTokenType.Integer, t.Type);
            Assert.Equal(3, (int)t);
        }

        [Fact]
        public void EvaluateLastSingleCharacterProperty()
        {
            JObject o2 = JObject.Parse("{'People':[{'N':'Jeff'}]}");
            string a2 = (string)o2.SelectToken("People[0].N");

            Assert.Equal("Jeff", a2);
        }

        [Fact]
        public void ExistsQuery()
        {
            JArray a = new JArray(new JObject(new JProperty("hi", "ho")), new JObject(new JProperty("hi2", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(1, t.Count);
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", "ho")), t[0]));
        }

        [Fact]
        public void EqualsQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", "ho")),
                new JObject(new JProperty("hi", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.['hi'] == 'ha' ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(1, t.Count);
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", "ha")), t[0]));
        }

        [Fact]
        public void NotEqualsQuery()
        {
            JArray a = new JArray(
                new JArray(new JObject(new JProperty("hi", "ho"))),
                new JArray(new JObject(new JProperty("hi", "ha"))));

            IList<JToken> t = a.SelectTokens("[ ?( @..hi <> 'ha' ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(1, t.Count);
            Assert.True(JToken.DeepEquals(new JArray(new JObject(new JProperty("hi", "ho"))), t[0]));
        }

        [Fact]
        public void NoPathQuery()
        {
            JArray a = new JArray(1, 2, 3);

            IList<JToken> t = a.SelectTokens("[ ?( @ > 1 ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.Equal(2, (int)t[0]);
            Assert.Equal(3, (int)t[1]);
        }

        [Fact]
        public void MultipleQueries()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);

            // json path does item based evaluation - http://www.sitepen.com/blog/2008/03/17/jsonpath-support/
            // first query resolves array to ints
            // int has no children to query
            IList<JToken> t = a.SelectTokens("[?(@ <> 1)][?(@ <> 4)][?(@ < 7)]").ToList();
            Assert.NotNull(t);
            Assert.Equal(0, t.Count);
        }

        [Fact]
        public void GreaterQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }

#if NET40
        [Fact]
        public void GreaterQueryBigInteger()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", new BigInteger(1))),
                new JObject(new JProperty("hi", new BigInteger(2))),
                new JObject(new JProperty("hi", new BigInteger(3))));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }
#endif

        [Fact]
        public void GreaterOrEqualQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 2.0)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi >= 1 ) ]").ToList();
            Assert.NotNull(t);
            Assert.Equal(4, t.Count);
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 1)), t[0]));
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[1]));
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 2.0)), t[2]));
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[3]));
        }

        [Fact]
        public void NestedQuery()
        {
            JArray a = new JArray(
                new JObject(
                    new JProperty("name", "Bad Boys"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "Independence Day"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "The Rock"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Nick Cage")))))
                );

            IList<JToken> t = a.SelectTokens("[?(@.cast[?(@.name=='Will Smith')])].name").ToList();
            Assert.NotNull(t);
            Assert.Equal(2, t.Count);
            Assert.Equal("Bad Boys", (string)t[0]);
            Assert.Equal("Independence Day", (string)t[1]);
        }

        [Fact]
        public void PathWithConstructor()
        {
            JArray a = JArray.Parse(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]");

            JValue v = (JValue)a.SelectToken("[1].Property2[1][0]");
            Assert.Equal(1L, v.Value);
        }


        [Fact]
        public void Example()
        {
            JObject o = JObject.Parse(@"{
        ""Stores"": [
          ""Lambton Quay"",
          ""Willis Street""
        ],
        ""Manufacturers"": [
          {
            ""Name"": ""Acme Co"",
            ""Products"": [
              {
                ""Name"": ""Anvil"",
                ""Price"": 50
              }
            ]
          },
          {
            ""Name"": ""Contoso"",
            ""Products"": [
              {
                ""Name"": ""Elbow Grease"",
                ""Price"": 99.95
              },
              {
                ""Name"": ""Headlight Fluid"",
                ""Price"": 4
              }
            ]
          }
        ]
      }");

            string name = (string)o.SelectToken("Manufacturers[0].Name");
            // Acme Co

            decimal productPrice = (decimal)o.SelectToken("Manufacturers[0].Products[0].Price");
            // 50

            string productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
            // Elbow Grease

            Assert.Equal("Acme Co", name);
            Assert.Equal(50m, productPrice);
            Assert.Equal("Elbow Grease", productName);

            IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();
            // Lambton Quay
            // Willis Street

            IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name")).ToList();
            // null
            // Headlight Fluid

            decimal totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));
            // 149.95

            Assert.Equal(2, storeNames.Count);
            Assert.Equal("Lambton Quay", storeNames[0]);
            Assert.Equal("Willis Street", storeNames[1]);
            Assert.Equal(2, firstProductNames.Count);
            Assert.Equal(null, firstProductNames[0]);
            Assert.Equal("Headlight Fluid", firstProductNames[1]);
            Assert.Equal(149.95m, totalPrice);
        }
    }
}