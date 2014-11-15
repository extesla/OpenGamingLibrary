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
using System.Collections.Generic;
#if NET40
using System.Numerics;
#endif
using System.Text;
using OpenGamingLibrary.Json.Converters;
using Xunit;
using OpenGamingLibrary.Json.Linq;
using System.IO;
using System.Linq;
using OpenGamingLibrary.Json.Utilities;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Linq
{
    
    public class JTokenTests : TestFixtureBase
    {
        [Fact]
        public void ReadFrom()
        {
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(new StringReader("{'pie':true}")));
            Assert.Equal(true, (bool)o["pie"]);

            JArray a = (JArray)JToken.ReadFrom(new JsonTextReader(new StringReader("[1,2,3]")));
            Assert.Equal(1, (int)a[0]);
            Assert.Equal(2, (int)a[1]);
            Assert.Equal(3, (int)a[2]);

            JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
            reader.Read();
            reader.Read();

            JProperty p = (JProperty)JToken.ReadFrom(reader);
            Assert.Equal("pie", p.Name);
            Assert.Equal(true, (bool)p.Value);

            JConstructor c = (JConstructor)JToken.ReadFrom(new JsonTextReader(new StringReader("new Date(1)")));
            Assert.Equal("Date", c.Name);
            Assert.True(JToken.DeepEquals(new JValue(1), c.Values().ElementAt(0)));

            JValue v;

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""stringvalue""")));
            Assert.Equal("stringvalue", (string)v);

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1")));
            Assert.Equal(1, (int)v);

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1.1")));
            Assert.Equal(1.1, (double)v);

#if !NET20
            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""1970-01-01T00:00:00+12:31"""))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            });
            Assert.Equal(typeof(DateTimeOffset), v.Value.GetType());
            Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, new TimeSpan(12, 31, 0)), v.Value);
