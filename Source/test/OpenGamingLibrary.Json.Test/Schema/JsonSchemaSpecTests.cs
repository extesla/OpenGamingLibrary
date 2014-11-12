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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Extensions;
using OpenGamingLibrary.IO;
using OpenGamingLibrary.Json.Linq;
using OpenGamingLibrary.Json.Schema;

namespace OpenGamingLibrary.Json.Test.Schema
{
    public class JsonSchemaSpecTest
    {
        public string FileName { get; set; }
        public string TestCaseDescription { get; set; }
        public JObject Schema { get; set; }
        public string TestDescription { get; set; }
        public JToken Data { get; set; }
        public bool IsValid { get; set; }
        public int TestNumber { get; set; }

        public override string ToString()
        {
            return FileName + " - " + TestCaseDescription + " - " + TestDescription;
        }
    }

    
    public class JsonSchemaSpecTests : TestFixtureBase
    {
		[Theory]
		[InlineData("GetSpecTestDetails")]
        public void SpecTest(JsonSchemaSpecTest jsonSchemaSpecTest)
        {
            //if (jsonSchemaSpecTest.ToString() == "enum.json - simple enum validation - something else is invalid")
            {
                Console.WriteLine("Running JSON Schema test " + jsonSchemaSpecTest.TestNumber + ": " + jsonSchemaSpecTest);

                JsonSchema s = JsonSchema.Read(jsonSchemaSpecTest.Schema.CreateReader());

                IList<string> errorMessages;
                bool v = jsonSchemaSpecTest.Data.IsValid(s, out errorMessages);
                errorMessages = errorMessages ?? new List<string>();

				var joinedErrorMessages = string.Join(", ", errorMessages.ToArray());
				Assert.True((jsonSchemaSpecTest.IsValid == v), jsonSchemaSpecTest.TestCaseDescription + " - " + jsonSchemaSpecTest.TestDescription + " - errors: " + joinedErrorMessages);
            }
        }

        public IList<JsonSchemaSpecTest> GetSpecTestDetails()
        {
            IList<JsonSchemaSpecTest> specTests = new List<JsonSchemaSpecTest>();

            // get test files location relative to the test project dll
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string baseTestPath = PathExtensions.Combine(baseDirectory, "Schema", "Specs");

            string[] testFiles = Directory.GetFiles(baseTestPath, "*.json", SearchOption.AllDirectories);

            // read through each of the *.json test files and extract the test details
            foreach (string testFile in testFiles)
            {
                string testJson = System.IO.File.ReadAllText(testFile);

                JArray a = JArray.Parse(testJson);

                foreach (JObject testCase in a)
                {
                    foreach (JObject test in testCase["tests"])
                    {
                        JsonSchemaSpecTest jsonSchemaSpecTest = new JsonSchemaSpecTest();

                        jsonSchemaSpecTest.FileName = Path.GetFileName(testFile);
                        jsonSchemaSpecTest.TestCaseDescription = (string)testCase["description"];
                        jsonSchemaSpecTest.Schema = (JObject)testCase["schema"];

                        jsonSchemaSpecTest.TestDescription = (string)test["description"];
                        jsonSchemaSpecTest.Data = test["data"];
                        jsonSchemaSpecTest.IsValid = (bool)test["valid"];
                        jsonSchemaSpecTest.TestNumber = specTests.Count + 1;

                        specTests.Add(jsonSchemaSpecTest);
                    }
                }
            }

            specTests = specTests.Where(s => s.FileName != "dependencies.json"
                                             && s.TestCaseDescription != "multiple disallow subschema"
                                             && s.TestCaseDescription != "types from separate schemas are merged"
                                             && s.TestCaseDescription != "when types includes a schema it should fully validate the schema"
                                             && s.TestCaseDescription != "types can include schemas").ToList();

            return specTests;
        }
    }
}