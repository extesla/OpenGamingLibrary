// Copyright (c) 2007 James Newton-King
// Copyright (c) 2014 Extesla, LLC.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OpenGamingLibrary.Xml
{
	public class XmlNodeWrapper : IXmlNode
	{
		private readonly XmlNode _node;
		private IList<IXmlNode> _childNodes;

		public XmlNodeWrapper(XmlNode node)
		{
			_node = node;
		}

		public object WrappedNode
		{
			get { return _node; }
		}

		public XmlNodeType NodeType
		{
			get { return _node.NodeType; }
		}

		public virtual string LocalName
		{
			get { return _node.LocalName; }
		}

		public IList<IXmlNode> ChildNodes
		{
			get
			{
				// childnodes is read multiple times
				// cache results to prevent multiple reads which kills perf in large documents
				if (_childNodes == null)
				{
					_childNodes = _node.ChildNodes
						.Cast<XmlNode>()
						.Select<XmlNode, IXmlNode>(WrapNode)
						.ToList();
				}

				return _childNodes;
			}
		}

		internal static IXmlNode WrapNode(XmlNode node)
		{
			switch (node.NodeType)
			{
			case XmlNodeType.Element:
				return new XmlElementWrapper((XmlElement) node);
			case XmlNodeType.XmlDeclaration:
				return new XmlDeclarationWrapper((XmlDeclaration) node);
			case XmlNodeType.DocumentType:
				return new XmlDocumentTypeWrapper((XmlDocumentType) node);
			default:
				return new XmlNodeWrapper(node);
			}
		}

		public IList<IXmlNode> Attributes
		{
			get
			{
				if (_node.Attributes == null)
				{
					return null;
				}
				return _node.Attributes
					.Cast<XmlAttribute>()
					.Select<XmlAttribute, IXmlNode>(WrapNode)
					.ToList();
			}
		}

		public IXmlNode ParentNode
		{
			get
			{
				XmlNode node = (_node is XmlAttribute)
					? ((XmlAttribute) _node).OwnerElement
					: _node.ParentNode;

				if (node == null)
					return null;

				return WrapNode(node);
			}
		}

		public string Value
		{
			get { return _node.Value; }
			set { _node.Value = value; }
		}

		public IXmlNode AppendChild(IXmlNode newChild)
		{
			XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper) newChild;
			_node.AppendChild(xmlNodeWrapper._node);
			_childNodes = null;

			return newChild;
		}

		public string NamespaceUri
		{
			get { return _node.NamespaceURI; }
		}
	}
}

