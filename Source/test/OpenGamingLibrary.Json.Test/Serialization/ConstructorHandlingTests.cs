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
using System.Reflection;
using OpenGamingLibrary.Json.Test.TestObjects;
using Xunit;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class ConstructorHandlingTests : TestFixtureBase
    {
        [Fact]
        public void UsePrivateConstructorIfThereAreMultipleConstructorsWithParametersAndNothingToFallbackTo()
        {
            string json = @"{Name:""Name!""}";

			PrivateConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json);

            Assert.Equal("Name!", c.Name);
        }

        [Fact]
        public void SuccessWithPrivateConstructorAndAllowNonPublic()
        {
            const string json = @"{Name:""Name!""}";
            var c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                });
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name);
        }

        [Fact]
        public void FailWithPrivateConstructorPlusParametizedAndDefault()
        {
            AssertException.Throws<Exception>(() =>
            {
                string json = @"{Name:""Name!""}";

                PrivateConstructorWithPublicParametizedConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParametizedConstructorTestClass>(json);
            });
        }

        [Fact]
        public void SuccessWithPrivateConstructorPlusParametizedAndAllowNonPublic()
        {
            string json = @"{Name:""Name!""}";

            PrivateConstructorWithPublicParametizedConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParametizedConstructorTestClass>(json,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                });
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name);
            Assert.Equal(1, c.Age);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructor()
        {
            string json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<PublicParametizedConstructorTestClass>(json);
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructorWhenParamaterIsNotAProperty()
        {
            string json = @"{nameParameter:""Name!""}";

            PublicParametizedConstructorWithNonPropertyParameterTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithNonPropertyParameterTestClass>(json);
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverter()
        {
            string json = @"{nameParameter:""Name!""}";

            PublicParametizedConstructorRequiringConverterTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterTestClass>(json, new NameContainerConverter());
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name.Value);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverterWithParameterAttribute()
        {
            string json = @"{nameParameter:""Name!""}";

            PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass>(json);
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name.Value);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverterWithPropertyAttribute()
        {
            string json = @"{name:""Name!""}";

            PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass>(json);
            Assert.NotNull(c);
            Assert.Equal("Name!", c.Name.Value);
        }

        [Fact]
        public void SuccessWithPublicParametizedConstructorWhenParamaterNameConflictsWithPropertyName()
        {
            string json = @"{name:""1""}";

            PublicParametizedConstructorWithPropertyNameConflict c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithPropertyNameConflict>(json);
            Assert.NotNull(c);
            Assert.Equal(1, c.Name);
        }

        [Fact]
        public void PublicParametizedConstructorWithPropertyNameConflictWithAttribute()
        {
            string json = @"{name:""1""}";

            PublicParametizedConstructorWithPropertyNameConflictWithAttribute c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithPropertyNameConflictWithAttribute>(json);
            Assert.NotNull(c);
            Assert.Equal(1, c.Name);
        }
    }
}