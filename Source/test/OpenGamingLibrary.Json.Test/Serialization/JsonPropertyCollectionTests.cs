﻿#region License
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
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    
    public class JsonPropertyCollectionTests : TestFixtureBase
    {
        [Fact]
        public void AddPropertyIncludesPrivateImplementations()
        {
            var value = new PrivateImplementationBClass
            {
                OverriddenProperty = "OverriddenProperty",
                PropertyA = "PropertyA",
                PropertyB = "PropertyB"
            };

            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(value.GetType());

            Assert.Equal(3, contract.Properties.Count);
            Assert.True(contract.Properties.Contains("OverriddenProperty"), "Contract is missing property 'OverriddenProperty'");
            Assert.True(contract.Properties.Contains("PropertyA"), "Contract is missing property 'PropertyA'");
            Assert.True(contract.Properties.Contains("PropertyB"), "Contract is missing property 'PropertyB'");
        }
    }
}