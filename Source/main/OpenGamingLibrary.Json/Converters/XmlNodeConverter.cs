// Copyright (C) 2007 James Newton-King
// Copyright (C) 2014 Extesla, LLC.
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
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenGamingLibrary.Xml;
using OpenGamingLibrary.Xml.Linq;
using OpenGamingLibrary.Json.Utilities;

namespace OpenGamingLibrary.Json.Converters
{
    /// <summary>
    /// Converts XML to and from JSON.
    /// </summary>
    public class XmlNodeConverter : JsonConverter
    {
        private const string TextName = "#text";
        private const string CommentName = "#comment";
        private const string CDataName = "#cdata-section";
        private const string WhitespaceName = "#whitespace";
        private const string SignificantWhitespaceName = "#significant-whitespace";
        private const string DeclarationName = "?xml";
        private const string JsonNamespaceUri = "http://james.newtonking.com/projects/json";

        /// <summary>
        /// Gets or sets the name of the root element to insert when deserializing to XML if the JSON structure has produces multiple root elements.
        /// </summary>
        /// <value>The name of the deserialize root element.</value>
        public string DeserializeRootElementName { get; set; }

        /// <summary>
        /// Gets or sets a flag to indicate whether to write the Json.NET array attribute.
        /// This attribute helps preserve arrays when converting the written XML back to JSON.
        /// </summary>
        /// <value><c>true</c> if the array attibute is written to the XML; otherwise, <c>false</c>.</value>
        public bool WriteArrayAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write the root JSON object.
        /// </summary>
        /// <value><c>true</c> if the JSON root object is omitted; otherwise, <c>false</c>.</value>
        public bool OmitRootObject { get; set; }

        #region Writing
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="value">The value.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IXmlNode node = WrapXml(value);

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            PushParentNamespaces(node, manager);

            if (!OmitRootObject)
                writer.WriteStartObject();

            SerializeNode(writer, node, manager, !OmitRootObject);

            if (!OmitRootObject)
                writer.WriteEndObject();
        }

        private IXmlNode WrapXml(object value)
        {
			if (value is XObject) {
				return XContainerWrapper.WrapNode((XObject)value);
			}
			if (value is XmlNode) {
				return XmlNodeWrapper.WrapNode((XmlNode)value);
			}
            throw new ArgumentException("Value must be an XML object.", "value");
        }

        private void PushParentNamespaces(IXmlNode node, XmlNamespaceManager manager)
        {
            List<IXmlNode> parentElements = null;

            IXmlNode parent = node;
            while ((parent = parent.ParentNode) != null)
            {
                if (parent.NodeType == XmlNodeType.Element)
                {
                    if (parentElements == null)
                        parentElements = new List<IXmlNode>();

                    parentElements.Add(parent);
                }
            }

            if (parentElements != null)
            {
                parentElements.Reverse();

                foreach (IXmlNode parentElement in parentElements)
                {
                    manager.PushScope();
                    foreach (IXmlNode attribute in parentElement.Attributes)
                    {
                        if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/" && attribute.LocalName != "xmlns")
                            manager.AddNamespace(attribute.LocalName, attribute.Value);
                    }
                }
            }
        }

        private string ResolveFullName(IXmlNode node, XmlNamespaceManager manager)
        {
            string prefix = (node.NamespaceUri == null || (node.LocalName == "xmlns" && node.NamespaceUri == "http://www.w3.org/2000/xmlns/"))
                ? null
                : manager.LookupPrefix(node.NamespaceUri);

            if (!string.IsNullOrEmpty(prefix))
                return prefix + ":" + node.LocalName;
            else
                return node.LocalName;
        }