#endif
        }

        [Fact]
        public void Load()
        {
            JObject o = (JObject)JToken.Load(new JsonTextReader(new StringReader("{'pie':true}")));
            Assert.Equal(true, (bool)o["pie"]);
        }

        [Fact]
        public void Parse()
        {
            JObject o = (JObject)JToken.Parse("{'pie':true}");
            Assert.Equal(true, (bool)o["pie"]);
        }

        [Fact]
        public void Parent()
        {
            JArray v = new JArray(new JConstructor("TestConstructor"), new JValue(new DateTime(2000, 12, 20)));

            Assert.Equal(null, v.Parent);

            JObject o =
                new JObject(
                    new JProperty("Test1", v),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            Assert.Equal(o.Property("Test1"), v.Parent);

            JProperty p = new JProperty("NewProperty", v);

            // existing value should still have same parent
            Assert.Equal(o.Property("Test1"), v.Parent);

            // new value should be cloned
            Assert.NotSame(p.Value, v);

            Assert.Equal((DateTime)((JValue)p.Value[1]).Value, (DateTime)((JValue)v[1]).Value);

            Assert.Equal(v, o["Test1"]);

            Assert.Equal(null, o.Parent);
            JProperty o1 = new JProperty("O1", o);
            Assert.Equal(o, o1.Value);

            Assert.NotEqual(null, o.Parent);
            JProperty o2 = new JProperty("O2", o);

            Assert.NotSame(o1.Value, o2.Value);
            Assert.Equal(o1.Value.Children().Count(), o2.Value.Children().Count());
            Assert.Equal(false, JToken.DeepEquals(o1, o2));
            Assert.Equal(true, JToken.DeepEquals(o1.Value, o2.Value));
        }

        [Fact]
        public void Next()
        {
            JArray a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            JToken next = a[0].Next;
            Assert.Equal(6, (int)next);

            next = next.Next;
            Assert.True(JToken.DeepEquals(new JArray(7, 8), next));

            next = next.Next;
            Assert.True(JToken.DeepEquals(new JArray(9, 10), next));

            next = next.Next;
            Assert.Null(next);
        }

        [Fact]
        public void Previous()
        {
            JArray a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            JToken previous = a[3].Previous;
            Assert.True(JToken.DeepEquals(new JArray(7, 8), previous));

            previous = previous.Previous;
            Assert.Equal(6, (int)previous);

            previous = previous.Previous;
            Assert.Equal(5, (int)previous);

            previous = previous.Previous;
            Assert.Null(previous);
        }

        [Fact]
        public void Children()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            Assert.Equal(4, a.Count());
            Assert.Equal(3, a.Children<JArray>().Count());
        }

        [Fact]
        public void BeforeAfter()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1, 2, 3),
                    new JArray(1, 2, 3),
                    new JArray(1, 2, 3)
                    );

            Assert.Equal(5, (int)a[1].Previous);
            Assert.Equal(2, a[2].BeforeSelf().Count());
            //Assert.Equal(2, a[2].AfterSelf().Count());
        }

        [Fact]
        public void Casting()
        {
            Assert.Equal(1L, (long)(new JValue(1)));
            Assert.Equal(2L, (long)new JArray(1, 2, 3)[1]);

            Assert.Equal(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
#if !NET20
            Assert.Equal(new DateTimeOffset(2000, 12, 20, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTime(2000, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
            Assert.Equal(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
            Assert.Equal(null, (DateTimeOffset?)new JValue((DateTimeOffset?)null));
            Assert.Equal(null, (DateTimeOffset?)(JValue)null);
#endif
            Assert.Equal(true, (bool)new JValue(true));
            Assert.Equal(true, (bool?)new JValue(true));
            Assert.Equal(null, (bool?)((JValue)null));
            Assert.Equal(null, (bool?)JValue.CreateNull());
            Assert.Equal(10, (long)new JValue(10));
            Assert.Equal(null, (long?)new JValue((long?)null));
            Assert.Equal(null, (long?)(JValue)null);
            Assert.Equal(null, (int?)new JValue((int?)null));
            Assert.Equal(null, (int?)(JValue)null);
            Assert.Equal(null, (DateTime?)new JValue((DateTime?)null));
            Assert.Equal(null, (DateTime?)(JValue)null);
            Assert.Equal(null, (short?)new JValue((short?)null));
            Assert.Equal(null, (short?)(JValue)null);
            Assert.Equal(null, (float?)new JValue((float?)null));
            Assert.Equal(null, (float?)(JValue)null);
            Assert.Equal(null, (double?)new JValue((double?)null));
            Assert.Equal(null, (double?)(JValue)null);
            Assert.Equal(null, (decimal?)new JValue((decimal?)null));
            Assert.Equal(null, (decimal?)(JValue)null);
            Assert.Equal(null, (uint?)new JValue((uint?)null));
            Assert.Equal(null, (uint?)(JValue)null);
            Assert.Equal(null, (sbyte?)new JValue((sbyte?)null));
            Assert.Equal(null, (sbyte?)(JValue)null);
            Assert.Equal(null, (byte?)new JValue((byte?)null));
            Assert.Equal(null, (byte?)(JValue)null);
            Assert.Equal(null, (ulong?)new JValue((ulong?)null));
            Assert.Equal(null, (ulong?)(JValue)null);
            Assert.Equal(null, (uint?)new JValue((uint?)null));
            Assert.Equal(null, (uint?)(JValue)null);
            Assert.Equal(11.1f, (float)new JValue(11.1));
            Assert.Equal(float.MinValue, (float)new JValue(float.MinValue));
            Assert.Equal(1.1, (double)new JValue(1.1));
            Assert.Equal(uint.MaxValue, (uint)new JValue(uint.MaxValue));
            Assert.Equal(ulong.MaxValue, (ulong)new JValue(ulong.MaxValue));
            Assert.Equal(ulong.MaxValue, (ulong)new JProperty("Test", new JValue(ulong.MaxValue)));
            Assert.Equal(null, (string)new JValue((string)null));
            Assert.Equal(5m, (decimal)(new JValue(5L)));
            Assert.Equal(5m, (decimal?)(new JValue(5L)));
            Assert.Equal(5f, (float)(new JValue(5L)));
            Assert.Equal(5f, (float)(new JValue(5m)));
            Assert.Equal(5f, (float?)(new JValue(5m)));
            Assert.Equal(5, (byte)(new JValue(5)));

            Assert.Equal(null, (sbyte?)JValue.CreateNull());

            Assert.Equal("1", (string)(new JValue(1)));
            Assert.Equal("1", (string)(new JValue(1.0)));
            Assert.Equal("1.0", (string)(new JValue(1.0m)));
            Assert.Equal("True", (string)(new JValue(true)));
            Assert.Equal(null, (string)(JValue.CreateNull()));
            Assert.Equal(null, (string)(JValue)null);
            Assert.Equal("12/12/2000 12:12:12", (string)(new JValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc))));
#if !NET20
            Assert.Equal("12/12/2000 12:12:12 +00:00", (string)(new JValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero))));
#endif
            Assert.Equal(true, (bool)(new JValue(1)));
            Assert.Equal(true, (bool)(new JValue(1.0)));
            Assert.Equal(true, (bool)(new JValue("true")));
            Assert.Equal(true, (bool)(new JValue(true)));
            Assert.Equal(1, (int)(new JValue(1)));
            Assert.Equal(1, (int)(new JValue(1.0)));
            Assert.Equal(1, (int)(new JValue("1")));
            Assert.Equal(1, (int)(new JValue(true)));
            Assert.Equal(1m, (decimal)(new JValue(1)));
            Assert.Equal(1m, (decimal)(new JValue(1.0)));
            Assert.Equal(1m, (decimal)(new JValue("1")));
            Assert.Equal(1m, (decimal)(new JValue(true)));
            Assert.Equal(TimeSpan.FromMinutes(1), (TimeSpan)(new JValue(TimeSpan.FromMinutes(1))));
            Assert.Equal("00:01:00", (string)(new JValue(TimeSpan.FromMinutes(1))));
            Assert.Equal(TimeSpan.FromMinutes(1), (TimeSpan)(new JValue("00:01:00")));
            Assert.Equal("46efe013-b56a-4e83-99e4-4dce7678a5bc", (string)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"))));
            Assert.Equal("http://www.google.com/", (string)(new JValue(new Uri("http://www.google.com"))));
            Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
            Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"))));
            Assert.Equal(new Uri("http://www.google.com"), (Uri)(new JValue("http://www.google.com")));
            Assert.Equal(new Uri("http://www.google.com"), (Uri)(new JValue(new Uri("http://www.google.com"))));
            Assert.Equal(null, (Uri)(JValue.CreateNull()));
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi")), (string)(new JValue(Encoding.UTF8.GetBytes("hi"))));
            Assert.Equal((byte[])Encoding.UTF8.GetBytes("hi"), (byte[])(new JValue(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi")))));
            Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray())));
            Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid?)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray())));
            Assert.Equal((sbyte?)1, (sbyte?)(new JValue((short?)1)));

            Assert.Equal(null, (Uri)(JValue)null);
            Assert.Equal(null, (int?)(JValue)null);
            Assert.Equal(null, (uint?)(JValue)null);
            Assert.Equal(null, (Guid?)(JValue)null);
            Assert.Equal(null, (TimeSpan?)(JValue)null);
            Assert.Equal(null, (byte[])(JValue)null);
            Assert.Equal(null, (bool?)(JValue)null);
            Assert.Equal(null, (char?)(JValue)null);
            Assert.Equal(null, (DateTime?)(JValue)null);
