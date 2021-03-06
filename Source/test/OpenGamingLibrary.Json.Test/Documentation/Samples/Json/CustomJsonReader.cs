﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using OpenGamingLibrary.Json.Linq;
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

namespace OpenGamingLibrary.Json.Test.Documentation.Samples.Json
{
    #region Types
    public class XmlJsonReader : JsonReader
    {
        private readonly Stack<JTokenType> _stateStack;
        private readonly XmlReader _reader;

        private JTokenType? _valueType;

        public XmlJsonReader(XmlReader reader)
        {
            _reader = reader;
            _stateStack = new Stack<JTokenType>();
        }

        private JTokenType PeekState()
        {
            JTokenType current = (_stateStack.Count > 0) ? _stateStack.Peek() : JTokenType.None;
            return current;
        }

        public override bool Read()
        {
            if (HandleValueType())
                return true;

            while (_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        string typeName = _reader.GetAttribute("type");
                        if (typeName == null)
                            throw new Exception("No type specified.");

                        _valueType = (JTokenType)Enum.Parse(typeof(JTokenType), typeName, true);

                        switch (PeekState())
                        {
                            case JTokenType.None:
                                HandleValueType();
                                return true;
                            case JTokenType.Object:
                                SetToken(JsonToken.PropertyName, _reader.LocalName);
                                _stateStack.Push(JTokenType.Property);
                                return true;
                            case JTokenType.Array:
                            case JTokenType.Constructor:
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    case XmlNodeType.EndElement:
                        switch (_stateStack.Peek())
                        {
                            case JTokenType.Object:
                                SetToken(JsonToken.EndObject);
                                _stateStack.Pop();
                                if (PeekState() == JTokenType.Property)
                                    _stateStack.Pop();
                                return true;
                            case JTokenType.Array:
                                SetToken(JsonToken.EndArray);
                                _stateStack.Pop();
                                if (PeekState() == JTokenType.Property)
                                    _stateStack.Pop();
                                return true;
                            case JTokenType.Constructor:
                                SetToken(JsonToken.EndConstructor);
                                _stateStack.Pop();
                                if (PeekState() == JTokenType.Property)
                                    _stateStack.Pop();
                                return true;
                        }

                        _stateStack.Pop();
                        if (PeekState() == JTokenType.Property)
                            _stateStack.Pop();

                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        switch (_valueType)
                        {
                            case JTokenType.Integer:
                                SetToken(JsonToken.Integer, Convert.ToInt64(_reader.Value));
                                break;
                            case JTokenType.Float:
                                SetToken(JsonToken.Float, Convert.ToDouble(_reader.Value));
                                break;
                            case JTokenType.String:
                            case JTokenType.Uri:
                            case JTokenType.TimeSpan:
                            case JTokenType.Guid:
                                SetToken(JsonToken.String, _reader.Value);
                                break;
                            case JTokenType.Boolean:
                                SetToken(JsonToken.Boolean, Convert.ToBoolean(_reader.Value));
                                break;
                            case JTokenType.Date:
                                SetToken(JsonToken.Date, Convert.ToDateTime(_reader.Value));
                                break;
                            case JTokenType.Bytes:
                                SetToken(JsonToken.Bytes, Convert.FromBase64String(_reader.Value));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        _stateStack.Push(_valueType.Value);
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return false;
        }

        private bool HandleValueType()
        {
            switch (_valueType)
            {
                case JTokenType.Null:
                    SetToken(JsonToken.Null);
                    _valueType = null;

                    if (PeekState() == JTokenType.Property)
                        _stateStack.Pop();
                    return true;
                case JTokenType.Object:
                    SetToken(JsonToken.StartObject);
                    _stateStack.Push(JTokenType.Object);
                    _valueType = null;
                    return true;
                case JTokenType.Array:
                    SetToken(JsonToken.StartArray);
                    _stateStack.Push(JTokenType.Array);
                    _valueType = null;
                    return true;
                case JTokenType.Constructor:
                    string constructorName = _reader.GetAttribute("name");
                    if (constructorName == null)
                        throw new Exception("No constructor name specified.");

                    SetToken(JsonToken.StartConstructor, constructorName);
                    _stateStack.Push(JTokenType.Constructor);
                    _valueType = null;
                    return true;
            }
            return false;
        }

        public override int? ReadAsInt32()
        {
            if (!Read())
                return null;

            return (Value != null) ? (int?)Convert.ToInt32(Value) : null;
        }

        public override string ReadAsString()
        {
            if (!Read())
                return null;

            return (string)Value;
        }

        public override byte[] ReadAsBytes()
        {
            if (!Read())
                return null;

            return (byte[])Value;
        }

        public override decimal? ReadAsDecimal()
        {
            if (!Read())
                return null;

            return (Value != null) ? (decimal?)Convert.ToDecimal(Value) : null;
        }

        public override DateTime? ReadAsDateTime()
        {
            if (!Read())
                return null;

            return (Value != null) ? (DateTime?)Convert.ToDateTime(Value) : null;
        }

        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            if (!Read())
                return null;

            return (Value != null) ? (DateTimeOffset?)Convert.ToDateTime(Value) : null;
        }
    }
    #endregion

    public class CustomJsonReader
    {
        public void Example()
        {
            #region Usage
            string xml = @"<Root type=""Object"">
              <Null type=""Null"" />
              <String type=""String"">This is a string!</String>
              <Char type=""String"">!</Char>
              <Integer type=""Integer"">123</Integer>
              <DateTime type=""Date"">2001-02-22T20:59:59Z</DateTime>
              <DateTimeOffset type=""Date"">2001-02-22T20:59:59+12:00</DateTimeOffset>
              <Float type=""Float"">1.1</Float>
              <Double type=""Float"">3.14</Double>
              <Decimal type=""Float"">19.95</Decimal>
              <Guid type=""Guid"">d66eab59-3715-4b35-9e06-fa61c1216eaa</Guid>
              <Uri type=""Uri"">http://james.newtonking.com</Uri>
              <Array type=""Array"">
                <Item type=""Integer"">1</Item>
                <Item type=""Bytes"">SGVsbG8gd29ybGQh</Item>
                <Item type=""Boolean"">True</Item>
              </Array>
              <Object type=""Object"">
                <String type=""String"">This is a string!</String>
                <Null type=""Null"" />
              </Object>
              <Constructor type=""Constructor"" name=""Date"">
                <Item type=""Integer"">2000</Item>
                <Item type=""Integer"">12</Item>
                <Item type=""Integer"">30</Item>
              </Constructor>
            </Root>";

            StringReader sr = new StringReader(xml);

            using (XmlReader xmlReader = XmlReader.Create(sr, new XmlReaderSettings { IgnoreWhitespace = true }))
            using (XmlJsonReader reader = new XmlJsonReader(xmlReader))
            {
                JObject o = JObject.Load(reader);
                //{
                //  "Null": null,
                //  "String": "This is a string!",
                //  "Char": "!",
                //  "Integer": 123,
                //  "DateTime": "2001-02-23T09:59:59+13:00",
                //  "DateTimeOffset": "2001-02-22T21:59:59+13:00",
                //  "Float": 1.1,
                //  "Double": 3.14,
                //  "Decimal": 19.95,
                //  "Guid": "d66eab59-3715-4b35-9e06-fa61c1216eaa",
                //  "Uri": "http://james.newtonking.com",
                //  "Array": [
                //    1,
                //    "SGVsbG8gd29ybGQh",
                //    true
                //  ],
                //  "Object": {
                //    "String": "This is a string!",
                //    "Null": null
                //  },
                //  "Constructor": new Date(2000, 12, 30)
                //}
            }
            #endregion

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { IgnoreWhitespace = true }))
            using (XmlJsonReader reader = new XmlJsonReader(xmlReader))
            {
                Assert.True(reader.Read());
                Assert.Equal(JsonToken.StartObject, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Null", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Null, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("String", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.String, reader.TokenType);
                Assert.Equal("This is a string!", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Char", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.String, reader.TokenType);
                Assert.Equal("!", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Integer", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Integer, reader.TokenType);
                Assert.Equal(123, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("DateTime", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Date, reader.TokenType);
                Assert.Equal(DateTime.Parse("2001-02-22T20:59:59Z"), reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("DateTimeOffset", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Date, reader.TokenType);
                Assert.Equal(DateTime.Parse("2001-02-22T20:59:59+12:00"), reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Float", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Float, reader.TokenType);
                Assert.Equal(1.1d, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Double", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Float, reader.TokenType);
                Assert.Equal(3.14d, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Decimal", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Float, reader.TokenType);
                Assert.Equal(19.95d, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Guid", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.String, reader.TokenType);
                Assert.Equal("d66eab59-3715-4b35-9e06-fa61c1216eaa", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Uri", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.String, reader.TokenType);
                Assert.Equal("http://james.newtonking.com", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Array", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.StartArray, reader.TokenType);

                Assert.Equal(1, reader.ReadAsInt32());
                Assert.Equal(JsonToken.Integer, reader.TokenType);
                Assert.Equal(1, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Bytes, reader.TokenType);
                Assert.Equal(Encoding.UTF8.GetBytes("Hello world!"), reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Boolean, reader.TokenType);
                Assert.Equal(true, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.EndArray, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Object", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.StartObject, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("String", reader.Value);

                Assert.Equal("This is a string!", reader.ReadAsString());
                Assert.Equal(JsonToken.String, reader.TokenType);
                Assert.Equal("This is a string!", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Null", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Null, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.EndObject, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.PropertyName, reader.TokenType);
                Assert.Equal("Constructor", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
                Assert.Equal("Date", reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Integer, reader.TokenType);
                Assert.Equal(2000, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Integer, reader.TokenType);
                Assert.Equal(12, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.Integer, reader.TokenType);
                Assert.Equal(30, reader.Value);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

                Assert.True(reader.Read());
                Assert.Equal(JsonToken.EndObject, reader.TokenType);

                Assert.False(reader.Read());
            }
        }
    }
}