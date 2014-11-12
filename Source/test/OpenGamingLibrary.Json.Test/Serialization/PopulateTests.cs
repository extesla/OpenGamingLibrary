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
using OpenGamingLibrary.Json.Test.TestObjects;
using Xunit;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class PopulateTests : TestFixtureBase
    {
        [Fact]
        public void PopulatePerson()
        {
            Person p = new Person();

            JsonConvert.PopulateObject(@"{""Name"":""James""}", p);

            Assert.Equal("James", p.Name);
        }

        [Fact]
        public void PopulateArray()
        {
            IList<Person> people = new List<Person>
            {
                new Person { Name = "Initial" }
            };

            JsonConvert.PopulateObject(@"[{""Name"":""James""}, null]", people);

            Assert.Equal(3, people.Count);
            Assert.Equal("Initial", people[0].Name);
            Assert.Equal("James", people[1].Name);
            Assert.Equal(null, people[2]);
        }

        [Fact]
        public void PopulateStore()
        {
            Store s = new Store();
            s.Color = StoreColor.Red;
            s.product = new List<Product>
            {
                new Product
                {
                    ExpiryDate = new DateTime(2000, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                    Name = "ProductName!",
                    Price = 9.9m
                }
            };
            s.Width = 99.99d;
            s.Mottos = new List<string> { "Can do!", "We deliver!" };

            string json = @"{
  ""Color"": 2,
  ""Establised"": ""\/Date(1264122061000+0000)\/"",
  ""Width"": 99.99,
  ""Employees"": 999,
  ""RoomsPerFloor"": [
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9
  ],
  ""Open"": false,
  ""Symbol"": ""@"",
  ""Mottos"": [
    ""Fail whale""
  ],
  ""Cost"": 100980.1,
  ""Escape"": ""\r\n\t\f\b?{\\r\\n\""'"",
  ""product"": [
    {
      ""Name"": ""ProductName!"",
      ""ExpiryDate"": ""\/Date(975801600000)\/"",
      ""Price"": 9.9,
      ""Sizes"": null
    }
  ]
}";

            JsonConvert.PopulateObject(json, s, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });

            Assert.Equal(1, s.Mottos.Count);
            Assert.Equal("Fail whale", s.Mottos[0]);
            Assert.Equal(1, s.product.Count);

            //Assert.Equal("James", p.Name);
        }

        [Fact]
        public void PopulateListOfPeople()
        {
            List<Person> p = new List<Person>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Populate(new StringReader(@"[{""Name"":""James""},{""Name"":""Jim""}]"), p);

            Assert.Equal(2, p.Count);
            Assert.Equal("James", p[0].Name);
            Assert.Equal("Jim", p[1].Name);
        }

        [Fact]
        public void PopulateDictionary()
        {
            Dictionary<string, string> p = new Dictionary<string, string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Populate(new StringReader(@"{""Name"":""James""}"), p);

            Assert.Equal(1, p.Count);
            Assert.Equal("James", p["Name"]);
        }

        [Fact]
        public void PopulateWithBadJson()
        {
			AssertException.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject("1", new Person()); }, "Unexpected initial token 'Integer' when populating object. Expected JSON object or array. Path '', line 1, position 1.");
        }
    }
}