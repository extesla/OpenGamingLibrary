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
using System.ComponentModel;
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
using OpenGamingLibrary.Json.Linq;
using System.Linq;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JArrayTests : TestFixtureBase
    {
        [Fact]
        public void RemoveSpecificAndRemoveSelf()
        {
            JObject o = new JObject
            {
                { "results", new JArray(1, 2, 3, 4) }
            };

            JArray a = (JArray)o["results"];

            var last = a.Last();

            Assert.True(a.Remove(last));

            last = a.Last();
            last.Remove();

            Assert.Equal(2, a.Count);
        }

        [Fact]
        public void Clear()
        {
            JArray a = new JArray { 1 };
            Assert.Equal(1, a.Count);

            a.Clear();
            Assert.Equal(0, a.Count);
        }

        [Fact]
        public void AddToSelf()
        {
            JArray a = new JArray();
            a.Add(a);

            Assert.False(ReferenceEquals(a[0], a));
        }

        [Fact]
        public void Contains()
        {
            JValue v = new JValue(1);

            JArray a = new JArray { v };

            Assert.Equal(false, a.Contains(new JValue(2)));
            Assert.Equal(false, a.Contains(new JValue(1)));
            Assert.Equal(false, a.Contains(null));
            Assert.Equal(true, a.Contains(v));
        }

        [Fact]
        public void GenericCollectionCopyTo()
        {
            JArray j = new JArray();
            j.Add(new JValue(1));
            j.Add(new JValue(2));
            j.Add(new JValue(3));
            Assert.Equal(3, j.Count);

            JToken[] a = new JToken[5];

            ((ICollection<JToken>)j).CopyTo(a, 1);

            Assert.Equal(null, a[0]);

            Assert.Equal(1, (int)a[1]);

            Assert.Equal(2, (int)a[2]);

            Assert.Equal(3, (int)a[3]);

            Assert.Equal(null, a[4]);
        }

        [Fact]
        public void GenericCollectionCopyToNullArrayShouldThrow()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentNullException>(() => { ((ICollection<JToken>)j).CopyTo(null, 0); }, @"Value cannot be null.
Parameter name: array");
        }

        [Fact]
        public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentOutOfRangeException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], -1); }, @"arrayIndex is less than 0.
