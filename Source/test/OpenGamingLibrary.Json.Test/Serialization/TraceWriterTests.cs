// Copyright (c) 2007 James Newton-King
// Copyright (c) 2014 Extesla, LLC.
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Numerics;
using OpenGamingLibrary.Xunit.Extensions;
using Xunit;

namespace OpenGamingLibrary.Json.Test.Serialization
{
    public class Staff
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public IList<string> Roles { get; set; }
    }

    
    public class TraceWriterTests : TestFixtureBase
    {
#if !(PORTABLE || ASPNETCORE50 || NETFX_CORE || PORTABLE40)
        [Fact]
        public void DiagnosticsTraceWriterTest()
        {
            StringWriter sw = new StringWriter();
            TextWriterTraceListener listener = new TextWriterTraceListener(sw);

            try
            {
                Trace.AutoFlush = true;
                Trace.Listeners.Add(listener);

                DiagnosticsTraceWriter traceWriter = new DiagnosticsTraceWriter();
                traceWriter.Trace(TraceLevel.Verbose, "Verbose!", null);
                traceWriter.Trace(TraceLevel.Info, "Info!", null);
                traceWriter.Trace(TraceLevel.Warning, "Warning!", null);
                traceWriter.Trace(TraceLevel.Error, "Error!", null);
                traceWriter.Trace(TraceLevel.Off, "Off!", null);

                StringAssert.Equal(@"OpenGamingLibrary.Json Verbose: 0 : Verbose!
OpenGamingLibrary.Json Information: 0 : Info!
OpenGamingLibrary.Json Warning: 0 : Warning!
OpenGamingLibrary.Json Error: 0 : Error!
", sw.ToString());
            }
            finally
            {
                Trace.Listeners.Remove(listener);
                Trace.AutoFlush = false;
            }
        }
#endif

        [Fact]
        public void MemoryTraceWriterSerializeTest()
        {
            Staff staff = new Staff();
            staff.Name = "Arnie Admin";
            staff.Roles = new List<string> { "Administrator" };
            staff.StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.SerializeObject(
                staff,
                new JsonSerializerSettings { TraceWriter = traceWriter, Converters = { new JavaScriptDateTimeConverter() } });

            Console.WriteLine(traceWriter);
            // 2012-11-11T12:08:42.761 Info Started serializing OpenGamingLibrary.Json.Test.Serialization.Staff. Path ''.
            // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.799 Info Finished serializing OpenGamingLibrary.Json.Test.Serialization.Staff. Path ''.

            MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;
            string output = memoryTraceWriter.ToString();

            Assert.Equal(916, output.Length);
            Assert.Equal(7, memoryTraceWriter.GetTraceMessages().Count());

            string json = @"Serialized JSON: 
{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

            json = StringAssert.Normalize(json);
            output = StringAssert.Normalize(output);

            Assert.True(output.Contains(json));
        }

        [Fact]
        public void MemoryTraceWriterDeserializeTest()
        {
            string json = @"{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

            Staff staff = new Staff();
            staff.Name = "Arnie Admin";
            staff.Roles = new List<string> { "Administrator" };
            staff.StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.DeserializeObject<Staff>(
                json,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter,
                    Converters = { new JavaScriptDateTimeConverter() },
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });

            Console.WriteLine(traceWriter);
            // 2012-11-11T12:08:42.761 Info Started serializing OpenGamingLibrary.Json.Test.Serialization.Staff. Path ''.
            // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.799 Info Finished serializing OpenGamingLibrary.Json.Test.Serialization.Staff. Path ''.
            // 2013-05-19T00:07:24.360 Verbose Deserialized JSON: 
            // {
            //   "Name": "Arnie Admin",
            //   "StartDate": new Date(
            //     976623132000
            //   ),
            //   "Roles": [
            //     "Administrator"
            //   ]
            // }

            MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;
            string output = memoryTraceWriter.ToString();

            Assert.Equal(1059, output.Length);
            Assert.Equal(7, memoryTraceWriter.GetTraceMessages().Count());

            json = StringAssert.Normalize(json);
            output = StringAssert.Normalize(output);

            Assert.True(output.Contains(json));
        }

        [Fact]
        public void MemoryTraceWriterLimitTest()
        {
            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            for (int i = 0; i < 1005; i++)
            {
                traceWriter.Trace(TraceLevel.Verbose, (i + 1).ToString(CultureInfo.InvariantCulture), null);
            }

            IList<string> traceMessages = traceWriter.GetTraceMessages().ToList();

            Assert.Equal(1000, traceMessages.Count);

            Assert.True(traceMessages.First().EndsWith(" 6"));
            Assert.True(traceMessages.Last().EndsWith(" 1005"));
        }

        [Fact]
        public void Serialize()
        {
            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            string json =
                JsonConvert.SerializeObject(
                    new TraceTestObject
                    {
                        StringArray = new[] { "1", "2" },
                        IntList = new List<int> { 1, 2 },
                        Version = new Version(1, 2, 3, 4),
                        StringDictionary =
                            new Dictionary<string, string>
                            {
                                { "1", "!" },
                                { "Two", "!!" },
                                { "III", "!!!" }
                            }
                    },
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter,
                        Formatting = Formatting.Indented
                    });

            Assert.Equal("Started serializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("Started serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[3].Message);
            Assert.Equal("Finished serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[4].Message);
            Assert.Equal("Started serializing System.Version. Path 'Version'.", traceWriter.TraceRecords[5].Message);
            Assert.Equal("Finished serializing System.Version. Path 'Version'.", traceWriter.TraceRecords[6].Message);
            Assert.Equal("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[7].Message);
            Assert.Equal("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[8].Message);
            Assert.Equal("Finished serializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[9].Message);

            Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Fact]
        public void Deserialize()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            TraceTestObject o2 = JsonConvert.DeserializeObject<TraceTestObject>(
                @"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  }
}",
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });

            Assert.Equal(2, o2.IntList.Count);
            Assert.Equal(2, o2.StringArray.Length);
            Assert.Equal(1, o2.Version.Major);
            Assert.Equal(2, o2.Version.Minor);
            Assert.Equal(3, o2.StringDictionary.Count);

            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path 'IntList', line 2, position 13.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 15.", traceWriter.TraceRecords[1].Message);
            Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
            Assert.Equal("Started deserializing System.String[]. Path 'StringArray', line 6, position 19.", traceWriter.TraceRecords[3].Message);
            Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
            Assert.Equal("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 13.", traceWriter.TraceRecords[5].Message);
            Assert.True(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing System.Version. Path 'Version'"));
            Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing System.Version. Path 'Version'"));
            Assert.Equal("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 9.", traceWriter.TraceRecords[8].Message);
            Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
            Assert.True(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path ''"));

            Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Fact]
        public void Populate()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            TraceTestObject o2 = new TraceTestObject();

            JsonConvert.PopulateObject(@"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  }
}",
                o2,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });

            Assert.Equal(2, o2.IntList.Count);
            Assert.Equal(2, o2.StringArray.Length);
            Assert.Equal(1, o2.Version.Major);
            Assert.Equal(2, o2.Version.Minor);
            Assert.Equal(3, o2.StringDictionary.Count);

            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path 'IntList', line 2, position 13.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 15.", traceWriter.TraceRecords[1].Message);
            Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
            Assert.Equal("Started deserializing System.String[]. Path 'StringArray', line 6, position 19.", traceWriter.TraceRecords[3].Message);
            Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
            Assert.Equal("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 13.", traceWriter.TraceRecords[5].Message);
            Assert.True(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing System.Version. Path 'Version'"));
            Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing System.Version. Path 'Version'"));
            Assert.Equal("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 9.", traceWriter.TraceRecords[8].Message);
            Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
            Assert.True(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path ''"));

            Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Fact]
        public void ErrorDeserializing()
        {
            string json = @"{""Integer"":""hi""}";

            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            AssertException.Throws<Exception>(() =>
            {
                JsonConvert.DeserializeObject<IntegerTestClass>(
                    json,
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Could not convert string to integer: hi. Path 'Integer', line 1, position 15.");

            Assert.Equal(2, traceWriter.TraceRecords.Count);

            Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.IntegerTestClass. Path 'Integer', line 1, position 11.", traceWriter.TraceRecords[0].Message);

            Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
            Assert.Equal("Error deserializing OpenGamingLibrary.Json.Test.Serialization.IntegerTestClass. Could not convert string to integer: hi. Path 'Integer', line 1, position 15.", traceWriter.TraceRecords[1].Message);
        }

        [Fact]
        public void ErrorDeserializingNested()
        {
            string json = @"{""IntList"":[1, ""two""]}";

            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            AssertException.Throws<Exception>(() =>
            {
                JsonConvert.DeserializeObject<TraceTestObject>(
                    json,
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");

            Assert.Equal(3, traceWriter.TraceRecords.Count);

            Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.TraceTestObject. Path 'IntList', line 1, position 11.", traceWriter.TraceRecords[0].Message);

            Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[1].Level);
            Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 1, position 12.", traceWriter.TraceRecords[1].Message);

            Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[2].Level);
            Assert.Equal("Error deserializing System.Collections.Generic.IList`1[System.Int32]. Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.", traceWriter.TraceRecords[2].Message);
        }

        [Fact]
        public void SerializeDictionarysWithPreserveObjectReferences()
        {
            PreserveReferencesHandlingTests.CircularDictionary circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
            circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.SerializeObject(
                circularDictionary,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TraceWriter = traceWriter
                });

            Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '1' for OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path ''."));
            Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '2' for OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other'."));
            Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference to Id '1' for OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'."));
        }

        [Fact]
        public void DeserializeDictionarysWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}";

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.DeserializeObject<PreserveReferencesHandlingTests.CircularDictionary>(json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default,
                    TraceWriter = traceWriter
                });

            Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '1' for OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other', line 3, position 11."));
            Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '2' for OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other.blah', line 5, position 12."));
            Assert.True(traceWriter.TraceRecords.Any(r => r.Message.StartsWith("Resolved object reference '1' to OpenGamingLibrary.Json.Test.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'")));
        }

        [Fact]
        public void WriteTypeNameForObjects()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            IList<object> l = new List<object>
            {
                new Dictionary<string, string> { { "key!", "value!" } },
                new Version(1, 2, 3, 4)
            };

            JsonConvert.SerializeObject(l, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TraceWriter = traceWriter
            });

            Assert.Equal("Started serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Writing type name 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' for System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values'.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("Writing type name 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' for System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[3].Message);
            Assert.Equal("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[4].Message);
            Assert.Equal("Started serializing System.Version. Path '$values[0]'.", traceWriter.TraceRecords[5].Message);
            Assert.Equal("Writing type name 'System.Version, mscorlib' for System.Version. Path '$values[1]'.", traceWriter.TraceRecords[6].Message);
            Assert.Equal("Finished serializing System.Version. Path '$values[1]'.", traceWriter.TraceRecords[7].Message);
            Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[8].Message);
        }

        [Fact]
        public void SerializeConverter()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            IList<DateTime> d = new List<DateTime>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
            };

            string json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = { new JavaScriptDateTimeConverter() },
                TraceWriter = traceWriter
            });

            Assert.Equal("Started serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("Finished serializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path '[0]'.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[3].Message);
        }

        [Fact]
        public void DeserializeConverter()
        {
            string json = @"[new Date(976623132000)]";

            InMemoryTraceWriter traceWriter =
                new InMemoryTraceWriter
                {
                    LevelFilter = TraceLevel.Verbose
                };

            JsonConvert.DeserializeObject<List<DateTime>>(
                json,
                new JsonSerializerSettings
                {
                    Converters = { new JavaScriptDateTimeConverter() },
                    TraceWriter = traceWriter
                });

            Assert.Equal("Started deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 1.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started deserializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path '[0]', line 1, position 10.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("Finished deserializing System.DateTime with converter OpenGamingLibrary.Json.Converters.JavaScriptDateTimeConverter. Path '[0]', line 1, position 23.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("Finished deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 24.", traceWriter.TraceRecords[3].Message);
        }

        [Fact]
        public void DeserializeTypeName()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{
  ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
  ""$values"": [
    {
      ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib"",
      ""key!"": ""value!""
    },
    {
      ""$type"": ""System.Version, mscorlib"",
      ""Major"": 1,
      ""Minor"": 2,
      ""Build"": 3,
      ""Revision"": 4,
      ""MajorRevision"": 0,
      ""MinorRevision"": 4
    }
  ]
}";

            JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                TraceWriter = traceWriter
            });

            Assert.Equal("Resolved type 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' to System.Collections.Generic.List`1[System.Object]. Path '$type', line 2, position 84.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Started deserializing System.Collections.Generic.List`1[System.Object]. Path '$values', line 3, position 15.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("Resolved type 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' to System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].$type', line 5, position 120.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("Started deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].key!', line 6, position 14.", traceWriter.TraceRecords[3].Message);
            Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'"));
            Assert.Equal("Resolved type 'System.Version, mscorlib' to System.Version. Path '$values[1].$type', line 9, position 42.", traceWriter.TraceRecords[5].Message);
            Assert.Equal("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path '$values[1].Major', line 10, position 15.", traceWriter.TraceRecords[6].Message);
            Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Started deserializing System.Version. Path '$values[1]'"));
            Assert.True(traceWriter.TraceRecords[8].Message.StartsWith("Finished deserializing System.Version. Path '$values[1]'"));
            Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.List`1[System.Object]. Path '$values'"));
        }

        [Fact]
        public void DeserializeISerializable()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            AssertException.Throws<SerializationException>(() =>
            {
                JsonConvert.DeserializeObject<Exception>(
                    "{}",
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Member 'ClassName' was not found.");

            Assert.True(traceWriter.TraceRecords[0].Message.StartsWith("Deserializing System.Exception using ISerializable constructor. Path ''"));
            Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.Equal("Error deserializing System.Exception. Member 'ClassName' was not found. Path '', line 1, position 2.", traceWriter.TraceRecords[1].Message);
            Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
        }

        [Fact]
        public void DeserializeMissingMember()
        {
            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.DeserializeObject<Person>(
                "{'MissingMemberProperty':'!!'}",
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });

            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Could not find member 'MissingMemberProperty' on OpenGamingLibrary.Json.Test.TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[1].Message);
            Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing OpenGamingLibrary.Json.Test.TestObjects.Person. Path ''"));
        }

        [Fact]
        public void DeserializeMissingMemberConstructor()
        {
            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{
  ""Major"": 1,
  ""Minor"": 2,
  ""Build"": 3,
  ""Revision"": 4,
  ""MajorRevision"": 0,
  ""MinorRevision"": 4,
  ""MissingMemberProperty"": null
}";

            JsonConvert.DeserializeObject<Version>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Assert.Equal("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Major', line 2, position 11.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("Could not find member 'MissingMemberProperty' on System.Version. Path 'MissingMemberProperty', line 8, position 32.", traceWriter.TraceRecords[1].Message);
            Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Started deserializing System.Version. Path ''"));
            Assert.True(traceWriter.TraceRecords[3].Message.StartsWith("Finished deserializing System.Version. Path ''"));
        }

        [Fact]
        public void PublicParametizedConstructorWithPropertyNameConflictWithAttribute()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{name:""1""}";

            PublicParametizedConstructorWithPropertyNameConflictWithAttribute c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithPropertyNameConflictWithAttribute>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Assert.NotNull(c);
            Assert.Equal(1, c.Name);

            Assert.Equal("Deserializing OpenGamingLibrary.Json.Test.TestObjects.PublicParametizedConstructorWithPropertyNameConflictWithAttribute using creator with parameters: name. Path 'name', line 1, position 6.", traceWriter.TraceRecords[0].Message);
        }

        [Fact]
        public void ShouldSerializeTestClass()
        {
            ShouldSerializeTestClass c = new ShouldSerializeTestClass();
            c.Age = 29;
            c.Name = "Jim";
            c._shouldSerializeName = true;

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.Equal("ShouldSerialize result for property 'Name' on OpenGamingLibrary.Json.Test.Serialization.ShouldSerializeTestClass: True. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.Equal(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            c._shouldSerializeName = false;

            JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.Equal("ShouldSerialize result for property 'Name' on OpenGamingLibrary.Json.Test.Serialization.ShouldSerializeTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.Equal(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);
        }

        [Fact]
        public void SpecifiedTest()
        {
            var c = new SpecifiedTestClass();
            c.Name = "James";
            c.Age = 27;
            c.NameSpecified = false;

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.Equal("Started serializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("IsSpecified result for property 'Name' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("IsSpecified result for property 'Weight' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("IsSpecified result for property 'Height' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[3].Message);
            Assert.Equal("IsSpecified result for property 'FavoriteNumber' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[4].Message);
            Assert.Equal("Finished serializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[5].Message);

            StringAssert.Equal(@"{
  ""Age"": 27
}", json);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            SpecifiedTestClass deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path 'Age', line 2, position 9.", traceWriter.TraceRecords[0].Message);
            Assert.True(traceWriter.TraceRecords[1].Message.StartsWith("Finished deserializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path ''"));

            Assert.Null(deserialized.Name);
            Assert.False(deserialized.NameSpecified);
            Assert.False(deserialized.WeightSpecified);
            Assert.False(deserialized.HeightSpecified);
            Assert.False(deserialized.FavoriteNumberSpecified);
            Assert.Equal(27, deserialized.Age);

            c.NameSpecified = true;
            c.WeightSpecified = true;
            c.HeightSpecified = true;
            c.FavoriteNumber = 23;
            json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Name"": ""James"",
  ""Age"": 27,
  ""Weight"": 0,
  ""Height"": 0,
  ""FavoriteNumber"": 23
}", json);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.Equal("Started deserializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path 'Name', line 2, position 10.", traceWriter.TraceRecords[0].Message);
            Assert.Equal("IsSpecified for property 'Name' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass set to true. Path 'Name', line 2, position 18.", traceWriter.TraceRecords[1].Message);
            Assert.Equal("IsSpecified for property 'Weight' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass set to true. Path 'Weight', line 4, position 14.", traceWriter.TraceRecords[2].Message);
            Assert.Equal("IsSpecified for property 'Height' on OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass set to true. Path 'Height', line 5, position 14.", traceWriter.TraceRecords[3].Message);
            Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing OpenGamingLibrary.Json.Test.Serialization.SpecifiedTestClass. Path ''"));

            Assert.Equal("James", deserialized.Name);
            Assert.True(deserialized.NameSpecified);
            Assert.True(deserialized.WeightSpecified);
            Assert.True(deserialized.HeightSpecified);
            Assert.True(deserialized.FavoriteNumberSpecified);
            Assert.Equal(27, deserialized.Age);
            Assert.Equal(23, deserialized.FavoriteNumber);
        }

        [Fact]
        public void TraceJsonWriterTest()
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter w = new JsonTextWriter(sw);
            TraceJsonWriter traceWriter = new TraceJsonWriter(w);

            traceWriter.WriteStartObject();
            traceWriter.WritePropertyName("Array");
            traceWriter.WriteStartArray();
            traceWriter.WriteValue("String!");
            traceWriter.WriteValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            traceWriter.WriteValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            traceWriter.WriteValue(1.1f);
            traceWriter.WriteValue(1.1d);
            traceWriter.WriteValue(1.1m);
            traceWriter.WriteValue(1);
            traceWriter.WriteValue((char)'!');
            traceWriter.WriteValue((short)1);
            traceWriter.WriteValue((ushort)1);
            traceWriter.WriteValue((int)1);
            traceWriter.WriteValue((uint)1);
            traceWriter.WriteValue((sbyte)1);
            traceWriter.WriteValue((byte)1);
            traceWriter.WriteValue((long)1);
            traceWriter.WriteValue((ulong)1);
            traceWriter.WriteValue((bool)true);

            traceWriter.WriteValue((DateTime?)new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            traceWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            traceWriter.WriteValue((float?)1.1f);
            traceWriter.WriteValue((double?)1.1d);
            traceWriter.WriteValue((decimal?)1.1m);
            traceWriter.WriteValue((int?)1);
            traceWriter.WriteValue((char?)'!');
            traceWriter.WriteValue((short?)1);
            traceWriter.WriteValue((ushort?)1);
            traceWriter.WriteValue((int?)1);
            traceWriter.WriteValue((uint?)1);
            traceWriter.WriteValue((sbyte?)1);
            traceWriter.WriteValue((byte?)1);
            traceWriter.WriteValue((long?)1);
            traceWriter.WriteValue((ulong?)1);
            traceWriter.WriteValue((bool?)true);
            traceWriter.WriteValue(BigInteger.Parse("9999999990000000000000000000000000000000000"));

            traceWriter.WriteValue((object)true);
            traceWriter.WriteValue(TimeSpan.FromMinutes(1));
            traceWriter.WriteValue(Guid.Empty);
            traceWriter.WriteValue(new Uri("http://www.google.com/"));
            traceWriter.WriteValue(Encoding.UTF8.GetBytes("String!"));
            traceWriter.WriteRawValue("[1],");
            traceWriter.WriteRaw("[1]");
            traceWriter.WriteNull();
            traceWriter.WriteUndefined();
            traceWriter.WriteStartConstructor("ctor");
            traceWriter.WriteValue(1);
            traceWriter.WriteEndConstructor();
            traceWriter.WriteComment("A comment");
            traceWriter.WriteWhitespace("       ");
            traceWriter.WriteEnd();
            traceWriter.WriteEndObject();
            traceWriter.Flush();
            traceWriter.Close();

            Console.WriteLine(traceWriter.GetJson());

            StringAssert.Equal(@"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    9999999990000000000000000000000000000000000,
    true,
    true,
    ""00:01:00"",
    ""00000000-0000-0000-0000-000000000000"",
    ""http://www.google.com/"",
    ""U3RyaW5nIQ=="",
    [1],[1],[1],
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/       
  ]
}", traceWriter.GetJson());
        }

        [Fact]
        public void TraceJsonReaderTest()
        {
            string json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+00:00"",
    ""U3RyaW5nIQ=="",
    1,
    1.1,
    9999999990000000000000000000000000000000000,
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/
  ]
}";

            StringReader sw = new StringReader(json);
            JsonTextReader w = new JsonTextReader(sw);
            TraceJsonReader traceReader = new TraceJsonReader(w);

            traceReader.Read();
            Assert.Equal(JsonToken.StartObject, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.PropertyName, traceReader.TokenType);
            Assert.Equal("Array", traceReader.Value);

            traceReader.Read();
            Assert.Equal(JsonToken.StartArray, traceReader.TokenType);
            Assert.Equal(null, traceReader.Value);

            traceReader.ReadAsString();
            Assert.Equal(JsonToken.String, traceReader.TokenType);
            Assert.Equal('"', traceReader.QuoteChar);
            Assert.Equal("String!", traceReader.Value);

            // for great code coverage justice!
            traceReader.QuoteChar = '\'';
            Assert.Equal('\'', traceReader.QuoteChar);

            traceReader.ReadAsString();
            Assert.Equal(JsonToken.String, traceReader.TokenType);
            Assert.Equal("2000-12-12T12:12:12Z", traceReader.Value);

            traceReader.ReadAsDateTime();
            Assert.Equal(JsonToken.Date, traceReader.TokenType);
            Assert.Equal(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), traceReader.Value);

            traceReader.ReadAsDateTimeOffset();
            Assert.Equal(JsonToken.Date, traceReader.TokenType);
            Assert.Equal(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), traceReader.Value);

            traceReader.ReadAsBytes();
            Assert.Equal(JsonToken.Bytes, traceReader.TokenType);
            Assert.Equal(Encoding.UTF8.GetBytes("String!"), (byte[])traceReader.Value);

            traceReader.ReadAsInt32();
            Assert.Equal(JsonToken.Integer, traceReader.TokenType);
            Assert.Equal(1, traceReader.Value);

            traceReader.ReadAsDecimal();
            Assert.Equal(JsonToken.Float, traceReader.TokenType);
            Assert.Equal(1.1m, traceReader.Value);

            traceReader.Read();
            Assert.Equal(JsonToken.Integer, traceReader.TokenType);
            Assert.Equal(typeof(BigInteger), traceReader.ValueType);
            Assert.Equal(BigInteger.Parse("9999999990000000000000000000000000000000000"), traceReader.Value);

            traceReader.Read();
            Assert.Equal(JsonToken.Null, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.Undefined, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.StartConstructor, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.Integer, traceReader.TokenType);
            Assert.Equal(1L, traceReader.Value);

            traceReader.Read();
            Assert.Equal(JsonToken.EndConstructor, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.Comment, traceReader.TokenType);
            Assert.Equal("A comment", traceReader.Value);

            traceReader.Read();
            Assert.Equal(JsonToken.EndArray, traceReader.TokenType);

            traceReader.Read();
            Assert.Equal(JsonToken.EndObject, traceReader.TokenType);

            Assert.False(traceReader.Read());

            traceReader.Close();

            Console.WriteLine(traceReader.GetJson());

            StringAssert.Equal(json, traceReader.GetJson());
        }
    }

    public class TraceRecord
    {
        public string Message { get; set; }
        public TraceLevel Level { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return Level + " - " + Message;
        }
    }

    public class InMemoryTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter { get; set; }
        public IList<TraceRecord> TraceRecords { get; set; }

        public InMemoryTraceWriter()
        {
            LevelFilter = TraceLevel.Verbose;
            TraceRecords = new List<TraceRecord>();
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            TraceRecords.Add(
                new TraceRecord
                {
                    Level = level,
                    Message = message,
                    Exception = ex
                });
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var traceRecord in TraceRecords)
            {
                sb.AppendLine(traceRecord.Message);
            }

            return sb.ToString();
        }
    }

    public class TraceTestObject
    {
        public IList<int> IntList { get; set; }
        public string[] StringArray { get; set; }
        public Version Version { get; set; }
        public IDictionary<string, string> StringDictionary { get; set; }
    }

    public class IntegerTestClass
    {
        public int Integer { get; set; }
    }
}