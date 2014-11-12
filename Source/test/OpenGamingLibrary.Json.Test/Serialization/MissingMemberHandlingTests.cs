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
using System.IO;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Test.TestObjects;
using Xunit;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class MissingMemberHandlingTests : TestFixtureBase
    {
        [Fact]
        public void MissingMemberDeserialize()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small", "Medium", "Large" };

            string output = JsonConvert.SerializeObject(product, Formatting.Indented);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": new Date(1230422400000),
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

			AssertException.Throws<JsonSerializationException>(() => { ProductShort deserializedProductShort = (ProductShort)JsonConvert.DeserializeObject(output, typeof(ProductShort), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }, @"Could not find member 'Price' on object of type 'ProductShort'. Path 'Price', line 4, position 11.");
        }

        [Fact]
        public void MissingMemberDeserializeOkay()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small", "Medium", "Large" };

            string output = JsonConvert.SerializeObject(product);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": new Date(1230422400000),
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.MissingMemberHandling = MissingMemberHandling.Ignore;

            object deserializedValue;

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(output)))
            {
                deserializedValue = jsonSerializer.Deserialize(jsonReader, typeof(ProductShort));
            }

            ProductShort deserializedProductShort = (ProductShort)deserializedValue;

            Assert.Equal("Apple", deserializedProductShort.Name);
            Assert.Equal(new DateTime(2008, 12, 28), deserializedProductShort.ExpiryDate);
            Assert.Equal("Small", deserializedProductShort.Sizes[0]);
            Assert.Equal("Medium", deserializedProductShort.Sizes[1]);
            Assert.Equal("Large", deserializedProductShort.Sizes[2]);
        }

        [Fact]
        public void MissingMemberIgnoreComplexValue()
        {
            JsonSerializer serializer = new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore };
            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            string response = @"{""PreProperty"":1,""DateProperty"":new Date(1225962698973),""PostProperty"":2}";

            MyClass myClass = (MyClass)serializer.Deserialize(new StringReader(response), typeof(MyClass));

            Assert.Equal(1, myClass.PreProperty);
            Assert.Equal(2, myClass.PostProperty);
        }

        [Fact]
        public void MissingMemeber()
        {
            string json = @"{""Missing"":1}";

			AssertException.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }, "Could not find member 'Missing' on object of type 'DoubleClass'. Path 'Missing', line 1, position 11.");
        }

        [Fact]
        public void MissingJson()
        {
            string json = @"{}";

            JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }
    }
}