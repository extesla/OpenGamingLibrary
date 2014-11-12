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
using System.Xml;

namespace OpenGamingLibrary.Xml
{
	public class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument
	{
		private readonly XmlDocument _document;

		public XmlDocumentWrapper(XmlDocument document)
			: base(document)
		{
			_document = document;
		}

		public IXmlNode CreateComment(string data)
		{
			return new XmlNodeWrapper(_document.CreateComment(data));
		}

		public IXmlNode CreateTextNode(string text)
		{
			return new XmlNodeWrapper(_document.CreateTextNode(text));
		}

		public IXmlNode CreateCDataSection(string data)
		{
			return new XmlNodeWrapper(_document.CreateCDataSection(data));
		}

		public IXmlNode CreateWhitespace(string text)
		{
			return new XmlNodeWrapper(_document.CreateWhitespace(text));
		}

		public IXmlNode CreateSignificantWhitespace(string text)
		{
			return new XmlNodeWrapper(_document.CreateSignificantWhitespace(text));
		}

		public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new XmlDeclarationWrapper(_document.CreateXmlDeclaration(version, encoding, standalone));
		}

		public IXmlNode CreateXmlDocumentType(string name, string publicId, string systemId, string internalSubset)
		{
			return new XmlDocumentTypeWrapper(_document.CreateDocumentType(name, publicId, systemId, null));
		}

		public IXmlNode CreateProcessingInstruction(string target, string data)
		{
			return new XmlNodeWrapper(_document.CreateProcessingInstruction(target, data));
		}

		public IXmlElement CreateElement(string elementName)
		{
			return new XmlElementWrapper(_document.CreateElement(elementName));
		}

		public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
		{
			return new XmlElementWrapper(_document.CreateElement(qualifiedName, namespaceUri));
		}

		public IXmlNode CreateAttribute(string name, string value)
		{
			XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(name));
			attribute.Value = value;

			return attribute;
		}

		public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value)
		{
			XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(qualifiedName, namespaceUri));
			attribute.Value = value;

			return attribute;
		}

		public IXmlElement DocumentElement
		{
			get
			{
				if (_document.DocumentElement == null)
					return null;

				return new XmlElementWrapper(_document.DocumentElement);
			}
		}
	}
}

