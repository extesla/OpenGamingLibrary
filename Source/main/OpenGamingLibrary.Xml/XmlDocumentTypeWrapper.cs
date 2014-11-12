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
using System.Xml;

namespace OpenGamingLibrary.Xml
{
	public class XmlDocumentTypeWrapper : XmlNodeWrapper, IXmlDocumentType
	{
		private readonly XmlDocumentType _documentType;

		public XmlDocumentTypeWrapper(XmlDocumentType documentType)
			: base(documentType)
		{
			_documentType = documentType;
		}

		public string Name
		{
			get { return _documentType.Name; }
		}

		public string System
		{
			get { return _documentType.SystemId; }
		}

		public string Public
		{
			get { return _documentType.PublicId; }
		}

		public string InternalSubset
		{
			get { return _documentType.InternalSubset; }
		}

		public override string LocalName
		{
			get { return "DOCTYPE"; }
		}
	}
}

