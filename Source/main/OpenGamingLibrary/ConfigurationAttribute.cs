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

namespace OpenGamingLibrary
{
	/// <summary>
	/// Denotes a class as being a configuration component, which may have
	/// methods to be invoked as part of the configuration of an
	/// <see cref="IContext"/>.
	/// </summary>
	/// <author>Sean Quinn</author>
	public class ConfigurationAttribute : ComponentAttribute
	{
		/// <summary>
		/// Initializes a new instance of the ConfigurationAttribute class.
		/// </summary>
		public ConfigurationAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the Configuration class.
		/// </summary>
		/// <param name="name"></param>
		public ConfigurationAttribute(string name)
		{
			Name = name;
		}
	}
}

