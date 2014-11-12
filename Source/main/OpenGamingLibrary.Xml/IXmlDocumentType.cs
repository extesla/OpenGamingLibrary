// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Xml
{

	/// <summary>
	/// I xml document type.
	/// </summary>
	public interface IXmlDocumentType : IXmlNode
	{
		string Name { get; }
		string System { get; }
		string Public { get; }
		string InternalSubset { get; }
	}
}

