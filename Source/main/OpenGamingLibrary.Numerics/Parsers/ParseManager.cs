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
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Parsers
{
	/// <summary>
	/// Used to retrieve needed parser.
	/// </summary>
	static internal class ParseManager
	{
		#region Fields

		/// <summary>
		/// Classic parser instance.
		/// </summary>
		static readonly public IParser ClassicParser;

		/// <summary>
		/// Fast parser instance.
		/// </summary>
		static readonly public IParser FastParser;

		#endregion Fields

		#region Constructors

		// .cctor
		static ParseManager()
		{
			// Create new pow2 parser instance
			IParser pow2Parser = new Pow2Parser();

			// Create new classic parser instance
			IParser classicParser = new ClassicParser(pow2Parser);

			// Fill publicity visible parser fields
			ClassicParser = classicParser;
			FastParser = new FastParser(pow2Parser, classicParser);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Returns parser instance for given parse mode.
		/// </summary>
		/// <param name="mode">Parse mode.</param>
		/// <returns>Parser instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="mode" /> is out of range.</exception>
		static public IParser GetParser(ParseMode mode)
		{
			switch (mode)
			{
				case ParseMode.Fast:
					return FastParser;
				case ParseMode.Classic:
					return ClassicParser;
				default:
					throw new ArgumentOutOfRangeException("mode");
			}
		}

		/// <summary>
		/// Returns current parser instance.
		/// </summary>
		/// <returns>Current parser instance.</returns>
		static public IParser GetCurrentParser()
		{
			return GetParser(BigInteger.GlobalSettings.ParseMode);
		}

		#endregion Methods
	}
}
