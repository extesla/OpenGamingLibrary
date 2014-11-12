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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JPropertyTests : TestFixtureBase
    {
        [Fact]
        public void NullValue()
        {
			var p = new JProperty("TestProperty", null);
            Assert.NotNull(p.Value);
            Assert.Equal(JTokenType.Null, p.Value.Type);
            Assert.Equal(p, p.Value.Parent);

            p.Value = null;
            Assert.NotNull(p.Value);
            Assert.Equal(JTokenType.Null, p.Value.Type);
            Assert.Equal(p, p.Value.Parent);
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void ListChanged()
        {
            var p = new JProperty("TestProperty", null);
            IBindingList l = p;

            ListChangedType? listChangedType = null;
            int? index = null;

            l.ListChanged += (sender, args) =>
            {
                listChangedType = args.ListChangedType;
                index = args.NewIndex;
            };

            p.Value = 1;

            Assert.Equal(ListChangedType.ItemChanged, listChangedType.Value);
            Assert.Equal(0, index.Value);
        }
#endif

        [Fact]
        public void IListCount()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            Assert.Equal(1, l.Count);
        }

        [Fact]
        public void IListClear()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            AssertException.Throws<JsonException>(() => { l.Clear(); }, "Cannot add or remove items from OpenGamingLibrary.Json.Linq.JProperty.");
        }

        [Fact]
        public void IListAdd()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            AssertException.Throws<JsonException>(() => { l.Add(null); }, "OpenGamingLibrary.Json.Linq.JProperty cannot have multiple values.");
        }

        [Fact]
        public void IListRemove()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            AssertException.Throws<JsonException>(() => { l.Remove(p.Value); }, "Cannot add or remove items from OpenGamingLibrary.Json.Linq.JProperty.");
        }

        [Fact]
        public void IListRemoveAt()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            AssertException.Throws<JsonException>(() => { l.RemoveAt(0); }, "Cannot add or remove items from OpenGamingLibrary.Json.Linq.JProperty.");
        }

        [Fact]
        public void JPropertyLinq()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            List<JToken> result = l.Cast<JToken>().ToList();
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void JPropertyDeepEquals()
        {
            JProperty p1 = new JProperty("TestProperty", null);
            JProperty p2 = new JProperty("TestProperty", null);

            Assert.Equal(true, JToken.DeepEquals(p1, p2));
        }

        [Fact]
        public void JPropertyIndexOf()
        {
            JValue v = new JValue(1);
            JProperty p1 = new JProperty("TestProperty", v);

            IList l1 = p1;
            Assert.Equal(0, l1.IndexOf(v));

            IList<JToken> l2 = p1;
            Assert.Equal(0, l2.IndexOf(v));
        }

        [Fact]
        public void JPropertyContains()
        {
            JValue v = new JValue(1);
            var p = new JProperty("TestProperty", v);

            Assert.Equal(true, p.Contains(v));
            Assert.Equal(false, p.Contains(new JValue(1)));
        }

        [Fact]
        public void Load()
        {
            JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
            reader.Read();

            Assert.Equal(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            JProperty property = JProperty.Load(reader);
            Assert.Equal("propertyname", property.Name);
            Assert.True(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

            Assert.Equal(JsonToken.EndObject, reader.TokenType);

            reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
            reader.Read();

            Assert.Equal(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            property = JProperty.Load(reader);
            Assert.Equal("propertyname", property.Name);
            Assert.True(JToken.DeepEquals(JValue.CreateNull(), property.Value));

            Assert.Equal(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void MultiContentConstructor()
        {
            var p = new JProperty("error", new List<string> { "one", "two" });
            JArray a = (JArray)p.Value;

            Assert.Equal(a.Count, 2);
            Assert.Equal("one", (string)a[0]);
            Assert.Equal("two", (string)a[1]);
        }

        [Fact]
        public void IListGenericAdd()
        {
            IList<JToken> t = new JProperty("error", new List<string> { "one", "two" });

            AssertException.Throws<JsonException>(() => { t.Add(1); }, "OpenGamingLibrary.Json.Linq.JProperty cannot have multiple values.");
        }
    }
}