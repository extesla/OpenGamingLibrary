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
using System.Xml.Linq;

namespace OpenGamingLibrary.Xml.Linq
{
	public class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration
	{
		internal XDeclaration Declaration { get; private set; }

		public XDeclarationWrapper(XDeclaration declaration) : base(null)
		{
			Declaration = declaration;
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.XmlDeclaration; }
		}

		public string Version
		{
			get { return Declaration.Version; }
		}

		public string Encoding
		{
			get { return Declaration.Encoding; }
			set { Declaration.Encoding = value; }
		}

		public string Standalone
		{
			get { return Declaration.Standalone; }
			set { Declaration.Standalone = value; }
		}
	}
}

