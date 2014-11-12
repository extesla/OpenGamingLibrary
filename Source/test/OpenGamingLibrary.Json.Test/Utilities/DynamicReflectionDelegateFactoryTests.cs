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

#if !(PORTABLE || ASPNETCORE50 || NETFX_CORE || PORTABLE40)
using System;
using System.Diagnostics;
using System.Reflection;
using Xunit;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Utilities;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Json.Test.Serialization;
using OpenGamingLibrary.Xunit.Extensions;
using System.Linq;

namespace OpenGamingLibrary.Json.Test.Utilities
{
    
    public class DynamicReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Fact]
        public void ConstructorWithRefString()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 1);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input" };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
        }

        [Fact]
        public void ConstructorWithRefStringAndOutBool()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input", false };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
            Assert.Equal(true, o.B1);
        }

        [Fact]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 3);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input", true, null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
            Assert.Equal(true, o.B1);
            Assert.Equal(false, o.B2);
        }

        [Fact]
        public void CreateGetWithBadObjectTarget()
        {
            AssertException.Throws<InvalidCastException>(() =>
            {
                Person p = new Person();
                p.Name = "Hi";

                Func<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

                setter(p);
            }, "Unable to cast object of type 'OpenGamingLibrary.Json.Test.TestObjects.Person' to type 'OpenGamingLibrary.Json.Test.TestObjects.Movie'.");
        }

        [Fact]
        public void CreateSetWithBadObjectTarget()
        {
			AssertException.Throws<InvalidCastException>(() =>
            {
                Person p = new Person();
                Movie m = new Movie();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, "Hi");

                Assert.Equal(m.Name, "Hi");

                setter(p, "Hi");

                Assert.Equal(p.Name, "Hi");
            }, "Unable to cast object of type 'OpenGamingLibrary.Json.Test.TestObjects.Person' to type 'OpenGamingLibrary.Json.Test.TestObjects.Movie'.");
        }

        [Fact]
        public void CreateSetWithBadTarget()
        {
			AssertException.Throws<InvalidCastException>(() =>
            {
                object structTest = new StructTest();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

                setter(structTest, "Hi");

                Assert.Equal("Hi", ((StructTest)structTest).StringProperty);

                setter(new TimeSpan(), "Hi");
            }, "Specified cast is not valid.");
        }

        [Fact]
        public void CreateSetWithBadObjectValue()
        {
			AssertException.Throws<InvalidCastException>(() =>
            {
                Movie m = new Movie();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, new Version("1.1.1.1"));
            }, "Unable to cast object of type 'System.Version' to type 'System.String'.");
        }

        [Fact]
        public void CreateStaticMethodCall()
        {
            MethodInfo castMethodInfo = typeof(JsonSerializerTest.DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

            Assert.NotNull(castMethodInfo);

            MethodCall<object, object> call = DynamicReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

            object result = call(null, "First!");
            Assert.NotNull(result);

            JsonSerializerTest.DictionaryKey key = (JsonSerializerTest.DictionaryKey)result;
            Assert.Equal("First!", key.Value);
        }
    }
}

#endif