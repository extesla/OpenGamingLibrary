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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
#if NET40
using System.NET40;
#endif
using System.Web.UI;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Utilities;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JObjectTests : TestFixtureBase
    {
        [Fact]
        public void JObjectWithComments()
        {
            const string json = @"{ /*comment2*/
        ""Name"": /*comment3*/ ""Apple"" /*comment4*/, /*comment5*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment6*/ [ /*comment7*/
          ""Small"", /*comment8*/
          ""Medium"" /*comment9*/,
          /*comment10*/ ""Large""
        /*comment11*/ ] /*comment12*/
      } /*comment13*/";

            JToken o = JToken.Parse(json);

            Assert.Equal("Apple", (string) o["Name"]);
        }

        [Fact]
        public void WritePropertyWithNoValue()
        {
            var o = new JObject();
            o.Add(new JProperty("novalue"));

            StringAssert.Equal(@"{
  ""novalue"": null
}", o.ToString());
        }

        [Fact]
        public void Keys()
        {
            var o = new JObject();
            var d = (IDictionary<string, JToken>)o;

            Assert.Equal(0, d.Keys.Count);

            o["value"] = true;

            Assert.Equal(1, d.Keys.Count);
        }

        [Fact]
        public void TryGetValue()
        {
            var o = new JObject();
            o.Add("PropertyNameValue", new JValue(1));
            Assert.Equal(1, o.Children().Count());

            JToken t;
            Assert.Equal(false, o.TryGetValue("sdf", out t));
            Assert.Equal(null, t);

            Assert.Equal(false, o.TryGetValue(null, out t));
            Assert.Equal(null, t);

            Assert.Equal(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.Equal(true, JToken.DeepEquals(new JValue(1), t));
        }

        [Fact]
        public void DictionaryItemShouldSet()
        {
            var o = new JObject();
            o["PropertyNameValue"] = new JValue(1);
            Assert.Equal(1, o.Children().Count());

            JToken t;
            Assert.Equal(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.Equal(true, JToken.DeepEquals(new JValue(1), t));

            o["PropertyNameValue"] = new JValue(2);
            Assert.Equal(1, o.Children().Count());

            Assert.Equal(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.Equal(true, JToken.DeepEquals(new JValue(2), t));

            o["PropertyNameValue"] = null;
            Assert.Equal(1, o.Children().Count());

            Assert.Equal(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.Equal(true, JToken.DeepEquals(JValue.CreateNull(), t));
        }

        [Fact]
        public void Remove()
        {
            var o = new JObject();
            o.Add("PropertyNameValue", new JValue(1));
            Assert.Equal(1, o.Children().Count());

            Assert.Equal(false, o.Remove("sdf"));
            Assert.Equal(false, o.Remove(null));
            Assert.Equal(true, o.Remove("PropertyNameValue"));

            Assert.Equal(0, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionRemove()
        {
            JValue v = new JValue(1);
            var o = new JObject();
            o.Add("PropertyNameValue", v);
            Assert.Equal(1, o.Children().Count());

            Assert.Equal(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1))));
            Assert.Equal(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2))));
            Assert.Equal(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1))));
            Assert.Equal(true, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", v)));

            Assert.Equal(0, o.Children().Count());
        }

        [Fact]
        public void DuplicatePropertyNameShouldThrow()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                o.Add("PropertyNameValue", null);
                o.Add("PropertyNameValue", null);
            }, "Can not add property PropertyNameValue to OpenGamingLibrary.Json.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void GenericDictionaryAdd()
        {
            var o = new JObject();

            o.Add("PropertyNameValue", new JValue(1));
            Assert.Equal(1, (int)o["PropertyNameValue"]);

            o.Add("PropertyNameValue1", null);
            Assert.Equal(null, ((JValue)o["PropertyNameValue1"]).Value);

            Assert.Equal(2, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionAdd()
        {
            var o = new JObject();
            ((ICollection<KeyValuePair<string, JToken>>)o).Add(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));

            Assert.Equal(1, (int)o["PropertyNameValue"]);
            Assert.Equal(1, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionClear()
        {
            var o = new JObject();
            o.Add("PropertyNameValue", new JValue(1));
            Assert.Equal(1, o.Children().Count());

            JProperty p = (JProperty)o.Children().ElementAt(0);

            ((ICollection<KeyValuePair<string, JToken>>)o).Clear();
            Assert.Equal(0, o.Children().Count());

            Assert.Equal(null, p.Parent);
        }

        [Fact]
        public void GenericCollectionContains()
        {
            JValue v = new JValue(1);
            var o = new JObject();
            o.Add("PropertyNameValue", v);
            Assert.Equal(1, o.Children().Count());

            bool contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));
            Assert.Equal(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", v));
            Assert.Equal(true, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2)));
            Assert.Equal(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1)));
            Assert.Equal(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(default(KeyValuePair<string, JToken>));
            Assert.Equal(false, contains);
        }

        [Fact]
        public void GenericDictionaryContains()
        {
            var o = new JObject();
            o.Add("PropertyNameValue", new JValue(1));
            Assert.Equal(1, o.Children().Count());

            bool contains = ((IDictionary<string, JToken>)o).ContainsKey("PropertyNameValue");
            Assert.Equal(true, contains);
        }

        [Fact]
        public void GenericCollectionCopyTo()
        {
            var o = new JObject();
            o.Add("PropertyNameValue", new JValue(1));
            o.Add("PropertyNameValue2", new JValue(2));
            o.Add("PropertyNameValue3", new JValue(3));
            Assert.Equal(3, o.Children().Count());

            KeyValuePair<string, JToken>[] a = new KeyValuePair<string, JToken>[5];

            ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(a, 1);

            Assert.Equal(default(KeyValuePair<string, JToken>), a[0]);

            Assert.Equal("PropertyNameValue", a[1].Key);
            Assert.Equal(1, (int)a[1].Value);

            Assert.Equal("PropertyNameValue2", a[2].Key);
            Assert.Equal(2, (int)a[2].Value);

            Assert.Equal("PropertyNameValue3", a[3].Key);
            Assert.Equal(3, (int)a[3].Value);

            Assert.Equal(default(KeyValuePair<string, JToken>), a[4]);
        }

        [Fact]
        public void GenericCollectionCopyToNullArrayShouldThrow()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(null, 0);
            }, @"Value cannot be null.