#if !NET20
            Assert.Equal(null, (DateTimeOffset?)(JValue)null);
#endif
            Assert.Equal(null, (short?)(JValue)null);
            Assert.Equal(null, (ushort?)(JValue)null);
            Assert.Equal(null, (byte?)(JValue)null);
            Assert.Equal(null, (byte?)(JValue)null);
            Assert.Equal(null, (sbyte?)(JValue)null);
            Assert.Equal(null, (sbyte?)(JValue)null);
            Assert.Equal(null, (long?)(JValue)null);
            Assert.Equal(null, (ulong?)(JValue)null);
            Assert.Equal(null, (double?)(JValue)null);
            Assert.Equal(null, (float?)(JValue)null);

            byte[] data = new byte[0];
            Assert.Equal(data, (byte[])(new JValue(data)));

            Assert.Equal(5, (int)(new JValue(StringComparison.OrdinalIgnoreCase)));

#if NET40
            string bigIntegerText = "1234567899999999999999999999999999999999999999999999999999999999999990";

            Assert.Equal(BigInteger.Parse(bigIntegerText), (new JValue(BigInteger.Parse(bigIntegerText))).Value);

            Assert.Equal(BigInteger.Parse(bigIntegerText), (new JValue(bigIntegerText)).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(long.MaxValue), (new JValue(long.MaxValue)).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(4.5d), (new JValue((4.5d))).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(4.5f), (new JValue((4.5f))).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(byte.MaxValue), (new JValue(byte.MaxValue)).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(123), (new JValue(123)).ToObject<BigInteger>());
            Assert.Equal(new BigInteger(123), (new JValue(123)).ToObject<BigInteger>());
            Assert.Equal(null, (JValue.CreateNull()).ToObject<BigInteger>());

            byte[] intData = BigInteger.Parse(bigIntegerText).ToByteArray();
            Assert.Equal(BigInteger.Parse(bigIntegerText), (new JValue(intData)).ToObject<BigInteger>());

            Assert.Equal(4.0d, (double)(new JValue(new BigInteger(4.5d))));
            Assert.Equal(true, (bool)(new JValue(new BigInteger(1))));
            Assert.Equal(long.MaxValue, (long)(new JValue(new BigInteger(long.MaxValue))));
            Assert.Equal(long.MaxValue, (long)(new JValue(new BigInteger(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 }))));
            Assert.Equal("9223372036854775807", (string)(new JValue(new BigInteger(long.MaxValue))));

            intData = (byte[])(new JValue(new BigInteger(long.MaxValue)));
            Assert.Equal(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 }, intData);