        private string GetPropertyName(IXmlNode node, XmlNamespaceManager manager)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Attribute:
                    if (node.NamespaceUri == JsonNamespaceUri)
                        return "$" + node.LocalName;
                    else
                        return "@" + ResolveFullName(node, manager);
                case XmlNodeType.CDATA:
                    return CDataName;
                case XmlNodeType.Comment:
                    return CommentName;
                case XmlNodeType.Element:
                    return ResolveFullName(node, manager);
                case XmlNodeType.ProcessingInstruction:
                    return "?" + ResolveFullName(node, manager);
                case XmlNodeType.DocumentType:
                    return "!" + ResolveFullName(node, manager);
                case XmlNodeType.XmlDeclaration:
                    return DeclarationName;
                case XmlNodeType.SignificantWhitespace:
                    return SignificantWhitespaceName;
                case XmlNodeType.Text:
                    return TextName;
                case XmlNodeType.Whitespace:
                    return WhitespaceName;
                default:
                    throw new JsonSerializationException("Unexpected XmlNodeType when getting node name: " + node.NodeType);
            }
        }

        private bool IsArray(IXmlNode node)
        {
            IXmlNode jsonArrayAttribute = (node.Attributes != null)
                ? node.Attributes.SingleOrDefault(a => a.LocalName == "Array" && a.NamespaceUri == JsonNamespaceUri)
                : null;

            return (jsonArrayAttribute != null && XmlConvert.ToBoolean(jsonArrayAttribute.Value));
        }

        private void SerializeGroupedNodes(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
        {
            // group nodes together by name
            Dictionary<string, List<IXmlNode>> nodesGroupedByName = new Dictionary<string, List<IXmlNode>>();

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                IXmlNode childNode = node.ChildNodes[i];
                string nodeName = GetPropertyName(childNode, manager);

                List<IXmlNode> nodes;
                if (!nodesGroupedByName.TryGetValue(nodeName, out nodes))
                {
                    nodes = new List<IXmlNode>();
                    nodesGroupedByName.Add(nodeName, nodes);
                }

                nodes.Add(childNode);
            }

            // loop through grouped nodes. write single name instances as normal,
            // write multiple names together in an array
            foreach (KeyValuePair<string, List<IXmlNode>> nodeNameGroup in nodesGroupedByName)
            {
                List<IXmlNode> groupedNodes = nodeNameGroup.Value;
                bool writeArray;

                if (groupedNodes.Count == 1)
                {
                    writeArray = IsArray(groupedNodes[0]);
                }
                else
                {
                    writeArray = true;
                }

                if (!writeArray)
                {
                    SerializeNode(writer, groupedNodes[0], manager, writePropertyName);
                }
                else
                {
                    string elementNames = nodeNameGroup.Key;

                    if (writePropertyName)
                        writer.WritePropertyName(elementNames);

                    writer.WriteStartArray();

                    for (int i = 0; i < groupedNodes.Count; i++)
                    {
                        SerializeNode(writer, groupedNodes[i], manager, false);
                    }

                    writer.WriteEndArray();
                }
            }
        }

        private void SerializeNode(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    SerializeGroupedNodes(writer, node, manager, writePropertyName);
                    break;
                case XmlNodeType.Element:
                    if (IsArray(node) && node.ChildNodes.All(n => n.LocalName == node.LocalName) && node.ChildNodes.Count > 0)
                    {
                        SerializeGroupedNodes(writer, node, manager, false);
                    }
                    else
                    {
                        manager.PushScope();

                        foreach (IXmlNode attribute in node.Attributes)
                        {
                            if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            {
                                string namespacePrefix = (attribute.LocalName != "xmlns")
                                    ? attribute.LocalName
                                    : string.Empty;
                                string namespaceUri = attribute.Value;

                                manager.AddNamespace(namespacePrefix, namespaceUri);
                            }
                        }

                        if (writePropertyName)
                            writer.WritePropertyName(GetPropertyName(node, manager));

                        if (!ValueAttributes(node.Attributes).Any() && node.ChildNodes.Count == 1
                            && node.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            // write elements with a single text child as a name value pair
                            writer.WriteValue(node.ChildNodes[0].Value);
                        }
                        else if (node.ChildNodes.Count == 0 && CollectionUtils.IsNullOrEmpty(node.Attributes))
                        {
                            IXmlElement element = (IXmlElement)node;

                            // empty element
                            if (element.IsEmpty)
                                writer.WriteNull();
                            else
                                writer.WriteValue(string.Empty);
                        }
                        else
                        {
                            writer.WriteStartObject();

                            for (int i = 0; i < node.Attributes.Count; i++)
                            {
                                SerializeNode(writer, node.Attributes[i], manager, true);
                            }

                            SerializeGroupedNodes(writer, node, manager, true);

                            writer.WriteEndObject();
                        }

                        manager.PopScope();
                    }

                    break;
                case XmlNodeType.Comment:
                    if (writePropertyName)
                        writer.WriteComment(node.Value);
                    break;
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    if (node.NamespaceUri == "http://www.w3.org/2000/xmlns/" && node.Value == JsonNamespaceUri)
                        return;

                    if (node.NamespaceUri == JsonNamespaceUri)
                    {
                        if (node.LocalName == "Array")
                            return;
                    }

                    if (writePropertyName)
                        writer.WritePropertyName(GetPropertyName(node, manager));
                    writer.WriteValue(node.Value);
                    break;
                case XmlNodeType.XmlDeclaration:
                    IXmlDeclaration declaration = (IXmlDeclaration)node;
                    writer.WritePropertyName(GetPropertyName(node, manager));
                    writer.WriteStartObject();

                    if (!string.IsNullOrEmpty(declaration.Version))
                    {
                        writer.WritePropertyName("@version");
                        writer.WriteValue(declaration.Version);
                    }
                    if (!string.IsNullOrEmpty(declaration.Encoding))
                    {
                        writer.WritePropertyName("@encoding");
                        writer.WriteValue(declaration.Encoding);
                    }
                    if (!string.IsNullOrEmpty(declaration.Standalone))
                    {
                        writer.WritePropertyName("@standalone");
                        writer.WriteValue(declaration.Standalone);
                    }

                    writer.WriteEndObject();
                    break;
                case XmlNodeType.DocumentType:
                    IXmlDocumentType documentType = (IXmlDocumentType)node;
                    writer.WritePropertyName(GetPropertyName(node, manager));
                    writer.WriteStartObject();

                    if (!string.IsNullOrEmpty(documentType.Name))
                    {
                        writer.WritePropertyName("@name");
                        writer.WriteValue(documentType.Name);
                    }
                    if (!string.IsNullOrEmpty(documentType.Public))
                    {
                        writer.WritePropertyName("@public");
                        writer.WriteValue(documentType.Public);
                    }
                    if (!string.IsNullOrEmpty(documentType.System))
                    {
                        writer.WritePropertyName("@system");
                        writer.WriteValue(documentType.System);
                    }
                    if (!string.IsNullOrEmpty(documentType.InternalSubset))
                    {
                        writer.WritePropertyName("@internalSubset");
                        writer.WriteValue(documentType.InternalSubset);
                    }

                    writer.WriteEndObject();
                    break;
                default:
                    throw new JsonSerializationException("Unexpected XmlNodeType when serializing nodes: " + node.NodeType);
            }
        }
        #endregion

        #region Reading
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            IXmlDocument document = null;
            IXmlNode rootNode = null;

