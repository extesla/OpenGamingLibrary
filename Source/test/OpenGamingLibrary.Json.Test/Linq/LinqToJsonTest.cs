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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Test.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using System.Linq;
using System.IO;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class LinqToJsonTest : TestFixtureBase
    {
        [Fact]
        public void EmptyJEnumerableCount()
        {
            JEnumerable<JToken> tokens = new JEnumerable<JToken>();

            Assert.Equal(0, tokens.Count());
        }

        [Fact]
        public void EmptyJEnumerableAsEnumerable()
        {
            IEnumerable tokens = new JEnumerable<JToken>();

            Assert.Equal(0, tokens.Cast<JToken>().Count());
        }

        [Fact]
        public void EmptyJEnumerableEquals()
        {
            JEnumerable<JToken> tokens1 = new JEnumerable<JToken>();
            JEnumerable<JToken> tokens2 = new JEnumerable<JToken>();

            Assert.True(tokens1.Equals(tokens2));

            object o1 = new JEnumerable<JToken>();
            object o2 = new JEnumerable<JToken>();

            Assert.True(o1.Equals(o2));
        }

        [Fact]
        public void EmptyJEnumerableGetHashCode()
        {
            JEnumerable<JToken> tokens = new JEnumerable<JToken>();

            Assert.Equal(0, tokens.GetHashCode());
        }

        [Fact]
        public void CommentsAndReadFrom()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)JToken.ReadFrom(jsonReader);

            Assert.Equal(4, a.Count);
            Assert.Equal(JTokenType.Comment, a[0].Type);
            Assert.Equal(" hi", ((JValue)a[0]).Value);
        }

        [Fact]
        public void StartingCommentAndReadFrom()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)JToken.ReadFrom(jsonReader);

            Assert.Equal(JTokenType.Comment, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.Equal(true, lineInfo.HasLineInfo());
            Assert.Equal(3, lineInfo.LineNumber);
            Assert.Equal(1, lineInfo.LinePosition);
        }

        [Fact]
        public void StartingUndefinedAndReadFrom()
        {
            StringReader textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)JToken.ReadFrom(jsonReader);

            Assert.Equal(JTokenType.Undefined, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.Equal(true, lineInfo.HasLineInfo());
            Assert.Equal(2, lineInfo.LineNumber);
            Assert.Equal(10, lineInfo.LinePosition);
        }

        [Fact]
        public void StartingEndArrayAndReadFrom()
        {
            StringReader textReader = new StringReader(@"[]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            jsonReader.Read();
            jsonReader.Read();

            AssertException.Throws<JsonReaderException>(() => JToken.ReadFrom(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
        }

        [Fact]
        public void JPropertyPath()
        {
            JObject o = new JObject
            {
                {
                    "person",
                    new JObject
                    {
                        { "$id", 1 }
                    }
                }
            };

            JContainer idProperty = o["person"]["$id"].Parent;
            Assert.Equal("person.$id", idProperty.Path);
        }

        [Fact]
        public void EscapedPath()
        {
            string json = @"{
  ""frameworks"": {
    ""aspnetcore50"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": ""NuGet""
        }
      }
    }
  }
}";

            JObject o = JObject.Parse(json);

            JToken v1 = o["frameworks"]["aspnetcore50"]["dependencies"]["System.Xml.ReaderWriter"]["source"];

            Console.WriteLine(v1.Path);

            JToken v2 = o.SelectToken(v1.Path);

            Assert.Equal(v1, v2);
        }

        [Fact]
        public void EscapedPathTests()
        {
            EscapedPathAssert("this has spaces", "['this has spaces']");
            EscapedPathAssert("(RoundBraces)", "['(RoundBraces)']");
            EscapedPathAssert("[SquareBraces]", "['[SquareBraces]']");
            EscapedPathAssert("this.has.dots", "['this.has.dots']");
        }

        private void EscapedPathAssert(string propertyName, string expectedPath)
        {
            int v1 = int.MaxValue;
            JValue value = new JValue(v1);

            JObject o = new JObject(new JProperty(propertyName, value));

            Assert.Equal(expectedPath, value.Path);

            JValue selectedValue = (JValue)o.SelectToken(value.Path);

            Assert.Equal(value, selectedValue);
        }

        [Fact]
        public void ForEach()
        {
            JArray items = new JArray(new JObject(new JProperty("name", "value!")));

            foreach (JObject friend in items)
            {
                Console.WriteLine(friend);
            }
        }

        [Fact]
        public void DoubleValue()
        {
            JArray j = JArray.Parse("[-1E+4,100.0e-2]");

            double value = (double)j[0];
            Assert.Equal(-10000d, value);

            value = (double)j[1];
            Assert.Equal(1d, value);
        }

        [Fact]
        public void Manual()
        {
            JArray array = new JArray();
            JValue text = new JValue("Manual text");
            JValue date = new JValue(new DateTime(2000, 5, 23));

            array.Add(text);
            array.Add(date);

            string json = array.ToString();
            // [
            //   "Manual text",
            //   "\/Date(958996800000+1200)\/"
            // ]
        }

        [Fact]
        public void LinqToJsonDeserialize()
        {
            JObject o = new JObject(
                new JProperty("Name", "John Smith"),
                new JProperty("BirthDate", new DateTime(1983, 3, 20))
                );

            JsonSerializer serializer = new JsonSerializer();
            Person p = (Person)serializer.Deserialize(new JTokenReader(o), typeof(Person));

            // John Smith
            Console.WriteLine(p.Name);
        }

        [Fact]
        public void ObjectParse()
        {
            string json = @"{
        CPU: 'Intel',
        Drives: [
          'DVD read/writer',
          ""500 gigabyte hard drive""
        ]
      }";

            JObject o = JObject.Parse(json);
            IList<JProperty> properties = o.Properties().ToList();

            Assert.Equal("CPU", properties[0].Name);
            Assert.Equal("Intel", (string)properties[0].Value);
            Assert.Equal("Drives", properties[1].Name);

            JArray list = (JArray)properties[1].Value;
            Assert.Equal(2, list.Children().Count());
            Assert.Equal("DVD read/writer", (string)list.Children().ElementAt(0));
            Assert.Equal("500 gigabyte hard drive", (string)list.Children().ElementAt(1));

            List<object> parameterValues =
                (from p in o.Properties()
                    where p.Value is JValue
                    select ((JValue)p.Value).Value).ToList();

            Assert.Equal(1, parameterValues.Count);
            Assert.Equal("Intel", parameterValues[0]);
        }

        [Fact]
        public void CreateLongArray()
        {
            string json = @"[0,1,2,3,4,5,6,7,8,9]";

            JArray a = JArray.Parse(json);
            List<int> list = a.Values<int>().ToList();

            List<int> expected = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.Equal(expected, list);
        }

        [Fact]
        public void GoogleSearchAPI()
        {
            #region GoogleJson
            string json = @"{
    results:
        [
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://www.google.com/"",
                url : ""http://www.google.com/"",
                visibleUrl : ""www.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:zhool8dxBV4J:www.google.com"",
                title : ""Google"",
                titleNoFormatting : ""Google"",
                content : ""Enables users to search the Web, Usenet, and 
images. Features include PageRank,   caching and translation of 
results, and an option to find similar pages.""
            },
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://news.google.com/"",
                url : ""http://news.google.com/"",
                visibleUrl : ""news.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:Va_XShOz_twJ:news.google.com"",
                title : ""Google News"",
                titleNoFormatting : ""Google News"",
                content : ""Aggregated headlines and a search engine of many of the world's news sources.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://groups.google.com/"",
                url : ""http://groups.google.com/"",
                visibleUrl : ""groups.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:x2uPD3hfkn0J:groups.google.com"",
                title : ""Google Groups"",
                titleNoFormatting : ""Google Groups"",
                content : ""Enables users to search and browse the Usenet 
archives which consist of over 700   million messages, and post new 
comments.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://maps.google.com/"",
                url : ""http://maps.google.com/"",
                visibleUrl : ""maps.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:dkf5u2twBXIJ:maps.google.com"",
                title : ""Google Maps"",
                titleNoFormatting : ""Google Maps"",
                content : ""Provides directions, interactive maps, and 
satellite/aerial imagery of the United   States. Can also search by 
keyword such as type of business.""
            }
        ],
        
    adResults:
        [
            {
                GsearchResultClass:""GwebSearch.ad"",
                title : ""Gartner Symposium/ITxpo"",
                content1 : ""Meet brilliant Gartner IT analysts"",
                content2 : ""20-23 May 2007- Barcelona, Spain"",
                url : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                impressionUrl : 
""http://www.google.com/uds/css/ad-indicator-on.gif?ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB"", 

                unescapedUrl : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                visibleUrl : ""www.gartner.com""
            }
        ]
}
";
            #endregion

            JObject o = JObject.Parse(json);

            List<JObject> resultObjects = o["results"].Children<JObject>().ToList();

            Assert.Equal(32, resultObjects.Properties().Count());

            Assert.Equal(32, resultObjects.Cast<JToken>().Values().Count());

            Assert.Equal(4, resultObjects.Cast<JToken>().Values("GsearchResultClass").Count());

            Assert.Equal(5, o.PropertyValues().Cast<JArray>().Children().Count());

            List<string> resultUrls = o["results"].Children().Values<string>("url").ToList();

            List<string> expectedUrls = new List<string>() { "http://www.google.com/", "http://news.google.com/", "http://groups.google.com/", "http://maps.google.com/" };

            Assert.Equal(expectedUrls, resultUrls);

            List<JToken> descendants = o.Descendants().ToList();
            Assert.Equal(89, descendants.Count);
        }

        [Fact]
        public void JTokenToString()
        {
            string json = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            JObject o = JObject.Parse(json);

            StringAssert.Equal(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}", o.ToString());

            JArray list = o.Value<JArray>("Drives");

            StringAssert.Equal(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", list.ToString());

            JProperty cpuProperty = o.Property("CPU");
            Assert.Equal(@"""CPU"": ""Intel""", cpuProperty.ToString());

            JProperty drivesProperty = o.Property("Drives");
            StringAssert.Equal(@"""Drives"": [
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", drivesProperty.ToString());
        }

        [Fact]
        public void JTokenToStringTypes()
        {
            string json = @"{""Color"":2,""Establised"":new Date(1264118400000),""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":new Date(949532490000),""Price"":0},{""Name"":""Alien"",""ExpiryDate"":new Date(-62135596800000),""Price"":0}]}";

            JObject o = JObject.Parse(json);

            StringAssert.Equal(@"""Establised"": new Date(
  1264118400000
)", o.Property("Establised").ToString());
            StringAssert.Equal(@"new Date(
  1264118400000
)", o.Property("Establised").Value.ToString());
            Assert.Equal(@"""Width"": 1.1", o.Property("Width").ToString());
            Assert.Equal(@"1.1", ((JValue)o.Property("Width").Value).ToString(CultureInfo.InvariantCulture));
            Assert.Equal(@"""Open"": false", o.Property("Open").ToString());
            Assert.Equal(@"False", o.Property("Open").Value.ToString());

            json = @"[null,undefined]";

            JArray a = JArray.Parse(json);
            StringAssert.Equal(@"[
  null,
  undefined
]", a.ToString());
            Assert.Equal(@"", a.Children().ElementAt(0).ToString());
            Assert.Equal(@"", a.Children().ElementAt(1).ToString());
        }

        [Fact]
        public void CreateJTokenTree()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", "Test1Value"),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            Assert.Equal(4, o.Properties().Count());

            StringAssert.Equal(@"{
  ""Test1"": ""Test1Value"",
  ""Test2"": ""Test2Value"",
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", o.ToString());

            JArray a =
                new JArray(
                    o,
                    new DateTime(2000, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                    55,
                    new JArray(
                        "1",
                        2,
                        3.0,
                        new DateTime(4, 5, 6, 7, 8, 9, DateTimeKind.Utc)
                        ),
                    new JConstructor(
                        "ConstructorName",
                        "param1",
                        2,
                        3.0
                        )
                    );

            Assert.Equal(5, a.Count());
            StringAssert.Equal(@"[
  {
    ""Test1"": ""Test1Value"",
    ""Test2"": ""Test2Value"",
    ""Test3"": ""Test3Value"",
    ""Test4"": null
  },
  ""2000-10-10T00:00:00Z"",
  55,
  [
    ""1"",
    2,
    3.0,
    ""0004-05-06T07:08:09Z""
  ],
  new ConstructorName(
    ""param1"",
    2,
    3.0
  )
]", a.ToString());
        }

        private class Post
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public IList<string> Categories { get; set; }
        }

        private List<Post> GetPosts()
        {
            return new List<Post>()
            {
                new Post()
                {
                    Title = "LINQ to JSON beta",
                    Description = "Annoucing LINQ to JSON",
                    Link = "http://james.newtonking.com/projects/json-net.aspx",
                    Categories = new List<string>() { "Json.NET", "LINQ" }
                },
                new Post()
                {
                    Title = "Json.NET 1.3 + New license + Now on CodePlex",
                    Description = "Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
                    Link = "http://james.newtonking.com/projects/json-net.aspx",
                    Categories = new List<string>() { "Json.NET", "CodePlex" }
                }
            };
        }

        [Fact]
        public void FromObjectExample()
        {
            Post p = new Post
            {
                Title = "How to use FromObject",
                Categories = new [] { "LINQ to JSON" }
            };

            // serialize Post to JSON then parse JSON – SLOW!
            //JObject o = JObject.Parse(JsonConvert.SerializeObject(p));

            // create JObject directly from the Post
            JObject o = JObject.FromObject(p);

            o["Title"] = o["Title"] + " - Super effective!";

            string json = o.ToString();
            // {
            //   "Title": "How to use FromObject - It's super effective!",
            //   "Categories": [
            //     "LINQ to JSON"
            //   ]
            // }

            StringAssert.Equal(@"{
  ""Title"": ""How to use FromObject - Super effective!"",
  ""Description"": null,
  ""Link"": null,
  ""Categories"": [
    ""LINQ to JSON""
  ]
}", json);
        }

        [Fact]
        public void QueryingExample()
        {
            JArray posts = JArray.Parse(@"[
              {
                'Title': 'JSON Serializer Basics',
                'Date': '2013-12-21T00:00:00',
                'Categories': []
              },
              {
                'Title': 'Querying LINQ to JSON',
                'Date': '2014-06-03T00:00:00',
                'Categories': [
                  'LINQ to JSON'
                ]
              }
            ]");

            JToken serializerBasics = posts
                .Single(p => (string)p["Title"] == "JSON Serializer Basics");
            // JSON Serializer Basics

            IList<JToken> since2012 = posts
                .Where(p => (DateTime)p["Date"] > new DateTime(2012, 1, 1)).ToList();
            // JSON Serializer Basics
            // Querying LINQ to JSON

            IList<JToken> linqToJson = posts
                .Where(p => p["Categories"].Any(c => (string)c == "LINQ to JSON")).ToList();
            // Querying LINQ to JSON

            Assert.NotNull(serializerBasics);
            Assert.Equal(2, since2012.Count);
            Assert.Equal(1, linqToJson.Count);
        }

        [Fact]
        public void CreateJTokenTreeNested()
        {
            List<Post> posts = GetPosts();

            JObject rss =
                new JObject(
                    new JProperty("channel",
                        new JObject(
                            new JProperty("title", "James Newton-King"),
                            new JProperty("link", "http://james.newtonking.com"),
                            new JProperty("description", "James Newton-King's blog."),
                            new JProperty("item",
                                new JArray(
                                    from p in posts
                                    orderby p.Title
                                    select new JObject(
                                        new JProperty("title", p.Title),
                                        new JProperty("description", p.Description),
                                        new JProperty("link", p.Link),
                                        new JProperty("category",
                                            new JArray(
                                                from c in p.Categories
                                                select new JValue(c)))))))));

            Console.WriteLine(rss.ToString());

            //{
            //  "channel": {
            //    "title": "James Newton-King",
            //    "link": "http://james.newtonking.com",
            //    "description": "James Newton-King's blog.",
            //    "item": [
            //      {
            //        "title": "Json.NET 1.3 + New license + Now on CodePlex",
            //        "description": "Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
            //        "link": "http://james.newtonking.com/projects/json-net.aspx",
            //        "category": [
            //          "Json.NET",
            //          "CodePlex"
            //        ]
            //      },
            //      {
            //        "title": "LINQ to JSON beta",
            //        "description": "Annoucing LINQ to JSON",
            //        "link": "http://james.newtonking.com/projects/json-net.aspx",
            //        "category": [
            //          "Json.NET",
            //          "LINQ"
            //        ]
            //      }
            //    ]
            //  }
            //}

            var postTitles =
                from p in rss["channel"]["item"]
                select p.Value<string>("title");

            foreach (var item in postTitles)
            {
                Console.WriteLine(item);
            }

            //LINQ to JSON beta
            //Json.NET 1.3 + New license + Now on CodePlex

            var categories =
                from c in rss["channel"]["item"].Children()["category"].Values<string>()
                group c by c
                into g
                orderby g.Count() descending
                select new { Category = g.Key, Count = g.Count() };

            foreach (var c in categories)
            {
                Console.WriteLine(c.Category + " - Count: " + c.Count);
            }

            //Json.NET - Count: 2
            //LINQ - Count: 1
            //CodePlex - Count: 1
        }

        [Fact]
        public void BasicQuerying()
        {
            string json = @"{
                        ""channel"": {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      }";

            JObject o = JObject.Parse(json);

            Assert.Equal(null, o["purple"]);
            Assert.Equal(null, o.Value<string>("purple"));

            Assert.IsType(typeof(JArray), o["channel"]["item"]);

            Assert.Equal(2, o["channel"]["item"].Children()["title"].Count());
            Assert.Equal(0, o["channel"]["item"].Children()["monkey"].Count());

            Assert.Equal("Json.NET 1.3 + New license + Now on CodePlex", (string)o["channel"]["item"][0]["title"]);

            Assert.Equal(new string[] { "Json.NET 1.3 + New license + Now on CodePlex", "LINQ to JSON beta" }, o["channel"]["item"].Children().Values<string>("title").ToArray());
        }

        [Fact]
        public void JObjectIntIndex()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JObject o = new JObject();
                Assert.Equal(null, o[0]);
            }, "Accessed JObject values with invalid key value: 0. Object property name expected.");
        }

        [Fact]
        public void JArrayStringIndex()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JArray a = new JArray();
                Assert.Equal(null, a["purple"]);
            }, @"Accessed JArray values with invalid key value: ""purple"". Array position index expected.");
        }

        [Fact]
        public void JConstructorStringIndex()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JConstructor c = new JConstructor("ConstructorValue");
                Assert.Equal(null, c["purple"]);
            }, @"Accessed JConstructor values with invalid key value: ""purple"". Argument position index expected.");
        }