#endif
        }

        [Fact]
        public void FailedCasting()
        {
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(true); }, "Can not convert Boolean to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1); }, "Can not convert Integer to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1.1); }, "Can not convert Float to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1.1m); }, "Can not convert Float to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(TimeSpan.Zero); }, "Can not convert TimeSpan to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)JValue.CreateNull(); }, "Can not convert Null to DateTime.");
            AssertException.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(Guid.NewGuid()); }, "Can not convert Guid to DateTime.");

            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(true); }, "Can not convert Boolean to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1); }, "Can not convert Integer to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1.1); }, "Can not convert Float to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1.1m); }, "Can not convert Float to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(TimeSpan.Zero); }, "Can not convert TimeSpan to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(Guid.NewGuid()); }, "Can not convert Guid to Uri.");
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(DateTime.Now); }, "Can not convert Date to Uri.");
#if !NET20
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(DateTimeOffset.Now); }, "Can not convert Date to Uri.");
#endif

            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(true); }, "Can not convert Boolean to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1); }, "Can not convert Integer to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1.1); }, "Can not convert Float to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1.1m); }, "Can not convert Float to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)JValue.CreateNull(); }, "Can not convert Null to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(Guid.NewGuid()); }, "Can not convert Guid to TimeSpan.");
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(DateTime.Now); }, "Can not convert Date to TimeSpan.");
#if !NET20
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(DateTimeOffset.Now); }, "Can not convert Date to TimeSpan.");
#endif
            AssertException.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to TimeSpan.");

            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(true); }, "Can not convert Boolean to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1); }, "Can not convert Integer to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1.1); }, "Can not convert Float to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1.1m); }, "Can not convert Float to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)JValue.CreateNull(); }, "Can not convert Null to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(DateTime.Now); }, "Can not convert Date to Guid.");