#if !NET20
            if (typeof(XObject).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XDocument) && objectType != typeof(XElement))
                    throw new JsonSerializationException("XmlNodeConverter only supports deserializing XDocument or XElement.");

                XDocument d = new XDocument();
                document = new XDocumentWrapper(d);
                rootNode = document;
            }
#endif
#if !(NETFX_CORE || PORTABLE)
            if (typeof(XmlNode).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XmlDocument))
                    throw new JsonSerializationException("XmlNodeConverter only supports deserializing XmlDocuments");

                XmlDocument d = new XmlDocument();
                // prevent http request when resolving any DTD references
                d.XmlResolver = null;

                document = new XmlDocumentWrapper(d);
                rootNode = document;
            }
#endif

            if (document == null || rootNode == null)
                throw new JsonSerializationException("Unexpected type when converting XML: " + objectType);

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("XmlNodeConverter can only convert JSON that begins with an object.");

            if (!string.IsNullOrEmpty(DeserializeRootElementName))
            {
                //rootNode = document.CreateElement(DeserializeRootElementName);
                //document.AppendChild(rootNode);
                ReadElement(reader, document, rootNode, DeserializeRootElementName, manager);
            }
            else
            {
                reader.Read();
                DeserializeNode(reader, document, manager, rootNode);
            }

#if !NET20
            if (objectType == typeof(XElement))
            {
                XElement element = (XElement)document.DocumentElement.WrappedNode;
                element.Remove();

                return element;
            }
