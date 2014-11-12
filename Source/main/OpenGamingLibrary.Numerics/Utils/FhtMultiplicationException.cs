// Copyright (C) 2005-2013, Andriy Kozachuk
// Copyright (C) 2014 Extesla, LLC.
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
using System;
using System.Runtime.Serialization;

namespace OpenGamingLibrary.Numerics.Utils
{
	/// <summary>
	/// Used when FHT multiplication result is incorrect.
	/// </summary>
	[Serializable]
	public class FhtMultiplicationException : ApplicationException, ISerializable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FhtMultiplicationException" /> class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public FhtMultiplicationException(string message) : base(message) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="FhtMultiplicationException" /> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected FhtMultiplicationException(SerializationInfo info, StreamingContext context)
			: base(info, context) {}
	}
}