#if !NET20
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(DateTimeOffset.Now); }, "Can not convert Date to Guid.");
#endif
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(TimeSpan.FromMinutes(1)); }, "Can not convert TimeSpan to Guid.");
            AssertException.Throws<ArgumentException>(() => { var i = (Guid)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to Guid.");

#if !NET20
            AssertException.Throws<ArgumentException>(() => { var i = (DateTimeOffset)new JValue(true); }, "Can not convert Boolean to DateTimeOffset.");
#endif
            AssertException.Throws<ArgumentException>(() => { var i = (Uri)new JValue(true); }, "Can not convert Boolean to Uri.");

#if NET40
            AssertException.Throws<ArgumentException>(() => { var i = (new JValue(new Uri("http://www.google.com"))).ToObject<BigInteger>(); }, "Can not convert Uri to BigInteger.");
            AssertException.Throws<ArgumentException>(() => { var i = (JValue.CreateNull()).ToObject<BigInteger>(); }, "Can not convert Null to BigInteger.");
            AssertException.Throws<ArgumentException>(() => { var i = (new JValue(Guid.NewGuid())).ToObject<BigInteger>(); }, "Can not convert Guid to BigInteger.");
            AssertException.Throws<ArgumentException>(() => { var i = (new JValue(Guid.NewGuid())).ToObject<BigInteger>(); }, "Can not convert Guid to BigInteger.");
#endif

            AssertException.Throws<ArgumentException>(() => { var i = (sbyte?)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");
            AssertException.Throws<ArgumentException>(() => { var i = (sbyte)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");

            AssertException.Throws<ArgumentException>(() => { var i = (new JValue("Ordinal1")).ToObject<StringComparison>(); }, "Could not convert 'Ordinal1' to StringComparison.");
            AssertException.Throws<ArgumentException>(() => { var i = (new JValue("Ordinal1")).ToObject<StringComparison?>(); }, "Could not convert 'Ordinal1' to StringComparison.");
        }

        [Fact]
        public void ToObject()
        {
#if NET40
            Assert.Equal((BigInteger)1, (new JValue(1).ToObject(typeof(BigInteger))));
            Assert.Equal((BigInteger)1, (new JValue(1).ToObject(typeof(BigInteger))));
            Assert.Equal((BigInteger)null, (JValue.CreateNull().ToObject(typeof(BigInteger))));
#endif
            Assert.Equal((ushort)1, (new JValue(1).ToObject(typeof(ushort))));
            Assert.Equal((ushort)1, (new JValue(1).ToObject(typeof(ushort?))));
            Assert.Equal((uint)1L, (new JValue(1).ToObject(typeof(uint))));
            Assert.Equal((uint)1L, (new JValue(1).ToObject(typeof(uint?))));
            Assert.Equal((ulong)1L, (new JValue(1).ToObject(typeof(ulong))));
            Assert.Equal((ulong)1L, (new JValue(1).ToObject(typeof(ulong?))));
            Assert.Equal((sbyte)1L, (new JValue(1).ToObject(typeof(sbyte))));
            Assert.Equal((sbyte)1L, (new JValue(1).ToObject(typeof(sbyte?))));
            Assert.Equal((byte)1L, (new JValue(1).ToObject(typeof(byte))));
            Assert.Equal((byte)1L, (new JValue(1).ToObject(typeof(byte?))));
            Assert.Equal((short)1L, (new JValue(1).ToObject(typeof(short))));
            Assert.Equal((short)1L, (new JValue(1).ToObject(typeof(short?))));
            Assert.Equal(1, (new JValue(1).ToObject(typeof(int))));
            Assert.Equal(1, (new JValue(1).ToObject(typeof(int?))));
            Assert.Equal(1L, (new JValue(1).ToObject(typeof(long))));
            Assert.Equal(1L, (new JValue(1).ToObject(typeof(long?))));
            Assert.Equal((float)1, (new JValue(1.0).ToObject(typeof(float))));
            Assert.Equal((float)1, (new JValue(1.0).ToObject(typeof(float?))));
            Assert.Equal((double)1, (new JValue(1.0).ToObject(typeof(double))));
            Assert.Equal((double)1, (new JValue(1.0).ToObject(typeof(double?))));
            Assert.Equal(1m, (new JValue(1).ToObject(typeof(decimal))));
            Assert.Equal(1m, (new JValue(1).ToObject(typeof(decimal?))));
            Assert.Equal(true, (new JValue(true).ToObject(typeof(bool))));
            Assert.Equal(true, (new JValue(true).ToObject(typeof(bool?))));
            Assert.Equal('b', (new JValue('b').ToObject(typeof(char))));
            Assert.Equal('b', (new JValue('b').ToObject(typeof(char?))));
            Assert.Equal(TimeSpan.MaxValue, (new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan))));
            Assert.Equal(TimeSpan.MaxValue, (new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan?))));
            Assert.Equal(DateTime.MaxValue, (new JValue(DateTime.MaxValue).ToObject(typeof(DateTime))));
            Assert.Equal(DateTime.MaxValue, (new JValue(DateTime.MaxValue).ToObject(typeof(DateTime?))));
#if !NET20
            Assert.Equal(DateTimeOffset.MaxValue, (new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset))));
            Assert.Equal(DateTimeOffset.MaxValue, (new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset?))));