#if !NET20
        [Fact]
        public void ToStringJsonConverter()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            serializer.Serialize(writer, o);

            string json = sw.ToString();

            StringAssert.Equal(@"{
  ""Test1"": new Date(
    971586305000
  ),
  ""Test2"": new Date(
    971546045000
  ),
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", json);
        }

        [Fact]
        public void DateTimeOffset()
        {
            List<DateTimeOffset> testDates = new List<DateTimeOffset>
            {
                new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
            };

            JsonSerializer jsonSerializer = new JsonSerializer();

            JTokenWriter jsonWriter;
            using (jsonWriter = new JTokenWriter())
            {
                jsonSerializer.Serialize(jsonWriter, testDates);
            }

            Assert.Equal(4, jsonWriter.Token.Children().Count());
        }
#endif

        [Fact]
        public void FromObject()
        {
            List<Post> posts = GetPosts();

            JObject o = JObject.FromObject(new
            {
                channel = new
                {
                    title = "James Newton-King",
                    link = "http://james.newtonking.com",
                    description = "James Newton-King's blog.",
                    item =
                        from p in posts
                        orderby p.Title
                        select new
                        {
                            title = p.Title,
                            description = p.Description,
                            link = p.Link,
                            category = p.Categories
                        }
                }
            });

            Console.WriteLine(o.ToString());
            Assert.IsType(typeof(JObject), o);
            Assert.IsType(typeof(JObject), o["channel"]);
            Assert.Equal("James Newton-King", (string)o["channel"]["title"]);
            Assert.Equal(2, o["channel"]["item"].Children().Count());

            JArray a = JArray.FromObject(new List<int>() { 0, 1, 2, 3, 4 });
            Assert.IsType(typeof(JArray), a);
            Assert.Equal(5, a.Count());
        }

        [Fact]
        public void FromAnonDictionary()
        {
            List<Post> posts = GetPosts();

            JObject o = JObject.FromObject(new
            {
                channel = new Dictionary<string, object>
                {
                    { "title", "James Newton-King" },
                    { "link", "http://james.newtonking.com" },
                    { "description", "James Newton-King's blog." },
                    {
                        "item",
                        (from p in posts
                            orderby p.Title
                            select new
                            {
                                title = p.Title,
                                description = p.Description,
                                link = p.Link,
                                category = p.Categories
                            })
                    }
                }
            });

            Console.WriteLine(o.ToString());
            Assert.IsType(typeof(JObject), o);
            Assert.IsType(typeof(JObject), o["channel"]);
            Assert.Equal("James Newton-King", (string)o["channel"]["title"]);
            Assert.Equal(2, o["channel"]["item"].Children().Count());

            JArray a = JArray.FromObject(new List<int>() { 0, 1, 2, 3, 4 });
            Assert.IsType(typeof(JArray), a);
            Assert.Equal(5, a.Count());
        }

        [Fact]
        public void AsJEnumerable()
        {
            JObject o = null;
            IJEnumerable<JToken> enumerable = null;

            enumerable = o.AsJEnumerable();
            Assert.Null(enumerable);

            o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", null)
                    );

            enumerable = o.AsJEnumerable();
            Assert.NotNull(enumerable);
            Assert.Equal(o, enumerable);

            DateTime d = enumerable["Test1"].Value<DateTime>();

            Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), d);
        }

        [Fact]
        public void LinqCast()
        {
            JToken olist = JArray.Parse("[12,55]");

            List<int> list1 = olist.AsEnumerable().Values<int>().ToList();

            Assert.Equal(12, list1[0]);
            Assert.Equal(55, list1[1]);
        }

        [Fact]
        public void ChildrenExtension()
        {
            const string json = @"[
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        },
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      ]";

            JArray o = JArray.Parse(json);

            Assert.Equal(4, o.Children()["item"].Children()["title"].Count());
            Assert.Equal(new string[]
            {
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta",
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta"
            },
                o.Children()["item"].Children()["title"].Values<string>().ToArray());
        }

        [Fact]
        public void UriGuidTimeSpanTestClassEmptyTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass();
            JObject o = JObject.FromObject(c1);

            StringAssert.Equal(@"{
  ""Guid"": ""00000000-0000-0000-0000-000000000000"",
  ""NullableGuid"": null,
  ""TimeSpan"": ""00:00:00"",
  ""NullableTimeSpan"": null,
  ""Uri"": null
}", o.ToString());

            UriGuidTimeSpanTestClass c2 = o.ToObject<UriGuidTimeSpanTestClass>();
            Assert.Equal(c1.Guid, c2.Guid);
            Assert.Equal(c1.NullableGuid, c2.NullableGuid);
            Assert.Equal(c1.TimeSpan, c2.TimeSpan);
            Assert.Equal(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.Equal(c1.Uri, c2.Uri);
        }

        [Fact]
        public void UriGuidTimeSpanTestClassValuesTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass
            {
                Guid = new Guid("1924129C-F7E0-40F3-9607-9939C531395A"),
                NullableGuid = new Guid("9E9F3ADF-E017-4F72-91E0-617EBE85967D"),
                TimeSpan = TimeSpan.FromDays(1),
                NullableTimeSpan = TimeSpan.FromHours(1),
                Uri = new Uri("http://testuri.com")
            };
            JObject o = JObject.FromObject(c1);

            StringAssert.Equal(@"{
  ""Guid"": ""1924129c-f7e0-40f3-9607-9939c531395a"",
  ""NullableGuid"": ""9e9f3adf-e017-4f72-91e0-617ebe85967d"",
  ""TimeSpan"": ""1.00:00:00"",
  ""NullableTimeSpan"": ""01:00:00"",
  ""Uri"": ""http://testuri.com/""
}", o.ToString());

            UriGuidTimeSpanTestClass c2 = o.ToObject<UriGuidTimeSpanTestClass>();
            Assert.Equal(c1.Guid, c2.Guid);
            Assert.Equal(c1.NullableGuid, c2.NullableGuid);
            Assert.Equal(c1.TimeSpan, c2.TimeSpan);
            Assert.Equal(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.Equal(c1.Uri, c2.Uri);
        }

        [Fact]
        public void ParseWithPrecendingComments()
        {
            string json = @"/* blah */ {'hi':'hi!'}";
            JObject o = JObject.Parse(json);
            Assert.Equal("hi!", (string)o["hi"]);

            json = @"/* blah */ ['hi!']";
            JArray a = JArray.Parse(json);
            Assert.Equal("hi!", (string)a[0]);
        }

        [Fact]
        public void SerializeWithNoRedundentIdPropertiesTest()
        {
            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            Dictionary<string, object> dic3 = new Dictionary<string, object>();
            List<object> list1 = new List<object>();
            List<object> list2 = new List<object>();

            dic1.Add("list1", list1);
            dic1.Add("list2", list2);
            dic1.Add("dic1", dic1);
            dic1.Add("dic2", dic2);
            dic1.Add("dic3", dic3);
            dic1.Add("integer", 12345);

            list1.Add("A string!");
            list1.Add(dic1);
            list1.Add(new List<object>());

            dic3.Add("dic3", dic3);

            var json = SerializeWithNoRedundentIdProperties(dic1);

            Assert.Equal(@"{
  ""$id"": ""1"",
  ""list1"": [
    ""A string!"",
    {
      ""$ref"": ""1""
    },
    []
  ],
  ""list2"": [],
  ""dic1"": {
    ""$ref"": ""1""
  },
  ""dic2"": {},
  ""dic3"": {
    ""$id"": ""3"",
    ""dic3"": {
      ""$ref"": ""3""
    }
  },
  ""integer"": 12345
}", json);
        }

        private static string SerializeWithNoRedundentIdProperties(object o)
        {
            JTokenWriter writer = new JTokenWriter();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            serializer.Serialize(writer, o);

            JToken t = writer.Token;

            if (t is JContainer)
            {
                JContainer c = t as JContainer;

                // find all the $id properties in the JSON
                IList<JProperty> ids = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$id").ToList();

                if (ids.Count > 0)
                {
                    // find all the $ref properties in the JSON
                    IList<JProperty> refs = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$ref").ToList();

                    foreach (JProperty idProperty in ids)
                    {
                        // check whether the $id property is used by a $ref
                        bool idUsed = refs.Any(r => idProperty.Value.ToString() == r.Value.ToString());

                        if (!idUsed)
                        {
                            // remove unused $id
                            idProperty.Remove();
                        }
                    }
                }
            }

            string json = t.ToString();
            return json;
        }
    }
}