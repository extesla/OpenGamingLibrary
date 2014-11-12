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
	public class XElementWrapper : XContainerWrapper, IXmlElement
	{
		private XElement Element
		{
			get { return (XElement)WrappedNode; }
		}

		public XElementWrapper(XElement element)
			: base(element)
		{
		}

		public void SetAttributeNode(IXmlNode attribute)
		{
			XObjectWrapper wrapper = (XObjectWrapper)attribute;
			Element.Add(wrapper.WrappedNode);
		}

		public override IList<IXmlNode> Attributes
		{
			get { return Element.Attributes().Select(a => new XAttributeWrapper(a)).Cast<IXmlNode>().ToList(); }
		}

		public override string Value
		{
			get { return Element.Value; }
			set { Element.Value = value; }
		}

		public override string LocalName
		{
			get { return Element.Name.LocalName; }
		}

		public override string NamespaceUri
		{
			get { return Element.Name.NamespaceName; }
		}

		public string GetPrefixOfNamespace(string namespaceUri)
		{
			return Element.GetPrefixOfNamespace(namespaceUri);
		}

		public bool IsEmpty
		{
			get { return Element.IsEmpty; }
		}
	}
}

