using System.Reflection;
using OpenGamingLibrary.Json.Utilities;
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
#if NET20
using OpenGamingLibrary.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace OpenGamingLibrary.Json.Test.Utilities
{
    public class OutAndRefTestClass
    {
        public string Input { get; set; }
        public bool B1 { get; set; }
        public bool B2 { get; set; }

        public OutAndRefTestClass(ref string value)
        {
            Input = value;
            value = "Output";
        }

        public OutAndRefTestClass(ref string value, out bool b1)
            : this(ref value)
        {
            b1 = true;
            B1 = true;
        }

        public OutAndRefTestClass(ref string value, ref bool b1, ref bool b2)
            : this(ref value)
        {
            B1 = b1;
            B2 = b2;
        }
    }

    
    public class LateboundReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Fact]
        public void ConstructorWithRefString()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input" };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
        }

        [Fact]
        public void ConstructorWithRefStringAndOutBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input", null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
        }

        [Fact]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 3);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParametrizedConstructor(constructor);

            object[] args = new object[] { "Input", true, null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.NotNull(o);
            Assert.Equal("Input", o.Input);
            Assert.Equal(true, o.B1);
            Assert.Equal(false, o.B2);
        }
    }
}