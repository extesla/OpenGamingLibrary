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

#if !(NET35 || NET20 || PORTABLE || ASPNETCORE50)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using OpenGamingLibrary.Json.Converters;
using OpenGamingLibrary.Json.Linq;
using Xunit;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using OpenGamingLibrary.Json.Utilities;
using System.Globalization;

namespace OpenGamingLibrary.Json.Test.Documentation
{
    public class Employee
    {
        public string Name { get; set; }
        public Employee Manager { get; set; }
    }

    #region ShouldSerializeContractResolver
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        Employee e = (Employee)instance;
                        return e.Manager != e;
                    };
            }

            return property;
        }
    }
    #endregion

    
    public class ConditionalPropertiesTests : TestFixtureBase
    {
        #region EmployeeShouldSerializeExample
        public class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }

            public bool ShouldSerializeManager()
            {
                // don't serialize the Manager property if an employee is their own manager
                return (Manager != this);
            }
        }
        #endregion

        [Fact]
        public void ShouldSerializeClassTest()
        {
            #region ShouldSerializeClassTest
            Employee joe = new Employee();
            joe.Name = "Joe Employee";
            Employee mike = new Employee();
            mike.Name = "Mike Manager";

            joe.Manager = mike;

            // mike is his own manager
            // ShouldSerialize will skip this property
            mike.Manager = mike;

            string json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented);
            // [
            //   {
            //     "Name": "Joe Employee",
            //     "Manager": {
            //       "Name": "Mike Manager"
            //     }
            //   },
            //   {
            //     "Name": "Mike Manager"
            //   }
            // ]
            #endregion

            StringAssert.Equal(@"[
  {
    ""Name"": ""Joe Employee"",
    ""Manager"": {
      ""Name"": ""Mike Manager""
    }
  },
  {
    ""Name"": ""Mike Manager""
  }
]", json);
        }

        [Fact]
        public void ShouldSerializeContractResolverTest()
        {
            OpenGamingLibrary.Json.Test.Documentation.Employee joe = new OpenGamingLibrary.Json.Test.Documentation.Employee();
            joe.Name = "Joe Employee";
            OpenGamingLibrary.Json.Test.Documentation.Employee mike = new OpenGamingLibrary.Json.Test.Documentation.Employee();
            mike.Name = "Mike Manager";

            joe.Manager = mike;
            mike.Manager = mike;

            string json = JsonConvert.SerializeObject(
                new[] { joe, mike },
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance
                });

            StringAssert.Equal(@"[
  {
    ""Name"": ""Joe Employee"",
    ""Manager"": {
      ""Name"": ""Mike Manager""
    }
  },
  {
    ""Name"": ""Mike Manager""
  }
]", json);
        }
    }
}

#endif