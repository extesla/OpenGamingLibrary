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
using Xunit;
using System.Runtime.Serialization;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class ReferenceLoopHandlingTests : TestFixtureBase
    {
        [Fact]
        public void ReferenceLoopHandlingTest()
        {
            JsonPropertyAttribute attribute = new JsonPropertyAttribute();
            Assert.Equal(null, attribute._defaultValueHandling);
            Assert.Equal(ReferenceLoopHandling.Error, attribute.ReferenceLoopHandling);

            attribute.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Assert.Equal(ReferenceLoopHandling.Ignore, attribute._referenceLoopHandling);
            Assert.Equal(ReferenceLoopHandling.Ignore, attribute.ReferenceLoopHandling);
        }

        [Fact]
        public void IgnoreObjectReferenceLoop()
        {
            ReferenceLoopHandlingObjectContainerAttribute o = new ReferenceLoopHandlingObjectContainerAttribute();
            o.Value = o;

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            Assert.Equal("{}", json);
        }

        [Fact]
        public void IgnoreObjectReferenceLoopWithPropertyOverride()
        {
            ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride o = new ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride();
            o.Value = o;

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            StringAssert.Equal(@"{
  ""Value"": {
    ""Value"": {
      ""Value"": {
        ""Value"": {
          ""Value"": {
            ""Value"": null
          }
        }
      }
    }
  }
}", json);
        }

        [Fact]
        public void IgnoreArrayReferenceLoop()
        {
            ReferenceLoopHandlingList a = new ReferenceLoopHandlingList();
            a.Add(a);

            string json = JsonConvert.SerializeObject(a, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            Assert.Equal("[]", json);
        }

        [Fact]
        public void IgnoreDictionaryReferenceLoop()
        {
            ReferenceLoopHandlingDictionary d = new ReferenceLoopHandlingDictionary();
            d.Add("First", d);

            string json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            Assert.Equal("{}", json);
        }

        [Fact]
        public void SerializePropertyItemReferenceLoopHandling()
        {
            PropertyItemReferenceLoopHandling c = new PropertyItemReferenceLoopHandling();
            c.Text = "Text!";
            c.SetData(new List<PropertyItemReferenceLoopHandling> { c });

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Text"": ""Text!"",
  ""Data"": [
    {
      ""Text"": ""Text!"",
      ""Data"": [
        {
          ""Text"": ""Text!"",
          ""Data"": [
            {
              ""Text"": ""Text!"",
              ""Data"": null
            }
          ]
        }
      ]
    }
  ]
}", json);
        }

#if !(PORTABLE || ASPNETCORE50 || NETFX_CORE || PORTABLE40)
        public class MainClass : ISerializable
        {
            public ChildClass Child { get; set; }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Child", Child);
            }
        }

        public class ChildClass : ISerializable
        {
            public string Name { get; set; }
            public MainClass Parent { get; set; }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Parent", Parent);
                info.AddValue("Name", Name);
            }
        }

        [Fact]
        public void ErrorISerializableCyclicReferenceLoop()
        {
            var main = new MainClass();
            var child = new ChildClass();

            child.Name = "Child1";
            child.Parent = main; // Obvious Circular Reference

            main.Child = child;

            var settings =
                new JsonSerializerSettings();

			AssertException.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(main, settings), "Self referencing loop detected with type 'OpenGamingLibrary.Json.Test.Serialization.ReferenceLoopHandlingTests+MainClass'. Path 'Child'.");
        }

        [Fact]
        public void IgnoreISerializableCyclicReferenceLoop()
        {
            var main = new MainClass();
            var child = new ChildClass();

            child.Name = "Child1";
            child.Parent = main; // Obvious Circular Reference

            main.Child = child;

            var settings =
                new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            var c = JsonConvert.SerializeObject(main, settings);
            Assert.Equal(@"{""Child"":{""Name"":""Child1""}}", c);
        }
#endif
    }

    public class PropertyItemReferenceLoopHandling
    {
        private IList<PropertyItemReferenceLoopHandling> _data;
        private int _accessCount;

        public string Text { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public IList<PropertyItemReferenceLoopHandling> Data
        {
            get
            {
                if (_accessCount >= 3)
                    return null;

                _accessCount++;
                return new List<PropertyItemReferenceLoopHandling>(_data);
            }
        }

        public void SetData(IList<PropertyItemReferenceLoopHandling> data)
        {
            _data = data;
        }
    }

    [JsonArray(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingList : List<ReferenceLoopHandlingList>
    {
    }

    [JsonDictionary(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingDictionary : Dictionary<string, ReferenceLoopHandlingDictionary>
    {
    }

    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingObjectContainerAttribute
    {
        public ReferenceLoopHandlingObjectContainerAttribute Value { get; set; }
    }

    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride
    {
        private ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride _value;
        private int _getCount;

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride Value
        {
            get
            {
                if (_getCount < 5)
                {
                    _getCount++;
                    return _value;
                }
                return null;
            }
            set { _value = value; }
        }
    }
}