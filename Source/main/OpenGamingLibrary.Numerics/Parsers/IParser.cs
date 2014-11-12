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
using System.Collections.Generic;

namespace OpenGamingLibrary.Numerics.Parsers
{
	/// <summary>
	/// Parser class interface.
	/// </summary>
	internal interface IParser
	{
		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="charToDigits">Char->digit dictionary.</param>
		/// <param name="checkFormat">Check actual format of number (0 or 0x at start).</param>
		/// <returns>Parsed object.</returns>
		BigInteger Parse(string value, uint numberBase, IDictionary<char, uint> charToDigits, bool checkFormat);
		
		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="startIndex">Index inside string from which to start.</param>
		/// <param name="endIndex">Index inside string on which to end.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="charToDigits">Char->digit dictionary.</param>
		/// <param name="digitsRes">Resulting digits.</param>
		/// <returns>Parsed integer length.</returns>
		uint Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes);
	}
}