Parameter name: arrayIndex");
        }

        [Fact]
        public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], 1); }, @"arrayIndex is equal to or greater than the length of array.");
        }

        [Fact]
        public void GenericCollectionCopyToInsufficientArrayCapacity()
        {
            JArray j = new JArray();
            j.Add(new JValue(1));
            j.Add(new JValue(2));
            j.Add(new JValue(3));

            AssertException.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[3], 1); }, @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
        }

        [Fact]
        public void Remove()
        {
            JValue v = new JValue(1);
            JArray j = new JArray();
            j.Add(v);

            Assert.Equal(1, j.Count);

            Assert.Equal(false, j.Remove(new JValue(1)));
            Assert.Equal(false, j.Remove(null));
            Assert.Equal(true, j.Remove(v));
            Assert.Equal(false, j.Remove(v));

            Assert.Equal(0, j.Count);
        }

        [Fact]
        public void IndexOf()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(1);
            JValue v3 = new JValue(1);

            JArray j = new JArray();

            j.Add(v1);
            Assert.Equal(0, j.IndexOf(v1));

            j.Add(v2);
            Assert.Equal(0, j.IndexOf(v1));
            Assert.Equal(1, j.IndexOf(v2));

            j.AddFirst(v3);
            Assert.Equal(1, j.IndexOf(v1));
            Assert.Equal(2, j.IndexOf(v2));
            Assert.Equal(0, j.IndexOf(v3));

            v3.Remove();
            Assert.Equal(0, j.IndexOf(v1));
            Assert.Equal(1, j.IndexOf(v2));
            Assert.Equal(-1, j.IndexOf(v3));
        }

        [Fact]
        public void RemoveAt()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(1);
            JValue v3 = new JValue(1);

            JArray j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);

            Assert.Equal(true, j.Contains(v1));
            j.RemoveAt(0);
            Assert.Equal(false, j.Contains(v1));

            Assert.Equal(true, j.Contains(v3));
            j.RemoveAt(1);
            Assert.Equal(false, j.Contains(v3));

            Assert.Equal(1, j.Count);
        }

        [Fact]
        public void RemoveAtOutOfRangeIndexShouldError()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentOutOfRangeException>(() => { j.RemoveAt(0); }, @"Index is equal to or greater than Count.
Parameter name: index");
        }

        [Fact]
        public void RemoveAtNegativeIndexShouldError()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentOutOfRangeException>(() => { j.RemoveAt(-1); }, @"Index is less than 0.
Parameter name: index");
        }

        [Fact]
        public void Insert()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(2);
            JValue v3 = new JValue(3);
            JValue v4 = new JValue(4);

            JArray j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);
            j.Insert(1, v4);

            Assert.Equal(0, j.IndexOf(v1));
            Assert.Equal(1, j.IndexOf(v4));
            Assert.Equal(2, j.IndexOf(v2));
            Assert.Equal(3, j.IndexOf(v3));
        }

        [Fact]
        public void AddFirstAddedTokenShouldBeFirst()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(2);
            JValue v3 = new JValue(3);

            JArray j = new JArray();
            Assert.Equal(null, j.First);
            Assert.Equal(null, j.Last);

            j.AddFirst(v1);
            Assert.Equal(v1, j.First);
            Assert.Equal(v1, j.Last);

            j.AddFirst(v2);
            Assert.Equal(v2, j.First);
            Assert.Equal(v1, j.Last);

            j.AddFirst(v3);
            Assert.Equal(v3, j.First);
            Assert.Equal(v1, j.Last);
        }

        [Fact]
        public void InsertShouldInsertAtZeroIndex()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(2);

            JArray j = new JArray();

            j.Insert(0, v1);
            Assert.Equal(0, j.IndexOf(v1));

            j.Insert(0, v2);
            Assert.Equal(1, j.IndexOf(v1));
            Assert.Equal(0, j.IndexOf(v2));
        }

        [Fact]
        public void InsertNull()
        {
            JArray j = new JArray();
            j.Insert(0, null);

            Assert.Equal(null, ((JValue)j[0]).Value);
        }

        [Fact]
        public void InsertNegativeIndexShouldThrow()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentOutOfRangeException>(() => { j.Insert(-1, new JValue(1)); }, @"Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index");
        }

        [Fact]
        public void InsertOutOfRangeIndexShouldThrow()
        {
            JArray j = new JArray();

            AssertException.Throws<ArgumentOutOfRangeException>(() => { j.Insert(2, new JValue(1)); }, @"Index must be within the bounds of the List.
Parameter name: index");
        }

        [Fact]
        public void Item()
        {
            JValue v1 = new JValue(1);
            JValue v2 = new JValue(2);
            JValue v3 = new JValue(3);
            JValue v4 = new JValue(4);

            JArray j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);

            j[1] = v4;

            Assert.Equal(null, v2.Parent);
            Assert.Equal(-1, j.IndexOf(v2));
            Assert.Equal(j, v4.Parent);
            Assert.Equal(1, j.IndexOf(v4));
        }

        [Fact]
        public void Parse_ShouldThrowOnUnexpectedToken()
        {
            string json = @"{""prop"":""value""}";

            AssertException.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Error reading JArray from JsonReader. Current JsonReader item is not an array: StartObject. Path '', line 1, position 1.");
        }

        public class ListItemFields
        {
            public string ListItemText { get; set; }
            public object ListItemValue { get; set; }
        }

        [Fact]
        public void ArrayOrder()
        {
            string itemZeroText = "Zero text";

            IEnumerable<ListItemFields> t = new List<ListItemFields>
            {
                new ListItemFields { ListItemText = "First", ListItemValue = 1 },
                new ListItemFields { ListItemText = "Second", ListItemValue = 2 },
                new ListItemFields { ListItemText = "Third", ListItemValue = 3 }
            };

            JObject optionValues =
                new JObject(
                    new JProperty("options",
                        new JArray(
                            new JObject(
                                new JProperty("text", itemZeroText),
                                new JProperty("value", "0")),
                            from r in t
                            orderby r.ListItemValue
                            select new JObject(
                                new JProperty("text", r.ListItemText),
                                new JProperty("value", r.ListItemValue.ToString())))));

            string result = "myOptions = " + optionValues.ToString();

            StringAssert.Equal(@"myOptions = {
  ""options"": [
    {
      ""text"": ""Zero text"",
      ""value"": ""0""
    },
    {
      ""text"": ""First"",
      ""value"": ""1""
    },
    {
      ""text"": ""Second"",
      ""value"": ""2""
    },
    {
      ""text"": ""Third"",
      ""value"": ""3""
    }
  ]
}", result);
        }

        [Fact]
        public void Iterate()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            int i = 1;
            foreach (JToken token in a)
            {
                Assert.Equal(i, (int)token);
                i++;
            }
        }


