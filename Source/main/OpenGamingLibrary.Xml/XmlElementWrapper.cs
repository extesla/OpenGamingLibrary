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

namespace OpenGamingLibrary.Xml
{
	public class XmlElementWrapper : XmlNodeWrapper, IXmlElement
	{
		private readonly XmlElement _element;

		public XmlElementWrapper(XmlElement element)
			: base(element)
		{
			_element = element;
		}

		public void SetAttributeNode(IXmlNode attribute)
		{
			XmlNodeWrapper xmlAttributeWrapper = (XmlNodeWrapper)attribute;

			_element.SetAttributeNode((XmlAttribute)xmlAttributeWrapper.WrappedNode);
		}

		public string GetPrefixOfNamespace(string namespaceUri)
		{
			return _element.GetPrefixOfNamespace(namespaceUri);
		}

		public bool IsEmpty
		{
			get { return _element.IsEmpty; }
		}
	}
}

