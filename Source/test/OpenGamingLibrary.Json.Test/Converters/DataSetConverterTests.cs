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
#if !(NETFX_CORE || PORTABLE || ASPNETCORE50 || PORTABLE40)
using System;
using OpenGamingLibrary.Json.Converters;
using Xunit;
using OpenGamingLibrary.Json.Serialization;
using OpenGamingLibrary.Json.Test.TestObjects;
using System.Data;
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Converters
{
    public class DataSetConverterTests : TestFixtureBase
    {
        [Fact]
        public void DeserializeInvalidDataTable()
        {
            AssertException.Throws<JsonException>(() => JsonConvert.DeserializeObject<DataSet>("{\"pending_count\":23,\"completed_count\":45}"), "Unexpected JSON token when reading DataTable. Expected StartArray, got Integer. Path 'pending_count', line 1, position 19.");
        }

        [Fact]
        public void SerializeAndDeserialize()
        {
            DataSet dataSet = new DataSet("dataSet");
            dataSet.Namespace = "NetFrameWork";
            DataTable table = new DataTable();
            DataColumn idColumn = new DataColumn("id", typeof(int));
            idColumn.AutoIncrement = true;

            DataColumn itemColumn = new DataColumn("item");
            table.Columns.Add(idColumn);
            table.Columns.Add(itemColumn);
            dataSet.Tables.Add(table);

            for (int i = 0; i < 2; i++)
            {
                DataRow newRow = table.NewRow();
                newRow["item"] = "item " + i;
                table.Rows.Add(newRow);
            }

            dataSet.AcceptChanges();

            string json = JsonConvert.SerializeObject(dataSet, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Table1"": [
    {
      ""id"": 0,
      ""item"": ""item 0""
    },
    {
      ""id"": 1,
      ""item"": ""item 1""
    }
  ]
}", json);

            DataSet deserializedDataSet = JsonConvert.DeserializeObject<DataSet>(json);
            Assert.NotNull(deserializedDataSet);

            Assert.Equal(1, deserializedDataSet.Tables.Count);

            DataTable dt = deserializedDataSet.Tables[0];

            Assert.Equal("Table1", dt.TableName);
            Assert.Equal(2, dt.Columns.Count);
            Assert.Equal("id", dt.Columns[0].ColumnName);
            Assert.Equal(typeof(long), dt.Columns[0].DataType);
            Assert.Equal("item", dt.Columns[1].ColumnName);
            Assert.Equal(typeof(string), dt.Columns[1].DataType);

            Assert.Equal(2, dt.Rows.Count);
        }

        [Fact]
        public void SerializeMultiTableDataSet()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(CreateDataTable("FirstTable", 2));
            ds.Tables.Add(CreateDataTable("SecondTable", 1));

            string json = JsonConvert.SerializeObject(ds, Formatting.Indented, new IsoDateTimeConverter());
            // {
            //   "FirstTable": [
            //     {
            //       "StringCol": "Item Name",
            //       "Int32Col": 1,
            //       "BooleanCol": true,
            //       "TimeSpanCol": "10.22:10:15.1000000",
            //       "DateTimeCol": "2000-12-29T00:00:00Z",
            //       "DecimalCol": 64.0021
            //     },
            //     {
            //       "StringCol": "Item Name",
            //       "Int32Col": 2,
            //       "BooleanCol": true,
            //       "TimeSpanCol": "10.22:10:15.1000000",
            //       "DateTimeCol": "2000-12-29T00:00:00Z",
            //       "DecimalCol": 64.0021
            //     }
            //   ],
            //   "SecondTable": [
            //     {
            //       "StringCol": "Item Name",
            //       "Int32Col": 1,
            //       "BooleanCol": true,
            //       "TimeSpanCol": "10.22:10:15.1000000",
            //       "DateTimeCol": "2000-12-29T00:00:00Z",
            //       "DecimalCol": 64.0021
            //     }
            //   ]
            // }

            DataSet deserializedDs = JsonConvert.DeserializeObject<DataSet>(json, new IsoDateTimeConverter());

            StringAssert.Equal(@"{
  ""FirstTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    },
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""SecondTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ]
}", json);

            Assert.NotNull(deserializedDs);
        }

        [Fact]
        public void DeserializeMultiTableDataSet()
        {
            string json = @"{
  ""FirstTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2147483647,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""SecondTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2147483647,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ]
}";

            DataSet ds = JsonConvert.DeserializeObject<DataSet>(json);
            Assert.NotNull(ds);

            Assert.Equal(2, ds.Tables.Count);
            Assert.Equal("FirstTable", ds.Tables[0].TableName);
            Assert.Equal("SecondTable", ds.Tables[1].TableName);

            DataTable dt = ds.Tables[0];
            Assert.Equal("StringCol", dt.Columns[0].ColumnName);
            Assert.Equal(typeof(string), dt.Columns[0].DataType);
            Assert.Equal("Int32Col", dt.Columns[1].ColumnName);
            Assert.Equal(typeof(long), dt.Columns[1].DataType);
            Assert.Equal("BooleanCol", dt.Columns[2].ColumnName);
            Assert.Equal(typeof(bool), dt.Columns[2].DataType);
            Assert.Equal("TimeSpanCol", dt.Columns[3].ColumnName);
            Assert.Equal(typeof(string), dt.Columns[3].DataType);
            Assert.Equal("DateTimeCol", dt.Columns[4].ColumnName);
            Assert.Equal(typeof(DateTime), dt.Columns[4].DataType);
            Assert.Equal("DecimalCol", dt.Columns[5].ColumnName);
            Assert.Equal(typeof(double), dt.Columns[5].DataType);

            Assert.Equal(1, ds.Tables[0].Rows.Count);
            Assert.Equal(1, ds.Tables[1].Rows.Count);
        }

        private DataTable CreateDataTable(string dataTableName, int rows)
        {
            // create a new DataTable.
            DataTable myTable = new DataTable(dataTableName);

            // create DataColumn objects of data types.
            DataColumn colString = new DataColumn("StringCol");
            colString.DataType = typeof(string);
            myTable.Columns.Add(colString);

            DataColumn colInt32 = new DataColumn("Int32Col");
            colInt32.DataType = typeof(int);
            myTable.Columns.Add(colInt32);

            DataColumn colBoolean = new DataColumn("BooleanCol");
            colBoolean.DataType = typeof(bool);
            myTable.Columns.Add(colBoolean);

            DataColumn colTimeSpan = new DataColumn("TimeSpanCol");
            colTimeSpan.DataType = typeof(TimeSpan);
            myTable.Columns.Add(colTimeSpan);

            DataColumn colDateTime = new DataColumn("DateTimeCol");
            colDateTime.DataType = typeof(DateTime);
            colDateTime.DateTimeMode = DataSetDateTime.Utc;
            myTable.Columns.Add(colDateTime);

            DataColumn colDecimal = new DataColumn("DecimalCol");
            colDecimal.DataType = typeof(decimal);
            myTable.Columns.Add(colDecimal);

            for (int i = 1; i <= rows; i++)
            {
                DataRow myNewRow = myTable.NewRow();

                myNewRow["StringCol"] = "Item Name";
                myNewRow["Int32Col"] = i;
                myNewRow["BooleanCol"] = true;
                myNewRow["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);
                myNewRow["DateTimeCol"] = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc);
                myNewRow["DecimalCol"] = 64.0021;
                myTable.Rows.Add(myNewRow);
            }

            return myTable;
        }

        public class DataSetAndTableTestClass
        {
            public string Before { get; set; }
            public DataSet Set { get; set; }
            public string Middle { get; set; }
            public DataTable Table { get; set; }
            public string After { get; set; }
        }

        [Fact]
        public void SerializeWithCamelCaseResolver()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(CreateDataTable("FirstTable", 2));
            ds.Tables.Add(CreateDataTable("SecondTable", 1));

            string json = JsonConvert.SerializeObject(ds, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            StringAssert.Equal(@"{
  ""firstTable"": [
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 1,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    },
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 2,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    }
  ],
  ""secondTable"": [
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 1,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    }
  ]
}", json);
        }

        [Fact]
        public void SerializeDataSetProperty()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(CreateDataTable("FirstTable", 2));
            ds.Tables.Add(CreateDataTable("SecondTable", 1));

            DataSetAndTableTestClass c = new DataSetAndTableTestClass
            {
                Before = "Before",
                Set = ds,
                Middle = "Middle",
                Table = CreateDataTable("LoneTable", 2),
                After = "After"
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new IsoDateTimeConverter());

            StringAssert.Equal(@"{
  ""Before"": ""Before"",
  ""Set"": {
    ""FirstTable"": [
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 1,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      },
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 2,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      }
    ],
    ""SecondTable"": [
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 1,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      }
    ]
  },
  ""Middle"": ""Middle"",
  ""Table"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    },
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""After"": ""After""
}", json);

            DataSetAndTableTestClass c2 = JsonConvert.DeserializeObject<DataSetAndTableTestClass>(json, new IsoDateTimeConverter());

            Assert.Equal(c.Before, c2.Before);
            Assert.Equal(c.Set.Tables.Count, c2.Set.Tables.Count);
            Assert.Equal(c.Middle, c2.Middle);
            Assert.Equal(c.Table.Rows.Count, c2.Table.Rows.Count);
            Assert.Equal(c.After, c2.After);
        }

        [Fact]
        public void SerializedTypedDataSet()
        {
            CustomerDataSet ds = new CustomerDataSet();
            ds.Customers.AddCustomersRow("234");

            string json = JsonConvert.SerializeObject(ds, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}", json);

            CustomerDataSet ds1 = new CustomerDataSet();
            DataTable table = ds1.Tables["Customers"];
            DataRow row = ds1.Tables["Customers"].NewRow();
            row["CustomerID"] = "234";

            table.Rows.Add(row);

            string json1 = JsonConvert.SerializeObject(ds1, Formatting.Indented);

            StringAssert.Equal(@"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}", json1);
        }

        [Fact]
        public void DeserializedTypedDataSet()
        {
            string json = @"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}";

            var ds = JsonConvert.DeserializeObject<CustomerDataSet>(json);

            Assert.Equal("234", ds.Customers[0].CustomerID);
        }
    }
}

#endif