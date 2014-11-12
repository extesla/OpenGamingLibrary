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
using OpenGamingLibrary.Json.Utilities;

namespace OpenGamingLibrary.Json.Test.Utilities
{
    
    public class StringUtilsTests : TestFixtureBase
    {
        [Fact]
        public void ToCamelCaseTest()
        {
            Assert.Equal("urlValue", StringUtils.ToCamelCase("URLValue"));
            Assert.Equal("url", StringUtils.ToCamelCase("URL"));
            Assert.Equal("id", StringUtils.ToCamelCase("ID"));
            Assert.Equal("i", StringUtils.ToCamelCase("I"));
            Assert.Equal("", StringUtils.ToCamelCase(""));
            Assert.Equal(null, StringUtils.ToCamelCase(null));
            Assert.Equal("iPhone", StringUtils.ToCamelCase("iPhone"));
            Assert.Equal("person", StringUtils.ToCamelCase("Person"));
            Assert.Equal("iPhone", StringUtils.ToCamelCase("IPhone"));
            Assert.Equal("i Phone", StringUtils.ToCamelCase("I Phone"));
            Assert.Equal(" IPhone", StringUtils.ToCamelCase(" IPhone"));
        }
    }
}