#endif
            Assert.Equal("b", (new JValue("b").ToObject(typeof(string))));
            Assert.Equal(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), (new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid))));
            Assert.Equal(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), (new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid?))));
            Assert.Equal(new Uri("http://www.google.com/"), (new JValue(new Uri("http://www.google.com/")).ToObject(typeof(Uri))));
            Assert.Equal(StringComparison.Ordinal, (new JValue("Ordinal").ToObject(typeof(StringComparison))));
            Assert.Equal(StringComparison.Ordinal, (new JValue("Ordinal").ToObject(typeof(StringComparison?))));
            Assert.Equal(null, (JValue.CreateNull().ToObject(typeof(StringComparison?))));
        }

        [Fact]
        public void ImplicitCastingTo()
        {
            Assert.True(JToken.DeepEquals(new JValue(new DateTime(2000, 12, 20)), (JValue)new DateTime(2000, 12, 20)));
            Assert.True(JToken.DeepEquals(new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)), (JValue)new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
            Assert.True(JToken.DeepEquals(new JValue((DateTimeOffset?)null), (JValue)(DateTimeOffset?)null));

#if NET40
            // had to remove implicit casting to avoid user reference to System.Numerics.dll
            Assert.True(JToken.DeepEquals(new JValue(new BigInteger(1)), new JValue(new BigInteger(1))));
            Assert.True(JToken.DeepEquals(new JValue((BigInteger)null), new JValue((BigInteger)null)));
#endif
            Assert.True(JToken.DeepEquals(new JValue(true), (JValue)true));
            Assert.True(JToken.DeepEquals(new JValue(true), (JValue)true));
            Assert.True(JToken.DeepEquals(new JValue(true), (JValue)(bool?)true));
            Assert.True(JToken.DeepEquals(new JValue((bool?)null), (JValue)(bool?)null));
            Assert.True(JToken.DeepEquals(new JValue(10), (JValue)10));
            Assert.True(JToken.DeepEquals(new JValue((long?)null), (JValue)(long?)null));
            Assert.True(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
            Assert.True(JToken.DeepEquals(new JValue(long.MaxValue), (JValue)long.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue((int?)null), (JValue)(int?)null));
            Assert.True(JToken.DeepEquals(new JValue((short?)null), (JValue)(short?)null));
            Assert.True(JToken.DeepEquals(new JValue((double?)null), (JValue)(double?)null));
            Assert.True(JToken.DeepEquals(new JValue((uint?)null), (JValue)(uint?)null));
            Assert.True(JToken.DeepEquals(new JValue((decimal?)null), (JValue)(decimal?)null));
            Assert.True(JToken.DeepEquals(new JValue((ulong?)null), (JValue)(ulong?)null));
            Assert.True(JToken.DeepEquals(new JValue((sbyte?)null), (JValue)(sbyte?)null));
            Assert.True(JToken.DeepEquals(new JValue((sbyte)1), (JValue)(sbyte)1));
            Assert.True(JToken.DeepEquals(new JValue((byte?)null), (JValue)(byte?)null));
            Assert.True(JToken.DeepEquals(new JValue((byte)1), (JValue)(byte)1));
            Assert.True(JToken.DeepEquals(new JValue((ushort?)null), (JValue)(ushort?)null));
            Assert.True(JToken.DeepEquals(new JValue(short.MaxValue), (JValue)short.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(ushort.MaxValue), (JValue)ushort.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(11.1f), (JValue)11.1f));
            Assert.True(JToken.DeepEquals(new JValue(float.MinValue), (JValue)float.MinValue));
            Assert.True(JToken.DeepEquals(new JValue(double.MinValue), (JValue)double.MinValue));
            Assert.True(JToken.DeepEquals(new JValue(uint.MaxValue), (JValue)uint.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(ulong.MaxValue), (JValue)ulong.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(ulong.MinValue), (JValue)ulong.MinValue));
            Assert.True(JToken.DeepEquals(new JValue((string)null), (JValue)(string)null));
            Assert.True(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
            Assert.True(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)decimal.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)(decimal?)decimal.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(decimal.MinValue), (JValue)decimal.MinValue));
            Assert.True(JToken.DeepEquals(new JValue(float.MaxValue), (JValue)(float?)float.MaxValue));
            Assert.True(JToken.DeepEquals(new JValue(double.MaxValue), (JValue)(double?)double.MaxValue));
            Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(double?)null));

            Assert.False(JToken.DeepEquals(new JValue(true), (JValue)(bool?)null));
            Assert.False(JToken.DeepEquals(JValue.CreateNull(), (JValue)(object)null));

            byte[] emptyData = new byte[0];
            Assert.True(JToken.DeepEquals(new JValue(emptyData), (JValue)emptyData));
            Assert.False(JToken.DeepEquals(new JValue(emptyData), (JValue)new byte[1]));
            Assert.True(JToken.DeepEquals(new JValue(Encoding.UTF8.GetBytes("Hi")), (JValue)Encoding.UTF8.GetBytes("Hi")));

            Assert.True(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)TimeSpan.FromMinutes(1)));
            Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(TimeSpan?)null));
            Assert.True(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)(TimeSpan?)TimeSpan.FromMinutes(1)));
            Assert.True(JToken.DeepEquals(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")), (JValue)new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
            Assert.True(JToken.DeepEquals(new JValue(new Uri("http://www.google.com")), (JValue)new Uri("http://www.google.com")));
            Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Uri)null));
            Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Guid?)null));
        }

        [Fact]
        public void Root()
        {
            JArray a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            Assert.Equal(a, a.Root);
            Assert.Equal(a, a[0].Root);
            Assert.Equal(a, ((JArray)a[2])[0].Root);
        }

        [Fact]
        public void Remove()
        {
            JToken t;
            JArray a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            a[0].Remove();

            Assert.Equal(6, (int)a[0]);

            a[1].Remove();

            Assert.Equal(6, (int)a[0]);
            Assert.True(JToken.DeepEquals(new JArray(9, 10), a[1]));
            Assert.Equal(2, a.Count());

            t = a[1];
            t.Remove();
            Assert.Equal(6, (int)a[0]);
            Assert.Null(t.Next);
            Assert.Null(t.Previous);
            Assert.Null(t.Parent);

            t = a[0];
            t.Remove();
            Assert.Equal(0, a.Count());

            Assert.Null(t.Next);
            Assert.Null(t.Previous);
            Assert.Null(t.Parent);
        }

        [Fact]
        public void AfterSelf()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            JToken t = a[1];
            List<JToken> afterTokens = t.AfterSelf().ToList();

            Assert.Equal(2, afterTokens.Count);
            Assert.True(JToken.DeepEquals(new JArray(1, 2), afterTokens[0]));
            Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), afterTokens[1]));
        }

        [Fact]
        public void BeforeSelf()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            JToken t = a[2];
            List<JToken> beforeTokens = t.BeforeSelf().ToList();

            Assert.Equal(2, beforeTokens.Count);
            Assert.True(JToken.DeepEquals(new JValue(5), beforeTokens[0]));
            Assert.True(JToken.DeepEquals(new JArray(1), beforeTokens[1]));
        }

        [Fact]
        public void HasValues()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            Assert.True(a.HasValues);
        }

        [Fact]
        public void Ancestors()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            JToken t = a[1][0];
            List<JToken> ancestors = t.Ancestors().ToList();
            Assert.Equal(2, ancestors.Count());
            Assert.Equal(a[1], ancestors[0]);
            Assert.Equal(a, ancestors[1]);
        }

        [Fact]
        public void Descendants()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            List<JToken> descendants = a.Descendants().ToList();
            Assert.Equal(10, descendants.Count());
            Assert.Equal(5, (int)descendants[0]);
            Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 4]));
            Assert.Equal(1, (int)descendants[descendants.Count - 3]);
            Assert.Equal(2, (int)descendants[descendants.Count - 2]);
            Assert.Equal(3, (int)descendants[descendants.Count - 1]);
        }

        [Fact]
        public void CreateWriter()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            JsonWriter writer = a.CreateWriter();
            Assert.NotNull(writer);
            Assert.Equal(4, a.Count());

            writer.WriteValue("String");
            Assert.Equal(5, a.Count());
            Assert.Equal("String", (string)a[4]);

            writer.WriteStartObject();
            writer.WritePropertyName("Property");
            writer.WriteValue("PropertyValue");
            writer.WriteEnd();

            Assert.Equal(6, a.Count());
            Assert.True(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
        }

        [Fact]
        public void AddFirst()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a.AddFirst("First");

            Assert.Equal("First", (string)a[0]);
            Assert.Equal(a, a[0].Parent);
            Assert.Equal(a[1], a[0].Next);
            Assert.Equal(5, a.Count());

            a.AddFirst("NewFirst");
            Assert.Equal("NewFirst", (string)a[0]);
            Assert.Equal(a, a[0].Parent);
            Assert.Equal(a[1], a[0].Next);
            Assert.Equal(6, a.Count());

            Assert.Equal(a[0], a[0].Next.Previous);
        }

        [Fact]
        public void RemoveAll()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            JToken first = a.First;
            Assert.Equal(5, (int)first);

            a.RemoveAll();
            Assert.Equal(0, a.Count());

            Assert.Null(first.Parent);
            Assert.Null(first.Next);
        }

        [Fact]
        public void AddPropertyToArray()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JArray a = new JArray();
                a.Add(new JProperty("PropertyName"));
            }, "Can not add OpenGamingLibrary.Json.Linq.JProperty to OpenGamingLibrary.Json.Linq.JArray.");
        }

        [Fact]
        public void AddValueToObject()
        {
            AssertException.Throws<ArgumentException>(() =>
            {
                JObject o = new JObject();
                o.Add(5);
            }, "Can not add OpenGamingLibrary.Json.Linq.JValue to OpenGamingLibrary.Json.Linq.JObject.");
        }

        [Fact]
        public void Replace()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[0].Replace(new JValue(int.MaxValue));
            Assert.Equal(int.MaxValue, (int)a[0]);
            Assert.Equal(4, a.Count());

            a[1][0].Replace(new JValue("Test"));
            Assert.Equal("Test", (string)a[1][0]);

            a[2].Replace(new JValue(int.MaxValue));
            Assert.Equal(int.MaxValue, (int)a[2]);
            Assert.Equal(4, a.Count());

            Assert.True(JToken.DeepEquals(new JArray(int.MaxValue, new JArray("Test"), int.MaxValue, new JArray(1, 2, 3)), a));
        }

        [Fact]
        public void ToStringWithConverters()
        {
            JArray a =
                new JArray(
                    new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
                    );

            string json = a.ToString(Formatting.Indented, new IsoDateTimeConverter());

            StringAssert.Equal(@"[
  ""2009-02-15T00:00:00Z""
]", json);

            json = JsonConvert.SerializeObject(a, new IsoDateTimeConverter());

            Assert.Equal(@"[""2009-02-15T00:00:00Z""]", json);
        }

        [Fact]
        public void ToStringWithNoIndenting()
        {
            JArray a =
                new JArray(
                    new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
                    );

            string json = a.ToString(Formatting.None, new IsoDateTimeConverter());

            Assert.Equal(@"[""2009-02-15T00:00:00Z""]", json);
        }

        [Fact]
        public void AddAfterSelf()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[1].AddAfterSelf("pie");

            Assert.Equal(5, (int)a[0]);
            Assert.Equal(1, a[1].Count());
            Assert.Equal("pie", (string)a[2]);
            Assert.Equal(5, a.Count());

            a[4].AddAfterSelf("lastpie");

            Assert.Equal("lastpie", (string)a[5]);
            Assert.Equal("lastpie", (string)a.Last);
        }

        [Fact]
        public void AddBeforeSelf()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[1].AddBeforeSelf("pie");

            Assert.Equal(5, (int)a[0]);
            Assert.Equal("pie", (string)a[1]);
            Assert.Equal(a, a[1].Parent);
            Assert.Equal(a[2], a[1].Next);
            Assert.Equal(5, a.Count());

            a[0].AddBeforeSelf("firstpie");

            Assert.Equal("firstpie", (string)a[0]);
            Assert.Equal(5, (int)a[1]);
            Assert.Equal("pie", (string)a[2]);
            Assert.Equal(a, a[0].Parent);
            Assert.Equal(a[1], a[0].Next);
            Assert.Equal(6, a.Count());

            a.Last.AddBeforeSelf("secondlastpie");

            Assert.Equal("secondlastpie", (string)a[5]);
            Assert.Equal(7, a.Count());
        }

        [Fact]
        public void DeepClone()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3),
                    new JObject(
                        new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
                        new JProperty("Second", 1),
                        new JProperty("Third", null),
                        new JProperty("Fourth", new JConstructor("Date", 12345)),
                        new JProperty("Fifth", double.PositiveInfinity),
                        new JProperty("Sixth", double.NaN)
                        )
                    );

            JArray a2 = (JArray)a.DeepClone();

            Console.WriteLine(a2.ToString(Formatting.Indented));

            Assert.True(a.DeepEquals(a2));
        }