#endif

            return document.WrappedNode;
        }

        private void DeserializeValue(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, string propertyName, IXmlNode currentNode)
        {
            switch (propertyName)
            {
                case TextName:
                    currentNode.AppendChild(document.CreateTextNode(reader.Value.ToString()));
                    break;
                case CDataName:
                    currentNode.AppendChild(document.CreateCDataSection(reader.Value.ToString()));
                    break;
                case WhitespaceName:
                    currentNode.AppendChild(document.CreateWhitespace(reader.Value.ToString()));
                    break;
                case SignificantWhitespaceName:
                    currentNode.AppendChild(document.CreateSignificantWhitespace(reader.Value.ToString()));
                    break;
                default:
                    // processing instructions and the xml declaration start with ?
                    if (!string.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
                    {
                        CreateInstruction(reader, document, currentNode, propertyName);
                    }
                    else if (string.Equals(propertyName, "!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        CreateDocumentType(reader, document, currentNode);
                    }
                    else
                    {
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            // handle nested arrays
                            ReadArrayElements(reader, document, propertyName, currentNode, manager);
                            return;
                        }

                        // have to wait until attributes have been parsed before creating element
                        // attributes may contain namespace info used by the element
                        ReadElement(reader, document, currentNode, propertyName, manager);
                    }
                    break;
            }
        }

        private void ReadElement(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, XmlNamespaceManager manager)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new JsonSerializationException("XmlNodeConverter cannot convert JSON with an empty property name to XML.");

            Dictionary<string, string> attributeNameValues = ReadAttributeElements(reader, manager);

            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            if (propertyName.StartsWith('@'))
            {
                string attributeName = propertyName.Substring(1);
                string attributeValue = reader.Value.ToString();

                string attributePrefix = MiscellaneousUtils.GetPrefix(attributeName);

                IXmlNode attribute = (!string.IsNullOrEmpty(attributePrefix))
                    ? document.CreateAttribute(attributeName, manager.LookupNamespace(attributePrefix), attributeValue)
                    : document.CreateAttribute(attributeName, attributeValue);

                ((IXmlElement)currentNode).SetAttributeNode(attribute);
            }
            else
            {
                IXmlElement element = CreateElement(propertyName, document, elementPrefix, manager);

                currentNode.AppendChild(element);

                // add attributes to newly created element
                foreach (KeyValuePair<string, string> nameValue in attributeNameValues)
                {
                    string attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

                    IXmlNode attribute = (!string.IsNullOrEmpty(attributePrefix))
                        ? document.CreateAttribute(nameValue.Key, manager.LookupNamespace(attributePrefix), nameValue.Value)
                        : document.CreateAttribute(nameValue.Key, nameValue.Value);

                    element.SetAttributeNode(attribute);
                }

                if (reader.TokenType == JsonToken.String
                    || reader.TokenType == JsonToken.Integer
                    || reader.TokenType == JsonToken.Float
                    || reader.TokenType == JsonToken.Boolean
                    || reader.TokenType == JsonToken.Date)
                {
                    element.AppendChild(document.CreateTextNode(ConvertTokenToXmlValue(reader)));
                }
                else if (reader.TokenType == JsonToken.Null)
                {
                    // empty element. do nothing
                }
                else
                {
                    // finished element will have no children to deserialize
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        manager.PushScope();
                        DeserializeNode(reader, document, manager, element);
                        manager.PopScope();
                    }

                    manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                }
            }
        }

        private string ConvertTokenToXmlValue(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return reader.Value.ToString();
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return XmlConvert.ToString(Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture));
            }
            else if (reader.TokenType == JsonToken.Float)
            {
                if (reader.Value is decimal)
                    return XmlConvert.ToString((decimal)reader.Value);
                if (reader.Value is float)
                    return XmlConvert.ToString((float)reader.Value);
                
                return XmlConvert.ToString(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
            }
            else if (reader.TokenType == JsonToken.Boolean)
            {
                return XmlConvert.ToString(Convert.ToBoolean(reader.Value, CultureInfo.InvariantCulture));
            }
            else if (reader.TokenType == JsonToken.Date)
            {
#if !NET20
                if (reader.Value is DateTimeOffset)
                    return XmlConvert.ToString((DateTimeOffset)reader.Value);
#endif

                DateTime d = Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture);
#if !(NETFX_CORE || PORTABLE)
                return XmlConvert.ToString(d, DateTimeUtils.ToSerializationMode(d.Kind));
#else
                return XmlConvert.ToString(d);
#endif
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Cannot get an XML string value from token type '{0}'.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private void ReadArrayElements(JsonReader reader, IXmlDocument document, string propertyName, IXmlNode currentNode, XmlNamespaceManager manager)
        {
            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            IXmlElement nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

            currentNode.AppendChild(nestedArrayElement);

            int count = 0;
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                DeserializeValue(reader, document, manager, propertyName, nestedArrayElement);
                count++;
            }

            if (WriteArrayAttribute)
            {
                AddJsonArrayAttribute(nestedArrayElement, document);
            }

            if (count == 1 && WriteArrayAttribute)
            {
                IXmlElement arrayElement = nestedArrayElement.ChildNodes.OfType<IXmlElement>().Single(n => n.LocalName == propertyName);
                AddJsonArrayAttribute(arrayElement, document);
            }
        }

        private void AddJsonArrayAttribute(IXmlElement element, IXmlDocument document)
        {
            element.SetAttributeNode(document.CreateAttribute("json:Array", JsonNamespaceUri, "true"));

#if !NET20
            // linq to xml doesn't automatically include prefixes via the namespace manager
            if (element is XElementWrapper)
            {
                if (element.GetPrefixOfNamespace(JsonNamespaceUri) == null)
                {
                    element.SetAttributeNode(document.CreateAttribute("xmlns:json", "http://www.w3.org/2000/xmlns/", JsonNamespaceUri));
                }
            }
#endif
        }

        private Dictionary<string, string> ReadAttributeElements(JsonReader reader, XmlNamespaceManager manager)
        {
            Dictionary<string, string> attributeNameValues = new Dictionary<string, string>();
            bool finishedAttributes = false;
            bool finishedElement = false;

            // a string token means the element only has a single text child
            if (reader.TokenType != JsonToken.String
                && reader.TokenType != JsonToken.Null
                && reader.TokenType != JsonToken.Boolean
                && reader.TokenType != JsonToken.Integer
                && reader.TokenType != JsonToken.Float
                && reader.TokenType != JsonToken.Date
                && reader.TokenType != JsonToken.StartConstructor)
            {
                // read properties until first non-attribute is encountered
                while (!finishedAttributes && !finishedElement && reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            string attributeName = reader.Value.ToString();

                            if (!string.IsNullOrEmpty(attributeName))
                            {
                                char firstChar = attributeName[0];
                                string attributeValue;

                                switch (firstChar)
                                {
                                    case '@':
                                        attributeName = attributeName.Substring(1);
                                        reader.Read();
                                        attributeValue = ConvertTokenToXmlValue(reader);
                                        attributeNameValues.Add(attributeName, attributeValue);

                                        string namespacePrefix;
                                        if (IsNamespaceAttribute(attributeName, out namespacePrefix))
                                        {
                                            manager.AddNamespace(namespacePrefix, attributeValue);
                                        }
                                        break;
                                    case '$':
                                        attributeName = attributeName.Substring(1);
                                        reader.Read();
                                        attributeValue = reader.Value.ToString();

                                        // check that JsonNamespaceUri is in scope
                                        // if it isn't then add it to document and namespace manager
                                        string jsonPrefix = manager.LookupPrefix(JsonNamespaceUri);
                                        if (jsonPrefix == null)
                                        {
                                            // ensure that the prefix used is free
                                            int? i = null;
                                            while (manager.LookupNamespace("json" + i) != null)
                                            {
                                                i = i.GetValueOrDefault() + 1;
                                            }
                                            jsonPrefix = "json" + i;

                                            attributeNameValues.Add("xmlns:" + jsonPrefix, JsonNamespaceUri);
                                            manager.AddNamespace(jsonPrefix, JsonNamespaceUri);
                                        }

                                        attributeNameValues.Add(jsonPrefix + ":" + attributeName, attributeValue);
                                        break;
                                    default:
                                        finishedAttributes = true;
                                        break;
                                }
                            }
                            else
                            {
                               
                                finishedAttributes = true;
                            }

                            break;
                        case JsonToken.EndObject:
                            finishedElement = true;
                            break;
                        default:
                            throw new JsonSerializationException("Unexpected JsonToken: " + reader.TokenType);
                    }
                }
            }

            return attributeNameValues;
        }

        private void CreateInstruction(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName)
        {
            if (propertyName == DeclarationName)
            {
                string version = null;
                string encoding = null;
                string standalone = null;
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    switch (reader.Value.ToString())
                    {
                        case "@version":
                            reader.Read();
                            version = reader.Value.ToString();
                            break;
                        case "@encoding":
                            reader.Read();
                            encoding = reader.Value.ToString();
                            break;
                        case "@standalone":
                            reader.Read();
                            standalone = reader.Value.ToString();
                            break;
                        default:
                            throw new JsonSerializationException("Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                    }
                }

                IXmlNode declaration = document.CreateXmlDeclaration(version, encoding, standalone);
                currentNode.AppendChild(declaration);
            }
            else
            {
                IXmlNode instruction = document.CreateProcessingInstruction(propertyName.Substring(1), reader.Value.ToString());
                currentNode.AppendChild(instruction);
            }
        }

        private void CreateDocumentType(JsonReader reader, IXmlDocument document, IXmlNode currentNode)
        {
            string name = null;
            string publicId = null;
            string systemId = null;
            string internalSubset = null;
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value.ToString())
                {
                    case "@name":
                        reader.Read();
                        name = reader.Value.ToString();
                        break;
                    case "@public":
                        reader.Read();
                        publicId = reader.Value.ToString();
                        break;
                    case "@system":
                        reader.Read();
                        systemId = reader.Value.ToString();
                        break;
                    case "@internalSubset":
                        reader.Read();
                        internalSubset = reader.Value.ToString();
                        break;
                    default:
                        throw new JsonSerializationException("Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                }
            }

            IXmlNode documentType = document.CreateXmlDocumentType(name, publicId, systemId, internalSubset);
            currentNode.AppendChild(documentType);
        }

        private IXmlElement CreateElement(string elementName, IXmlDocument document, string elementPrefix, XmlNamespaceManager manager)
        {
            string ns = string.IsNullOrEmpty(elementPrefix) ? manager.DefaultNamespace : manager.LookupNamespace(elementPrefix);

            IXmlElement element = (!string.IsNullOrEmpty(ns)) ? document.CreateElement(elementName, ns) : document.CreateElement(elementName);

            return element;
        }

        private void DeserializeNode(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode)
        {
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
                            throw new JsonSerializationException("JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.");

                        string propertyName = reader.Value.ToString();
                        reader.Read();

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            int count = 0;
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                DeserializeValue(reader, document, manager, propertyName, currentNode);
                                count++;
                            }

                            if (count == 1 && WriteArrayAttribute)
                            {
                                IXmlElement arrayElement = currentNode.ChildNodes.OfType<IXmlElement>().Single(n => n.LocalName == propertyName);
                                AddJsonArrayAttribute(arrayElement, document);
                            }
                        }
                        else
                        {
                            DeserializeValue(reader, document, manager, propertyName, currentNode);
                        }
                        break;
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        while (reader.Read() && reader.TokenType != JsonToken.EndConstructor)
                        {
                            DeserializeValue(reader, document, manager, constructorName, currentNode);
                        }
                        break;
                    case JsonToken.Comment:
                        currentNode.AppendChild(document.CreateComment((string)reader.Value));
                        break;
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        return;
                    default:
                        throw new JsonSerializationException("Unexpected JsonToken when deserializing node: " + reader.TokenType);
                }
            } while (reader.TokenType == JsonToken.PropertyName || reader.Read());
            // don't read if current token is a property. token was already read when parsing element attributes
        }

        /// <summary>
        /// Checks if the attributeName is a namespace attribute.
        /// </summary>
        /// <param name="attributeName">Attribute name to test.</param>
        /// <param name="prefix">The attribute name prefix if it has one, otherwise an empty string.</param>
        /// <returns>True if attribute name is for a namespace attribute, otherwise false.</returns>
        private bool IsNamespaceAttribute(string attributeName, out string prefix)
        {
            if (attributeName.StartsWith("xmlns", StringComparison.Ordinal))
            {
                if (attributeName.Length == 5)
                {
                    prefix = string.Empty;
                    return true;
                }
                else if (attributeName[5] == ':')
                {
                    prefix = attributeName.Substring(6, attributeName.Length - 6);
                    return true;
                }
            }
            prefix = null;
            return false;
        }

        private IEnumerable<IXmlNode> ValueAttributes(IEnumerable<IXmlNode> c)
        {
            return c.Where(a => a.NamespaceUri != JsonNamespaceUri);
        }
        #endregion

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
            if (typeof(XObject).IsAssignableFrom(valueType))
                return true;
            if (typeof(XmlNode).IsAssignableFrom(valueType))
                return true;
            return false;
        }
    }
}