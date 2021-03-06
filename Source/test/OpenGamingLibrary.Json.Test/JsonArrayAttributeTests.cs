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
using Xunit;

namespace OpenGamingLibrary.Json.Test
{
    
    public class JsonArrayAttributeTests : TestFixtureBase
    {
        [Fact]
        public void IsReferenceTest()
        {
            var attribute = new JsonPropertyAttribute();
            Assert.Equal(null, attribute._isReference);
            Assert.Equal(false, attribute.IsReference);

            attribute.IsReference = false;
            Assert.Equal(false, attribute._isReference);
            Assert.Equal(false, attribute.IsReference);

            attribute.IsReference = true;
            Assert.Equal(true, attribute._isReference);
            Assert.Equal(true, attribute.IsReference);
        }

        [Fact]
        public void NullValueHandlingTest()
        {
            var attribute = new JsonPropertyAttribute();
            Assert.Equal(null, attribute._nullValueHandling);
            Assert.Equal(NullValueHandling.Include, attribute.NullValueHandling);

            attribute.NullValueHandling = NullValueHandling.Ignore;
            Assert.Equal(NullValueHandling.Ignore, attribute._nullValueHandling);
            Assert.Equal(NullValueHandling.Ignore, attribute.NullValueHandling);
        }

        [Fact]
        public void DefaultValueHandlingTest()
        {
            var attribute = new JsonPropertyAttribute();
            Assert.Equal(null, attribute._defaultValueHandling);
            Assert.Equal(DefaultValueHandling.Include, attribute.DefaultValueHandling);

            attribute.DefaultValueHandling = DefaultValueHandling.Ignore;
            Assert.Equal(DefaultValueHandling.Ignore, attribute._defaultValueHandling);
            Assert.Equal(DefaultValueHandling.Ignore, attribute.DefaultValueHandling);
        }
    }
}