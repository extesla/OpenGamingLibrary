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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace OpenGamingLibrary.Xml.Linq
{
	public class XDocumentWrapper : XContainerWrapper, IXmlDocument
	{
		private XDocument Document
		{
			get { return (XDocument)WrappedNode; }
		}

		public XDocumentWrapper(XDocument document)
			: base(document)
		{
		}

		public override IList<IXmlNode> ChildNodes
		{
			get
			{
				IList<IXmlNode> childNodes = base.ChildNodes;

				if (Document.Declaration != null && childNodes[0].NodeType != XmlNodeType.XmlDeclaration)
					childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));

				return childNodes;
			}
		}

		public IXmlNode CreateComment(string text)
		{
			return new XObjectWrapper(new XComment(text));
		}

		public IXmlNode CreateTextNode(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateCDataSection(string data)
		{
			return new XObjectWrapper(new XCData(data));
		}

		public IXmlNode CreateWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateSignificantWhitespace(string text)
		{
			return new XObjectWrapper(new XText(text));
		}

		public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
		}

		public IXmlNode CreateXmlDocumentType(string name, string publicId, string systemId, string internalSubset)
		{
			return new XDocumentTypeWrapper(new XDocumentType(name, publicId, systemId, internalSubset));
		}

		public IXmlNode CreateProcessingInstruction(string target, string data)
		{
			return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
		}

		public IXmlElement CreateElement(string elementName)
		{
			return new XElementWrapper(new XElement(elementName));
		}

		public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
		{
			string localName = GetLocalName(qualifiedName);
			return new XElementWrapper(new XElement(XName.Get(localName, namespaceUri)));
		}

		public IXmlNode CreateAttribute(string name, string value)
		{
			return new XAttributeWrapper(new XAttribute(name, value));
		}

		public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value)
		{
			string localName = GetLocalName(qualifiedName);
			return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceUri), value));
		}

		public IXmlElement DocumentElement
		{
			get
			{
				if (Document.Root == null)
					return null;

				return new XElementWrapper(Document.Root);
			}
		}

		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			var declarationWrapper = newChild as XDeclarationWrapper;
			if (declarationWrapper != null)
			{
				Document.Declaration = declarationWrapper.Declaration;
				return declarationWrapper;
			}
			else
			{
				return base.AppendChild(newChild);
			}
		}
	}
}

