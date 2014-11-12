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
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Parsers
{
	/// <summary>
	/// Provides special fast (with linear time) parsing if base is pow of 2.
	/// </summary>
	sealed internal class Pow2Parser : IParser
	{
		// Not needed in this implementation
		public BigInteger Parse(string value, uint numberBase, IDictionary<char, uint> charToDigits, bool checkFormat)
		{
			return null;
		}

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
		public uint Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
		{
			// Calculate length of input string
			int bitsInChar = Bits.Msb(numberBase);
			uint valueLength = (uint)(endIndex - startIndex + 1);
			ulong valueBitLength = (ulong)valueLength * (ulong)bitsInChar;

			// Calculate needed digits length and first shift
			uint digitsLength = (uint)(valueBitLength / Constants.DigitBitCount) + 1;
			uint digitIndex = digitsLength - 1;
			int initialShift = (int)(valueBitLength % Constants.DigitBitCount);

			// Probably correct digits length
			if (initialShift == 0)
			{
				--digitsLength;
			}

			// Do parsing in big cycle
			uint digit;
			for (int i = startIndex; i <= endIndex; ++i)
			{
				digit = StrRepHelper.GetDigit(charToDigits, value[i], numberBase);

				// Correct initial digit shift
				if (initialShift == 0)
				{
					// If shift is equals to zero then char is not on digit elemtns bounds,
					// so just go to the previous digit
					initialShift = Constants.DigitBitCount - bitsInChar;
					--digitIndex;
				}
				else
				{
					// Here shift might be negative, but it's okay
					initialShift -= bitsInChar;
				}

				// Insert new digit in correct place
				digitsRes[digitIndex] |= initialShift < 0 ? digit >> -initialShift : digit << initialShift;

				// In case if shift was negative we also must modify previous digit
				if (initialShift < 0)
				{
					initialShift += Constants.DigitBitCount;
					digitsRes[--digitIndex] |= digit << initialShift;
				}
			}

			if (digitsRes[digitsLength - 1] == 0)
			{
				--digitsLength;
			}
			return digitsLength;
		}
	}
}