Parameter name: array");
        }

        [Fact]
        public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
        {
            AssertException.Throws<ArgumentOutOfRangeException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], -1);
            }, @"arrayIndex is less than 0.
Parameter name: arrayIndex");
        }

        [Fact]
        public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], 1);
            }, @"arrayIndex is equal to or greater than the length of array.");
        }

        [Fact]
        public void GenericCollectionCopyToInsufficientArrayCapacity()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                o.Add("PropertyNameValue", new JValue(1));
                o.Add("PropertyNameValue2", new JValue(2));
                o.Add("PropertyNameValue3", new JValue(3));

                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[3], 1);
            }, @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
        }

        [Fact]
        public void FromObjectRaw()
        {
            PersonRaw raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            JObject o = JObject.FromObject(raw);

            Assert.Equal("FirstNameValue", (string)o["first_name"]);
            Assert.Equal(JTokenType.Raw, ((JValue)o["RawContent"]).Type);
            Assert.Equal("[1,2,3,4,5]", (string)o["RawContent"]);
            Assert.Equal("LastNameValue", (string)o["last_name"]);
        }

        [Fact]
        public void JTokenReader()
        {
            PersonRaw raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            JObject o = JObject.FromObject(raw);

            JsonReader reader = new JTokenReader(o);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.StartObject, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.Raw, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.PropertyName, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.String, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            Assert.False(reader.Read());
        }

        [Fact]
        public void DeserializeFromRaw()
        {
            PersonRaw raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            JObject o = JObject.FromObject(raw);

            JsonReader reader = new JTokenReader(o);
            JsonSerializer serializer = new JsonSerializer();
            raw = (PersonRaw)serializer.Deserialize(reader, typeof(PersonRaw));

            Assert.Equal("FirstNameValue", raw.FirstName);
            Assert.Equal("LastNameValue", raw.LastName);
            Assert.Equal("[1,2,3,4,5]", raw.RawContent.Value);
        }

        [Fact]
        public void Parse_ShouldThrowOnUnexpectedToken()
        {
            AssertException.Throws<JsonReaderException>(() =>
            {
                const string json = @"[""prop""]";
                JObject.Parse(json);
            }, "Error reading JObject from JsonReader. Current JsonReader item is not an object: StartArray. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseJavaScriptDate()
        {
            const string json = @"[new Date(1207285200000)]";

            JArray a = (JArray)JsonConvert.DeserializeObject(json);
            JValue v = (JValue)a[0];

            Assert.Equal(DateTimeUtils.ConvertJavaScriptTicksToDateTime(1207285200000), (DateTime)v);
        }

        [Fact]
        public void GenericValueCast()
        {
            string json = @"{""foo"":true}";
			var o = JsonConvert.DeserializeObject(json) as JObject;
            bool? value = o.Value<bool?>("foo");
            Assert.Equal(true, value);

            json = @"{""foo"":null}";
            o = (JObject)JsonConvert.DeserializeObject(json);
            value = o.Value<bool?>("foo");
            Assert.Equal(null, value);
        }

        [Fact]
        public void Blog()
        {
            AssertException.Throws<JsonReaderException>(() => { JObject.Parse(@"{
    ""name"": ""James"",
    ]!#$THIS IS: BAD JSON![{}}}}]
  }"); }, "Invalid property identifier character: ]. Path 'name', line 3, position 5.");
        }

        [Fact]
        public void RawChildValues()
        {
            var o = new JObject();
            o["val1"] = new JRaw("1");
            o["val2"] = new JRaw("1");

            string json = o.ToString();

            StringAssert.Equal(@"{
  ""val1"": 1,
  ""val2"": 1
}", json);
        }

        [Fact]
        public void Iterate()
        {
            var o = new JObject();
            o.Add("PropertyNameValue1", new JValue(1));
            o.Add("PropertyNameValue2", new JValue(2));

            JToken t = o;

            int i = 1;
            foreach (JProperty property in t)
            {
                Assert.Equal("PropertyNameValue" + i, property.Name);
                Assert.Equal(i, (int)property.Value);

                i++;
            }
        }

        [Fact]
        public void KeyValuePairIterate()
        {
            var o = new JObject();
            o.Add("PropertyNameValue1", new JValue(1));
            o.Add("PropertyNameValue2", new JValue(2));

            int i = 1;
            foreach (KeyValuePair<string, JToken> pair in o)
            {
                Assert.Equal("PropertyNameValue" + i, pair.Key);
                Assert.Equal(i, (int)pair.Value);

                i++;
            }
        }

        [Fact]
        public void WriteObjectNullStringValue()
        {
            string s = null;
            JValue v = new JValue(s);
            Assert.Equal(null, v.Value);
            Assert.Equal(JTokenType.String, v.Type);

            var o = new JObject();
            o["title"] = v;

            string output = o.ToString();

            StringAssert.Equal(@"{
  ""title"": null
}", output);
        }

        [Fact]
        public void Example()
        {
            const string json = @"{
        ""Name"": ""Apple"",
        ""Expiry"": new Date(1230422400000),
        ""Price"": 3.99,
        ""Sizes"": [
          ""Small"",
          ""Medium"",
          ""Large""
        ]
      }";

            JObject o = JObject.Parse(json);

            string name = (string)o["Name"];
            // Apple

            JArray sizes = (JArray)o["Sizes"];

            string smallest = (string)sizes[0];
            // Small

            Console.WriteLine(name);
            Console.WriteLine(smallest);
        }

        [Fact]
        public void DeserializeClassManually()
        {
            const string jsonText = @"{
  ""short"":
  {
    ""original"":""http://www.foo.com/"",
    ""short"":""krehqk"",
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

            JObject json = JObject.Parse(jsonText);

            Shortie shortie = new Shortie
            {
                Original = (string)json["short"]["original"],
                Short = (string)json["short"]["short"],
                Error = new ShortieException
                {
                    Code = (int)json["short"]["error"]["code"],
                    ErrorMessage = (string)json["short"]["error"]["msg"]
                }
            };

            Console.WriteLine(shortie.Original);
            // http://www.foo.com/

            Console.WriteLine(shortie.Error.ErrorMessage);
            // No action taken

            Assert.Equal("http://www.foo.com/", shortie.Original);
            Assert.Equal("krehqk", shortie.Short);
            Assert.Equal(null, shortie.Shortened);
            Assert.Equal(0, shortie.Error.Code);
            Assert.Equal("No action taken", shortie.Error.ErrorMessage);
        }

        [Fact]
        public void JObjectContainingHtml()
        {
            var o = new JObject();
            o["rc"] = new JValue(200);
            o["m"] = new JValue("");
            o["o"] = new JValue(@"<div class='s1'>" + StringUtils.CarriageReturnLineFeed + @"</div>");

            StringAssert.Equal(@"{
  ""rc"": 200,
  ""m"": """",
  ""o"": ""<div class='s1'>\r\n</div>""
}", o.ToString());
        }

        [Fact]
        public void ImplicitValueConversions()
        {
            JObject moss = new JObject();
            moss["FirstName"] = new JValue("Maurice");
            moss["LastName"] = new JValue("Moss");
            moss["BirthDate"] = new JValue(new DateTime(1977, 12, 30));
            moss["Department"] = new JValue("IT");
            moss["JobTitle"] = new JValue("Support");

            Console.WriteLine(moss.ToString());
            //{
            //  "FirstName": "Maurice",
            //  "LastName": "Moss",
            //  "BirthDate": "\/Date(252241200000+1300)\/",
            //  "Department": "IT",
            //  "JobTitle": "Support"
            //}


            JObject jen = new JObject();
            jen["FirstName"] = "Jen";
            jen["LastName"] = "Barber";
            jen["BirthDate"] = new DateTime(1978, 3, 15);
            jen["Department"] = "IT";
            jen["JobTitle"] = "Manager";

            Console.WriteLine(jen.ToString());
            //{
            //  "FirstName": "Jen",
            //  "LastName": "Barber",
            //  "BirthDate": "\/Date(258721200000+1300)\/",
            //  "Department": "IT",
            //  "JobTitle": "Manager"
            //}
        }

        [Fact]
        public void ReplaceJPropertyWithJPropertyWithSameName()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");

            var o = new JObject(p1, p2);
            IList l = o;
            Assert.Equal(p1, l[0]);
            Assert.Equal(p2, l[1]);

            JProperty p3 = new JProperty("Test1", "III");

            p1.Replace(p3);
            Assert.Equal(null, p1.Parent);
            Assert.Equal(l, p3.Parent);

            Assert.Equal(p3, l[0]);
            Assert.Equal(p2, l[1]);

            Assert.Equal(2, l.Count);
            Assert.Equal(2, o.Properties().Count());

            JProperty p4 = new JProperty("Test4", "IV");

            p2.Replace(p4);
            Assert.Equal(null, p2.Parent);
            Assert.Equal(l, p4.Parent);

            Assert.Equal(p3, l[0]);
            Assert.Equal(p4, l[1]);
        }

#if !(NET20 || NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void PropertyChanging()
        {
            object changing = null;
            object changed = null;
            int changingCount = 0;
            int changedCount = 0;

            var o = new JObject();
            o.PropertyChanging += (sender, args) =>
            {
                JObject s = (JObject)sender;
                changing = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
                changingCount++;
            };
            o.PropertyChanged += (sender, args) =>
            {
                JObject s = (JObject)sender;
                changed = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
                changedCount++;
            };

            o["StringValue"] = "value1";
            Assert.Equal(null, changing);
            Assert.Equal("value1", changed);
            Assert.Equal("value1", (string)o["StringValue"]);
            Assert.Equal(1, changingCount);
            Assert.Equal(1, changedCount);

            o["StringValue"] = "value1";
            Assert.Equal(1, changingCount);
            Assert.Equal(1, changedCount);

            o["StringValue"] = "value2";
            Assert.Equal("value1", changing);
            Assert.Equal("value2", changed);
            Assert.Equal("value2", (string)o["StringValue"]);
            Assert.Equal(2, changingCount);
            Assert.Equal(2, changedCount);

            o["StringValue"] = null;
            Assert.Equal("value2", changing);
            Assert.Equal(null, changed);
            Assert.Equal(null, (string)o["StringValue"]);
            Assert.Equal(3, changingCount);
            Assert.Equal(3, changedCount);

            o["NullValue"] = null;
            Assert.Equal(null, changing);
            Assert.Equal(null, changed);
            Assert.Equal(JValue.CreateNull(), o["NullValue"]);
            Assert.Equal(4, changingCount);
            Assert.Equal(4, changedCount);

            o["NullValue"] = null;
            Assert.Equal(4, changingCount);
            Assert.Equal(4, changedCount);
        }
#endif

        [Fact]
        public void PropertyChanged()
        {
            object changed = null;
            int changedCount = 0;

            var o = new JObject();
            o.PropertyChanged += (sender, args) =>
            {
                JObject s = (JObject)sender;
                changed = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
                changedCount++;
            };

            o["StringValue"] = "value1";
            Assert.Equal("value1", changed);
            Assert.Equal("value1", (string)o["StringValue"]);
            Assert.Equal(1, changedCount);

            o["StringValue"] = "value1";
            Assert.Equal(1, changedCount);

            o["StringValue"] = "value2";
            Assert.Equal("value2", changed);
            Assert.Equal("value2", (string)o["StringValue"]);
            Assert.Equal(2, changedCount);

            o["StringValue"] = null;
            Assert.Equal(null, changed);
            Assert.Equal(null, (string)o["StringValue"]);
            Assert.Equal(3, changedCount);

            o["NullValue"] = null;
            Assert.Equal(null, changed);
            Assert.Equal(JValue.CreateNull(), o["NullValue"]);
            Assert.Equal(4, changedCount);

            o["NullValue"] = null;
            Assert.Equal(4, changedCount);
        }

        [Fact]
        public void IListContains()
        {
            JProperty p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.True(l.Contains(p));
            Assert.False(l.Contains(new JProperty("Test", 1)));
        }

        [Fact]
        public void IListIndexOf()
        {
            JProperty p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.Equal(0, l.IndexOf(p));
            Assert.Equal(-1, l.IndexOf(new JProperty("Test", 1)));
        }

        [Fact]
        public void IListClear()
        {
            JProperty p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.Equal(1, l.Count);

            l.Clear();

            Assert.Equal(0, l.Count);
        }

        [Fact]
        public void IListCopyTo()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            object[] a = new object[l.Count];

            l.CopyTo(a, 0);

            Assert.Equal(p1, a[0]);
            Assert.Equal(p2, a[1]);
        }

        [Fact]
        public void IListAdd()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l.Add(p3);

            Assert.Equal(3, l.Count);
            Assert.Equal(p3, l[2]);
        }

        [Fact]
        public void IListAddBadToken()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l.Add(new JValue("Bad!"));
            }, "Can not add OpenGamingLibrary.Json.Linq.JValue to OpenGamingLibrary.Json.Linq.JObject.");
        }

        [Fact]
        public void IListAddBadValue()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l.Add("Bad!");
            }, "Argument is not a JToken.");
        }

        [Fact]
        public void IListAddPropertyWithExistingName()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                JProperty p3 = new JProperty("Test2", "II");

                l.Add(p3);
            }, "Can not add property Test2 to OpenGamingLibrary.Json.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IListRemove()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            // won't do anything
            l.Remove(p3);
            Assert.Equal(2, l.Count);

            l.Remove(p1);
            Assert.Equal(1, l.Count);
            Assert.False(l.Contains(p1));
            Assert.True(l.Contains(p2));

            l.Remove(p2);
            Assert.Equal(0, l.Count);
            Assert.False(l.Contains(p2));
            Assert.Equal(null, p2.Parent);
        }

        [Fact]
        public void IListRemoveAt()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            // won't do anything
            l.RemoveAt(0);

            l.Remove(p1);
            Assert.Equal(1, l.Count);

            l.Remove(p2);
            Assert.Equal(0, l.Count);
        }

        [Fact]
        public void IListInsert()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l.Insert(1, p3);
            Assert.Equal(l, p3.Parent);

            Assert.Equal(p1, l[0]);
            Assert.Equal(p3, l[1]);
            Assert.Equal(p2, l[2]);
        }

        [Fact]
        public void IListIsReadOnly()
        {
            IList l = new JObject();
            Assert.False(l.IsReadOnly);
        }

        [Fact]
        public void IListIsFixedSize()
        {
            IList l = new JObject();
            Assert.False(l.IsFixedSize);
        }

        [Fact]
        public void IListSetItem()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l[0] = p3;

            Assert.Equal(p3, l[0]);
            Assert.Equal(p2, l[1]);
        }

        [Fact]
        public void IListSetItemAlreadyExists()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                JProperty p3 = new JProperty("Test3", "III");

                l[0] = p3;
                l[1] = p3;
            }, "Can not add property Test3 to OpenGamingLibrary.Json.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IListSetItemInvalid()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l[0] = new JValue(true);
            }, @"Can not add OpenGamingLibrary.Json.Linq.JValue to OpenGamingLibrary.Json.Linq.JObject.");
        }

        [Fact]
        public void IListSyncRoot()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            Assert.NotNull(l.SyncRoot);
        }

        [Fact]
        public void IListIsSynchronized()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            Assert.False(l.IsSynchronized);
        }

        [Fact]
        public void GenericListJTokenContains()
        {
            JProperty p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.True(l.Contains(p));
            Assert.False(l.Contains(new JProperty("Test", 1)));
        }

        [Fact]
        public void GenericListJTokenIndexOf()
        {
            JProperty p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.Equal(0, l.IndexOf(p));
            Assert.Equal(-1, l.IndexOf(new JProperty("Test", 1)));
        }

        [Fact]
        public void GenericListJTokenClear()
        {
            JProperty p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.Equal(1, l.Count);

            l.Clear();

            Assert.Equal(0, l.Count);
        }

        [Fact]
        public void GenericListJTokenCopyTo()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            JToken[] a = new JToken[l.Count];

            l.CopyTo(a, 0);

            Assert.Equal(p1, a[0]);
            Assert.Equal(p2, a[1]);
        }

        [Fact]
        public void GenericListJTokenAdd()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l.Add(p3);

            Assert.Equal(3, l.Count);
            Assert.Equal(p3, l[2]);
        }

        [Fact]
        public void GenericListJTokenAddBadToken()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                l.Add(new JValue("Bad!"));
            }, "Can not add OpenGamingLibrary.Json.Linq.JValue to OpenGamingLibrary.Json.Linq.JObject.");
        }

        [Fact]
        public void GenericListJTokenAddBadValue()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                // string is implicitly converted to JValue
                l.Add("Bad!");
            }, "Can not add OpenGamingLibrary.Json.Linq.JValue to OpenGamingLibrary.Json.Linq.JObject.");
        }

        [Fact]
        public void GenericListJTokenAddPropertyWithExistingName()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                JProperty p3 = new JProperty("Test2", "II");

                l.Add(p3);
            }, "Can not add property Test2 to OpenGamingLibrary.Json.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void GenericListJTokenRemove()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            // won't do anything
            Assert.False(l.Remove(p3));
            Assert.Equal(2, l.Count);

            Assert.True(l.Remove(p1));
            Assert.Equal(1, l.Count);
            Assert.False(l.Contains(p1));
            Assert.True(l.Contains(p2));

            Assert.True(l.Remove(p2));
            Assert.Equal(0, l.Count);
            Assert.False(l.Contains(p2));
            Assert.Equal(null, p2.Parent);
        }

        [Fact]
        public void GenericListJTokenRemoveAt()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            // won't do anything
            l.RemoveAt(0);

            l.Remove(p1);
            Assert.Equal(1, l.Count);

            l.Remove(p2);
            Assert.Equal(0, l.Count);
        }

        [Fact]
        public void GenericListJTokenInsert()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l.Insert(1, p3);
            Assert.Equal(l, p3.Parent);

            Assert.Equal(p1, l[0]);
            Assert.Equal(p3, l[1]);
            Assert.Equal(p2, l[2]);
        }

        [Fact]
        public void GenericListJTokenIsReadOnly()
        {
            IList<JToken> l = new JObject();
            Assert.False(l.IsReadOnly);
        }

        [Fact]
        public void GenericListJTokenSetItem()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            JProperty p3 = new JProperty("Test3", "III");

            l[0] = p3;

            Assert.Equal(p3, l[0]);
            Assert.Equal(p2, l[1]);
        }

        [Fact]
        public void GenericListJTokenSetItemAlreadyExists()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JProperty p1 = new JProperty("Test1", 1);
                JProperty p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                JProperty p3 = new JProperty("Test3", "III");

                l[0] = p3;
                l[1] = p3;
            }, "Can not add property Test3 to OpenGamingLibrary.Json.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IBindingListSortDirection()
        {
            IBindingList l = new JObject();
            Assert.Equal(ListSortDirection.Ascending, l.SortDirection);
        }

        [Fact]
        public void IBindingListSortProperty()
        {
            IBindingList l = new JObject();
            Assert.Equal(null, l.SortProperty);
        }

        [Fact]
        public void IBindingListSupportsChangeNotification()
        {
            IBindingList l = new JObject();
            Assert.Equal(true, l.SupportsChangeNotification);
        }

        [Fact]
        public void IBindingListSupportsSearching()
        {
            IBindingList l = new JObject();
            Assert.Equal(false, l.SupportsSearching);
        }

        [Fact]
        public void IBindingListSupportsSorting()
        {
            IBindingList l = new JObject();
            Assert.Equal(false, l.SupportsSorting);
        }

        [Fact]
        public void IBindingListAllowEdit()
        {
            IBindingList l = new JObject();
            Assert.Equal(true, l.AllowEdit);
        }

        [Fact]
        public void IBindingListAllowNew()
        {
            IBindingList l = new JObject();
            Assert.Equal(true, l.AllowNew);
        }

        [Fact]
        public void IBindingListAllowRemove()
        {
            IBindingList l = new JObject();
            Assert.Equal(true, l.AllowRemove);
        }

        [Fact]
        public void IBindingListAddIndex()
        {
            IBindingList l = new JObject();
            // do nothing
            l.AddIndex(null);
        }

        [Fact]
        public void IBindingListApplySort()
        {
			AssertException.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.ApplySort(null, ListSortDirection.Ascending);
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListRemoveSort()
        {
            AssertException.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.RemoveSort();
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListRemoveIndex()
        {
            IBindingList l = new JObject();
            // do nothing
            l.RemoveIndex(null);
        }

        [Fact]
        public void IBindingListFind()
        {
			AssertException.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.Find(null, null);
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListIsSorted()
        {
            IBindingList l = new JObject();
            Assert.Equal(false, l.IsSorted);
        }

        [Fact]
        public void IBindingListAddNew()
        {
            AssertException.Throws<JsonException>(() =>
            {
                IBindingList l = new JObject();
                l.AddNew();
            }, "Could not determine new value to add to 'OpenGamingLibrary.Json.Linq.JObject'.");
        }

        [Fact]
        public void IBindingListAddNewWithEvent()
        {
            var o = new JObject();
            o._addingNew += (s, e) => e.NewObject = new JProperty("Property!");

            IBindingList l = o;
            object newObject = l.AddNew();
            Assert.NotNull(newObject);

            JProperty p = (JProperty)newObject;
            Assert.Equal("Property!", p.Name);
            Assert.Equal(o, p.Parent);
        }

        [Fact]
        public void ITypedListGetListName()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            ITypedList l = new JObject(p1, p2);

            Assert.Equal(string.Empty, l.GetListName(null));
        }

        [Fact]
        public void ITypedListGetItemProperties()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            ITypedList l = new JObject(p1, p2);

            PropertyDescriptorCollection propertyDescriptors = l.GetItemProperties(null);
            Assert.Null(propertyDescriptors);
        }

        [Fact]
        public void ListChanged()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            var o = new JObject(p1, p2);

            ListChangedType? changedType = null;
            int? index = null;

            o.ListChanged += (s, a) =>
            {
                changedType = a.ListChangedType;
                index = a.NewIndex;
            };

            JProperty p3 = new JProperty("Test3", "III");

            o.Add(p3);
            Assert.Equal(changedType, ListChangedType.ItemAdded);
            Assert.Equal(index, 2);
            Assert.Equal(p3, ((IList<JToken>)o)[index.Value]);

            JProperty p4 = new JProperty("Test4", "IV");

            ((IList<JToken>)o)[index.Value] = p4;
            Assert.Equal(changedType, ListChangedType.ItemChanged);
            Assert.Equal(index, 2);
            Assert.Equal(p4, ((IList<JToken>)o)[index.Value]);
            Assert.False(((IList<JToken>)o).Contains(p3));
            Assert.True(((IList<JToken>)o).Contains(p4));

            o["Test1"] = 2;
            Assert.Equal(changedType, ListChangedType.ItemChanged);
            Assert.Equal(index, 0);
            Assert.Equal(2, (int)o["Test1"]);
        }

        [Fact]
        public void GetGeocodeAddress()
        {
            const string json = @"{
  ""name"": ""Address: 435 North Mulford Road Rockford, IL 61107"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [ {
    ""id"": ""p1"",
    ""address"": ""435 N Mulford Rd, Rockford, IL 61107, USA"",
    ""AddressDetails"": {
   ""Accuracy"" : 8,
   ""Country"" : {
      ""AdministrativeArea"" : {
         ""AdministrativeAreaName"" : ""IL"",
         ""SubAdministrativeArea"" : {
            ""Locality"" : {
               ""LocalityName"" : ""Rockford"",
               ""PostalCode"" : {
                  ""PostalCodeNumber"" : ""61107""
               },
               ""Thoroughfare"" : {
                  ""ThoroughfareName"" : ""435 N Mulford Rd""
               }
            },
            ""SubAdministrativeAreaName"" : ""Winnebago""
         }
      },
      ""CountryName"" : ""USA"",
      ""CountryNameCode"" : ""US""
   }
},
    ""ExtendedData"": {
      ""LatLonBox"": {
        ""north"": 42.2753076,
        ""south"": 42.2690124,
        ""east"": -88.9964645,
        ""west"": -89.0027597
      }
    },
    ""Point"": {
      ""coordinates"": [ -88.9995886, 42.2721596, 0 ]
    }
  } ]
}";

            JObject o = JObject.Parse(json);

            string searchAddress = (string)o["Placemark"][0]["AddressDetails"]["Country"]["AdministrativeArea"]["SubAdministrativeArea"]["Locality"]["Thoroughfare"]["ThoroughfareName"];
            Assert.Equal("435 N Mulford Rd", searchAddress);
        }

        [Fact]
        public void SetValueWithInvalidPropertyName()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                o[0] = new JValue(3);
            }, "Set JObject values with invalid key value: 0. Object property name expected.");
        }

        [Fact]
        public void SetValue()
        {
            object key = "TestKey";

            var o = new JObject();
            o[key] = new JValue(3);

            Assert.Equal(3, (int)o[key]);
        }

        [Fact]
        public void ParseMultipleProperties()
        {
            const string json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

            JObject o = JObject.Parse(json);
            string value = (string)o["Name"];

            Assert.Equal("Name2", value);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void WriteObjectNullDBNullValue()
        {
            DBNull dbNull = DBNull.Value;
            JValue v = new JValue(dbNull);
            Assert.Equal(DBNull.Value, v.Value);
            Assert.Equal(JTokenType.Null, v.Type);

            var o = new JObject();
            o["title"] = v;

            string output = o.ToString();

            StringAssert.Equal(@"{
  ""title"": null
}", output);
        }
#endif

        [Fact]
        public void InvalidValueCastExceptionMessage()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                const string json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

                JObject o = JObject.Parse(json);

                string name = (string)o["responseData"];
            }, "Can not convert Object to String.");
        }

        [Fact]
        public void InvalidPropertyValueCastExceptionMessage()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                const string json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

                JObject o = JObject.Parse(json);

                string name = (string)o.Property("responseData");
            }, "Can not convert Object to String.");
        }

        [Fact]
        public void ParseIncomplete()
        {
            AssertException.Throws<Exception>(() => { JObject.Parse("{ foo:"); }, "Unexpected end of content while loading JObject. Path 'foo', line 1, position 6.");
        }

        [Fact]
        public void LoadFromNestedObject()
        {
            const string jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

            JsonReader reader = new JsonTextReader(new StringReader(jsonText));
            reader.Read();
            reader.Read();
            reader.Read();
            reader.Read();
            reader.Read();

            JObject o = (JObject)JToken.ReadFrom(reader);
            Assert.NotNull(o);
            StringAssert.Equal(@"{
  ""code"": 0,
  ""msg"": ""No action taken""
}", o.ToString(Formatting.Indented));
        }

        [Fact]
        public void LoadFromNestedObjectIncomplete()
        {
            AssertException.Throws<JsonReaderException>(() =>
            {
                const string jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0";

                JsonReader reader = new JsonTextReader(new StringReader(jsonText));
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();

                JToken.ReadFrom(reader);
            }, "Unexpected end of content while loading JObject. Path 'short.error.code', line 6, position 15.");
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void GetProperties()
        {
            JObject o = JObject.Parse("{'prop1':12,'prop2':'hi!','prop3':null,'prop4':[1,2,3]}");

            ICustomTypeDescriptor descriptor = o;

            PropertyDescriptorCollection properties = descriptor.GetProperties();
            Assert.Equal(4, properties.Count);

            PropertyDescriptor prop1 = properties[0];
            Assert.Equal("prop1", prop1.Name);
            Assert.Equal(typeof(object), prop1.PropertyType);
            Assert.Equal(typeof(JObject), prop1.ComponentType);
            Assert.Equal(false, prop1.CanResetValue(o));
            Assert.Equal(false, prop1.ShouldSerializeValue(o));

            PropertyDescriptor prop2 = properties[1];
            Assert.Equal("prop2", prop2.Name);
            Assert.Equal(typeof(object), prop2.PropertyType);
            Assert.Equal(typeof(JObject), prop2.ComponentType);
            Assert.Equal(false, prop2.CanResetValue(o));
            Assert.Equal(false, prop2.ShouldSerializeValue(o));

            PropertyDescriptor prop3 = properties[2];
            Assert.Equal("prop3", prop3.Name);
            Assert.Equal(typeof(object), prop3.PropertyType);
            Assert.Equal(typeof(JObject), prop3.ComponentType);
            Assert.Equal(false, prop3.CanResetValue(o));
            Assert.Equal(false, prop3.ShouldSerializeValue(o));

            PropertyDescriptor prop4 = properties[3];
            Assert.Equal("prop4", prop4.Name);
            Assert.Equal(typeof(object), prop4.PropertyType);
            Assert.Equal(typeof(JObject), prop4.ComponentType);
            Assert.Equal(false, prop4.CanResetValue(o));
            Assert.Equal(false, prop4.ShouldSerializeValue(o));
        }
#endif

        [Fact]
        public void ParseEmptyObjectWithComment()
        {
            JObject o = JObject.Parse("{ /* A Comment */ }");
            Assert.Equal(0, o.Count);
        }

        [Fact]
        public void FromObjectTimeSpan()
        {
            JValue v = (JValue)JToken.FromObject(TimeSpan.FromDays(1));
            Assert.Equal(v.Value, TimeSpan.FromDays(1));

            Assert.Equal("1.00:00:00", v.ToString());
        }

        [Fact]
        public void FromObjectUri()
        {
            JValue v = (JValue)JToken.FromObject(new Uri("http://www.stuff.co.nz"));
            Assert.Equal(v.Value, new Uri("http://www.stuff.co.nz"));

            Assert.Equal("http://www.stuff.co.nz/", v.ToString());
        }

        [Fact]
        public void FromObjectGuid()
        {
            JValue v = (JValue)JToken.FromObject(new Guid("9065ACF3-C820-467D-BE50-8D4664BEAF35"));
            Assert.Equal(v.Value, new Guid("9065ACF3-C820-467D-BE50-8D4664BEAF35"));

            Assert.Equal("9065acf3-c820-467d-be50-8d4664beaf35", v.ToString());
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            AssertException.Throws<JsonReaderException>(() =>
            {
                const string json = @"{
""Name"": ""Apple"",
""Expiry"": new Date(1230422400000),
""Price"": 3.99,
""Sizes"": [
""Small"",
""Medium"",
""Large""
]
}, 987987";

                JObject o = JObject.Parse(json);
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 10, position 2.");
        }

        [Fact]
        public void DeepEqualsIgnoreOrder()
        {
            JObject o1 = new JObject(
                new JProperty("null", null),
                new JProperty("integer", 1),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("array", new JArray(1, 2)));

            Assert.True(o1.DeepEquals(o1));

            JObject o2 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1),
                new JProperty("array", new JArray(1, 2)));

            Assert.True(o1.DeepEquals(o2));

            JObject o3 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 2),
                new JProperty("array", new JArray(1, 2)));

            Assert.False(o1.DeepEquals(o3));

            JObject o4 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1),
                new JProperty("array", new JArray(2, 1)));

            Assert.False(o1.DeepEquals(o4));

            JObject o5 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1));

            Assert.False(o1.DeepEquals(o5));

            Assert.False(o1.DeepEquals(null));
        }

        [Fact]
        public void ToListOnEmptyObject()
        {
            JObject o = JObject.Parse(@"{}");
            IList<JToken> l1 = o.ToList<JToken>();
            Assert.Equal(0, l1.Count);

            IList<KeyValuePair<string, JToken>> l2 = o.ToList<KeyValuePair<string, JToken>>();
            Assert.Equal(0, l2.Count);

            o = JObject.Parse(@"{'hi':null}");

            l1 = o.ToList<JToken>();
            Assert.Equal(1, l1.Count);

            l2 = o.ToList<KeyValuePair<string, JToken>>();
            Assert.Equal(1, l2.Count);
        }

        [Fact]
        public void EmptyObjectDeepEquals()
        {
            Assert.True(JToken.DeepEquals(new JObject(), new JObject()));

            JObject a = new JObject();
            JObject b = new JObject();

            b.Add("hi", "bye");
            b.Remove("hi");

            Assert.True(JToken.DeepEquals(a, b));
            Assert.True(JToken.DeepEquals(b, a));
        }

        [Fact]
        public void GetValueBlogExample()
        {
            JObject o = JObject.Parse(@"{
        'name': 'Lower',
        'NAME': 'Upper'
      }");

            string exactMatch = (string)o.GetValue("NAME", StringComparison.OrdinalIgnoreCase);
            // Upper

            string ignoreCase = (string)o.GetValue("Name", StringComparison.OrdinalIgnoreCase);
            // Lower

            Assert.Equal("Upper", exactMatch);
            Assert.Equal("Lower", ignoreCase);
        }

        [Fact]
        public void GetValue()
        {
            JObject a = new JObject();
            a["Name"] = "Name!";
            a["name"] = "name!";
            a["title"] = "Title!";

            Assert.Equal(null, a.GetValue("NAME", StringComparison.Ordinal));
            Assert.Equal(null, a.GetValue("NAME"));
            Assert.Equal(null, a.GetValue("TITLE"));
            Assert.Equal("Name!", (string)a.GetValue("NAME", StringComparison.OrdinalIgnoreCase));
            Assert.Equal("name!", (string)a.GetValue("name", StringComparison.Ordinal));
            Assert.Equal(null, a.GetValue(null, StringComparison.Ordinal));
            Assert.Equal(null, a.GetValue(null));

            JToken v;
            Assert.False(a.TryGetValue("NAME", StringComparison.Ordinal, out v));
            Assert.Equal(null, v);

            Assert.False(a.TryGetValue("NAME", out v));
            Assert.False(a.TryGetValue("TITLE", out v));

            Assert.True(a.TryGetValue("NAME", StringComparison.OrdinalIgnoreCase, out v));
            Assert.Equal("Name!", (string)v);

            Assert.True(a.TryGetValue("name", StringComparison.Ordinal, out v));
            Assert.Equal("name!", (string)v);

            Assert.False(a.TryGetValue(null, StringComparison.Ordinal, out v));
        }

        public class FooJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var token = JToken.FromObject(value, new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                if (token.Type == JTokenType.Object)
                {
                    var o = (JObject)token;
                    o.AddFirst(new JProperty("foo", "bar"));
                    o.WriteTo(writer);
                }
                else
                    token.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException("This custom converter only supportes serialization and not deserialization.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }

        [Fact]
        public void FromObjectInsideConverterWithCustomSerializer()
        {
            var p = new Person
            {
                Name = "Daniel Wertheim",
            };

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new FooJsonConverter() },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(p, settings);

            Assert.Equal(@"{""foo"":""bar"",""name"":""Daniel Wertheim"",""birthDate"":""0001-01-01T00:00:00"",""lastModified"":""0001-01-01T00:00:00""}", json);
        }
    }
}