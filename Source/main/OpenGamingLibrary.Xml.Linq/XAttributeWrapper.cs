﻿// Copyright (c) 2007 James Newton-King
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
using System.Xml.Linq;

namespace OpenGamingLibrary.Xml.Linq
{
	public class XAttributeWrapper : XObjectWrapper
	{
		private XAttribute Attribute
		{
			get { return (XAttribute)WrappedNode; }
		}

		public XAttributeWrapper(XAttribute attribute)
			: base(attribute)
		{
		}

		public override string Value
		{
			get { return Attribute.Value; }
			set { Attribute.Value = value; }
		}

		public override string LocalName
		{
			get { return Attribute.Name.LocalName; }
		}

		public override string NamespaceUri
		{
			get { return Attribute.Name.NamespaceName; }
		}

		public override IXmlNode ParentNode
		{
			get
			{
				return Attribute.Parent == null ? null : XContainerWrapper.WrapNode(Attribute.Parent);
			}
		}
	}

}

