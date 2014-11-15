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
#if !(NET20 || NETFX_CORE || ASPNETCORE50)
using System.Data.Linq;
#endif
using System.Text;
using OpenGamingLibrary.Json.Converters;
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

namespace OpenGamingLibrary.Json.Test.Converters
{
    
    public class BinaryConverterTests : TestFixtureBase
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("This is some test data!!!");

        public class ByteArrayClass
        {
            public byte[] ByteArray { get; set; }
            public byte[] NullByteArray { get; set; }
        }

#if !(NET20 || NETFX_CORE || PORTABLE || PORTABLE40 || ASPNETCORE50)
        [Fact]
        public void DeserializeBinaryClass()
        {
            string json = @"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}";

            BinaryClass binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

            Assert.Equal(new Binary(TestData), binaryClass.Binary);
            Assert.Equal(null, binaryClass.NullBinary);
        }

        [Fact]
        public void DeserializeBinaryClassFromJsonArray()
        {
            string json = @"{
  ""Binary"": [0, 1, 2, 3],
  ""NullBinary"": null
}";

            BinaryClass binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

            Assert.Equal(new byte[] { 0, 1, 2, 3 }, binaryClass.Binary.ToArray());
            Assert.Equal(null, binaryClass.NullBinary);
        }

        public class BinaryClass
        {
            public Binary Binary { get; set; }
            public Binary NullBinary { get; set; }
        }

        [Fact]
        public void SerializeBinaryClass()
        {
            BinaryClass binaryClass = new BinaryClass();
            binaryClass.Binary = new Binary(TestData);
            binaryClass.NullBinary = null;

            string json = JsonConvert.SerializeObject(binaryClass, Formatting.Indented, new BinaryConverter());

            StringAssert.Equal(@"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}", json);
        }
#endif

        [Fact]
        public void SerializeByteArrayClass()
        {
            ByteArrayClass byteArrayClass = new ByteArrayClass();
            byteArrayClass.ByteArray = TestData;
            byteArrayClass.NullByteArray = null;

            string json = JsonConvert.SerializeObject(byteArrayClass, Formatting.Indented);

            StringAssert.Equal(@"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}", json);
        }

        [Fact]
        public void DeserializeByteArrayClass()
        {
            string json = @"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}";

            ByteArrayClass byteArrayClass = JsonConvert.DeserializeObject<ByteArrayClass>(json);

            Assert.Equal(TestData, byteArrayClass.ByteArray);
            Assert.Equal(null, byteArrayClass.NullByteArray);
        }

        [Fact]
        public void DeserializeByteArrayFromJsonArray()
        {
            string json = @"{
  ""ByteArray"": [0, 1, 2, 3],
  ""NullByteArray"": null
}";

            ByteArrayClass c = JsonConvert.DeserializeObject<ByteArrayClass>(json);
            Assert.NotNull(c.ByteArray);
            Assert.Equal(4, c.ByteArray.Length);
            Assert.Equal(new byte[] { 0, 1, 2, 3 }, c.ByteArray);
        }
    }
}