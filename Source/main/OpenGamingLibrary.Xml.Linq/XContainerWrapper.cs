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
using System.Linq;
using System.Xml.Linq;

namespace OpenGamingLibrary.Xml.Linq
{

	public class XContainerWrapper : XObjectWrapper
	{
		private IList<IXmlNode> _childNodes;

		private XContainer Container
		{
			get { return (XContainer)WrappedNode; }
		}

		public XContainerWrapper(XContainer container)
			: base(container)
		{
		}

		public override IList<IXmlNode> ChildNodes
		{
			get
			{
				// childnodes is read multiple times
				// cache results to prevent multiple reads which kills perf in large documents
				if (_childNodes == null)
				{
					_childNodes = Container.Nodes()
						.Cast<XObject>()
						.Select<XObject, IXmlNode>(WrapNode)
						.ToList();
				}

				return _childNodes;
			}
		}

		public override IXmlNode ParentNode
		{
			get
			{
				if (Container.Parent == null)
					return null;

				return WrapNode(Container.Parent);
			}
		}

		internal static IXmlNode WrapNode(XObject node)
		{
			if (node is XDocument)
				return new XDocumentWrapper((XDocument)node);
			else if (node is XElement)
				return new XElementWrapper((XElement)node);
			else if (node is XContainer)
				return new XContainerWrapper((XContainer)node);
			else if (node is XProcessingInstruction)
				return new XProcessingInstructionWrapper((XProcessingInstruction)node);
			else if (node is XText)
				return new XTextWrapper((XText)node);
			else if (node is XComment)
				return new XCommentWrapper((XComment)node);
			else if (node is XAttribute)
				return new XAttributeWrapper((XAttribute)node);
			else if (node is XDocumentType)
				return new XDocumentTypeWrapper((XDocumentType)node);
			else
				return new XObjectWrapper(node);
		}

		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			Container.Add(newChild.WrappedNode);
			_childNodes = null;

			return newChild;
		}
	}
}

