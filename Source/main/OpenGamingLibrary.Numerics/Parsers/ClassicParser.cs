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
using OpenGamingLibrary.Numerics.OpHelpers;

namespace OpenGamingLibrary.Numerics.Parsers
{
	/// <summary>
	/// Classic parsing algorithm using multiplication (O[n^2]).
	/// </summary>
	sealed internal class ClassicParser : ParserBase
	{
		#region Constructor

		/// <summary>
		/// Creates new <see cref="ClassicParser" /> instance.
		/// </summary>
		/// <param name="pow2Parser">Parser for pow2 case.</param>
		public ClassicParser(IParser pow2Parser) : base(pow2Parser) {}

		#endregion Constructor

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
		override public uint Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
		{
			uint newLength = base.Parse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes);

			// Maybe base method already parsed this number
			if (newLength != 0) return newLength;

			// Do parsing in big cycle
			ulong numberBaseLong = numberBase;
			ulong digit;
			for (int i = startIndex; i <= endIndex; ++i)
			{
				digit = StrRepHelper.GetDigit(charToDigits, value[i], numberBase);

				// Next multiply existing values by base and add this value to them
				if (newLength == 0)
				{
					if (digit != 0)
					{
						digitsRes[0] = (uint)digit;
						newLength = 1;
					}
				}
				else
				{
					for (uint j = 0; j < newLength; ++j)
					{
						digit += digitsRes[j] * numberBaseLong;
						digitsRes[j] = (uint)digit;
						digit >>= 32;
					}
					if (digit != 0)
					{
						digitsRes[newLength++] = (uint)digit;
					}
				}
			}

			return newLength;
		}
	}
}
