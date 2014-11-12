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
using System.Xml;
using System.Xml.Linq;
using OpenGamingLibrary.Xml;

namespace OpenGamingLibrary.Xml.Linq
{
	public class XObjectWrapper : IXmlNode
	{
		private readonly XObject _xmlObject;

		public XObjectWrapper(XObject xmlObject)
		{
			_xmlObject = xmlObject;
		}

		public object WrappedNode
		{
			get { return _xmlObject; }
		}

		public virtual XmlNodeType NodeType
		{
			get { return _xmlObject.NodeType; }
		}

		public virtual string LocalName
		{
			get { return null; }
		}

		public virtual IList<IXmlNode> ChildNodes
		{
			get { return new List<IXmlNode>(); }
		}

		public virtual IList<IXmlNode> Attributes
		{
			get { return null; }
		}

		public virtual IXmlNode ParentNode
		{
			get { return null; }
		}

		public virtual string Value
		{
			get { return null; }
			set { throw new InvalidOperationException(); }
		}

		public virtual IXmlNode AppendChild(IXmlNode newChild)
		{
			throw new InvalidOperationException();
		}

		public virtual string NamespaceUri
		{
			get { return null; }
		}

		protected string GetPrefix(string qualifiedName)
		{
			string prefix;
			string localName;
			GetQualifiedNameParts(qualifiedName, out prefix, out localName);

			return prefix;
		}

		protected string GetLocalName(string qualifiedName)
		{
			string prefix;
			string localName;
			GetQualifiedNameParts(qualifiedName, out prefix, out localName);

			return localName;
		}

		protected void GetQualifiedNameParts(string qualifiedName, out string prefix, out string localName)
		{
			int colonPosition = qualifiedName.IndexOf(':');

			if ((colonPosition == -1 || colonPosition == 0) || (qualifiedName.Length - 1) == colonPosition)
			{
				prefix = null;
				localName = qualifiedName;
			}
			else
			{
				prefix = qualifiedName.Substring(0, colonPosition);
				localName = qualifiedName.Substring(colonPosition + 1);
			}
		}
	}
}