#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
        [Fact]
        public void Clone()
        {
            JArray a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3),
                    new JObject(
                        new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
                        new JProperty("Second", 1),
                        new JProperty("Third", null),
                        new JProperty("Fourth", new JConstructor("Date", 12345)),
                        new JProperty("Fifth", double.PositiveInfinity),
                        new JProperty("Sixth", double.NaN)
                        )
                    );

            ICloneable c = a;

            JArray a2 = (JArray)c.Clone();

            Assert.True(a.DeepEquals(a2));
        }
#endif

        [Fact]
        public void DoubleDeepEquals()
        {
            JArray a =
                new JArray(
                    double.NaN,
                    double.PositiveInfinity,
                    double.NegativeInfinity
                    );

            JArray a2 = (JArray)a.DeepClone();

            Assert.True(a.DeepEquals(a2));

            double d = 1 + 0.1 + 0.1 + 0.1;

            JValue v1 = new JValue(d);
            JValue v2 = new JValue(1.3);

            Assert.True(v1.DeepEquals(v2));
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            AssertException.Throws<JsonReaderException>(() =>
            {
                string json = @"[
""Small"",
""Medium"",
""Large""
],";

                JToken.Parse(json);
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 2.");
        }

        [Fact]
        public void Path()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", new JArray(1, 2, 3)),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", new JObject(new JProperty("Test1", new JArray(1, new JObject(new JProperty("Test1", 1)), 3)))),
                    new JProperty("Test4", new JConstructor("Date", new JArray(1, 2, 3)))
                    );

            JToken t = o.SelectToken("Test1[0]");
            Assert.Equal("Test1[0]", t.Path);

            t = o.SelectToken("Test2");
            Assert.Equal("Test2", t.Path);

            t = o.SelectToken("");
            Assert.Equal("", t.Path);

            t = o.SelectToken("Test4[0][0]");
            Assert.Equal("Test4[0][0]", t.Path);

            t = o.SelectToken("Test4[0]");
            Assert.Equal("Test4[0]", t.Path);

            t = t.DeepClone();
            Assert.Equal("", t.Path);

            t = o.SelectToken("Test3.Test1[1].Test1");
            Assert.Equal("Test3.Test1[1].Test1", t.Path);

            JArray a = new JArray(1);
            Assert.Equal("", a.Path);

            Assert.Equal("[0]", a[0].Path);
        }
    }
}