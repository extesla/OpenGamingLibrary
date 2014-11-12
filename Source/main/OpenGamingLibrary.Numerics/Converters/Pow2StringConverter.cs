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
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Converters
{
	/// <summary>
	/// Provides special fast (with linear time) ToString converting if base is pow of 2.
	/// </summary>
	sealed internal class Pow2StringConverter : IStringConverter
	{
		// Not needed in this implementation
		public string ToString(BigInteger BigInteger, uint numberBase, char[] alphabet)
		{
			return null;
		}

		/// <summary>
		/// Converts digits from internal representaion into given base.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="length">Big integer length.</param>
		/// <param name="numberBase">Base to use for output.</param>
		/// <param name="outputLength">Calculated output length (will be corrected inside).</param>
		/// <returns>Conversion result (later will be transformed to string).</returns>
		public uint[] ToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
		{
			// Calculate real output length
			int bitsInChar = Bits.Msb(numberBase);
			ulong digitsBitLength = (ulong)(length - 1) * Constants.DigitBitCount + (ulong)Bits.Msb(digits[length - 1]) + 1UL;
			uint realOutputLength = (uint)(digitsBitLength / (ulong)bitsInChar);
			if (digitsBitLength % (ulong)bitsInChar != 0)
			{
				++realOutputLength;
			}

			// Prepare shift variables
			int nextDigitShift = Constants.DigitBitCount - bitsInChar; // after this shift next digit must be used
			int initialShift = 0;

			// We will also need bitmask for copying digits
			uint digitBitMask = numberBase - 1;

			// Create an output array for storing of number in other base
			uint[] outputArray = new uint[realOutputLength];

			// Walk thru original digits and fill output
			uint outputDigit;
			for (uint outputIndex = 0, digitIndex = 0; outputIndex < realOutputLength; ++outputIndex)
			{
				// Get part of current digit
				outputDigit = digits[digitIndex] >> initialShift;

				// Maybe we need to go to the next digit
				if (initialShift >= nextDigitShift)
				{
					// Go to the next digit
					++digitIndex;

					// Maybe we also need a part of the next digit
					if (initialShift != nextDigitShift && digitIndex < length)
					{
						outputDigit |= digits[digitIndex] << (Constants.DigitBitCount - initialShift);
					}

					// Modify shift so that it will be valid for the next digit
					initialShift = (initialShift + bitsInChar) % Constants.DigitBitCount;
				}
				else
				{
					// Modify shift as usual
					initialShift += bitsInChar;
				}

				// Write masked result to the output
				outputArray[outputIndex] = outputDigit & digitBitMask;
			}

			outputLength = realOutputLength;
			return outputArray;
		}
	}
}