#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void ITypedListGetItemProperties()
        {
            JProperty p1 = new JProperty("Test1", 1);
            JProperty p2 = new JProperty("Test2", "Two");
            ITypedList a = new JArray(new JObject(p1, p2));

            PropertyDescriptorCollection propertyDescriptors = a.GetItemProperties(null);
            Assert.NotNull(propertyDescriptors);
            Assert.Equal(2, propertyDescriptors.Count);
            Assert.Equal("Test1", propertyDescriptors[0].Name);
            Assert.Equal("Test2", propertyDescriptors[1].Name);
        }
#endif

        [Fact]
        public void AddArrayToSelf()
        {
            JArray a = new JArray(1, 2);
            a.Add(a);

            Assert.Equal(3, a.Count);
            Assert.Equal(1, (int)a[0]);
            Assert.Equal(2, (int)a[1]);
            Assert.NotSame(a, a[2]);
        }

        [Fact]
        public void SetValueWithInvalidIndex()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JArray a = new JArray();
                a["badvalue"] = new JValue(3);
            }, @"Set JArray values with invalid key value: ""badvalue"". Array position index expected.");
        }

        [Fact]
        public void SetValue()
        {
            object key = 0;

            JArray a = new JArray((object)null);
            a[key] = new JValue(3);

            Assert.Equal(3, (int)a[key]);
        }

        [Fact]
        public void ReplaceAll()
        {
            JArray a = new JArray(new[] { 1, 2, 3 });
            Assert.Equal(3, a.Count);
            Assert.Equal(1, (int)a[0]);
            Assert.Equal(2, (int)a[1]);
            Assert.Equal(3, (int)a[2]);

            a.ReplaceAll(1);
            Assert.Equal(1, a.Count);
            Assert.Equal(1, (int)a[0]);
        }

        [Fact]
        public void ParseIncomplete()
        {
            AssertException.Throws<JsonReaderException>(() => { JArray.Parse("[1"); }, "Unexpected end of content while loading JArray. Path '[0]', line 1, position 2.");
        }

        [Fact]
        public void InsertAddEnd()
        {
            JArray array = new JArray();
            array.Insert(0, 123);
            array.Insert(1, 456);

            Assert.Equal(2, array.Count);
            Assert.Equal(123, (int)array[0]);
            Assert.Equal(456, (int)array[1]);
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
], 987987";

            AssertException.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 2.");
        }

        [Fact]
        public void ToListOnEmptyArray()
        {
            string json = @"{""decks"":[]}";

            JArray decks = (JArray)JObject.Parse(json)["decks"];
            IList<JToken> l = decks.ToList();
            Assert.Equal(0, l.Count);

            json = @"{""decks"":[1]}";

            decks = (JArray)JObject.Parse(json)["decks"];
            l = decks.ToList();
            Assert.Equal(1, l.Count);
        }
    }
}