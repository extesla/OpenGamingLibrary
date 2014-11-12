using System;
using System.Collections.Generic;
using System.Text;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Serialization;
using Xunit;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Converters
{
    
    public class KeyValuePairConverterTests : TestFixtureBase
    {
        [Fact]
        public void SerializeUsingInternalConverter()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver();
            JsonObjectContract contract = (JsonObjectContract) contractResolver.ResolveContract(typeof (KeyValuePair<string, int>));

            Assert.Equal(typeof(KeyValuePairConverter), contract.InternalConverter.GetType());

            IList<KeyValuePair<string, int>> values = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("123", 123),
                new KeyValuePair<string, int>("456", 456)
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            StringAssert.Equal(@"[
  {
    ""Key"": ""123"",
    ""Value"": 123
  },
  {
    ""Key"": ""456"",
    ""Value"": 456
  }
]", json);

            IList<KeyValuePair<string, int>> v2 = JsonConvert.DeserializeObject<IList<KeyValuePair<string, int>>>(json);

            Assert.Equal(2, v2.Count);
            Assert.Equal("123", v2[0].Key);
            Assert.Equal(123, v2[0].Value);
            Assert.Equal("456", v2[1].Key);
            Assert.Equal(456, v2[1].Value);
        }

        [Fact]
        public void DeserializeUnexpectedEnd()
        {
			AssertException.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<KeyValuePair<string, int>>(@"{""Key"": ""123"","), "Unexpected end when reading KeyValuePair. Path 'Key', line 1, position 14.");
        }
    }
}