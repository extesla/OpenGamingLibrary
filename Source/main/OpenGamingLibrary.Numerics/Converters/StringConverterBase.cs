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
using System.Text;
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Converters
{
	/// <summary>
	/// Base class for ToString converters.
	/// Contains default implementations of convert operation over <see cref="BigInteger" /> instances.
	/// </summary>
	abstract internal class StringConverterBase : IStringConverter
	{
		#region Private fields

		IStringConverter _pow2StringConverter; // converter for pow2 case

		#endregion Private fields

		#region Constructor

		/// <summary>
		/// Creates new <see cref="StringConverterBase" /> instance.
		/// </summary>
		/// <param name="pow2StringConverter">Converter for pow2 case.</param>
		public StringConverterBase(IStringConverter pow2StringConverter)
		{
			_pow2StringConverter = pow2StringConverter;
		}

		#endregion Constructor

		/// <summary>
		/// Returns string representation of <see cref="BigInteger" /> object in given base.
		/// </summary>
		/// <param name="BigInteger">Big integer to convert.</param>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Object string representation.</returns>
		/// <exception cref="ArgumentException"><paramref name="numberBase" /> is less then 2 or <paramref name="BigInteger" /> is too big to fit in string.</exception>
		virtual public string ToString(BigInteger BigInteger, uint numberBase, char[] alphabet)
		{
			// Test base
			if (numberBase < 2 || numberBase > 65536)
			{
				throw new ArgumentException(Strings.ToStringSmallBase, "numberBase");
			}

			// Special processing for zero values
			if (BigInteger._length == 0) return "0";

			// Calculate output array length
			uint outputLength = (uint)System.Math.Ceiling(Constants.DigitBaseLog / System.Math.Log(numberBase) * BigInteger._length);

			// Define length coefficient for string builder
			bool isBigBase = numberBase > alphabet.Length;
			uint lengthCoef = isBigBase ? (uint)System.Math.Ceiling(System.Math.Log10(numberBase)) + 2U : 1U;

			// Determine maximal possible length of string
			ulong maxBuilderLength = (ulong)outputLength * lengthCoef + 1UL;
			if (maxBuilderLength > int.MaxValue)
			{
				// This big integer can't be transformed to string
				throw new ArgumentException(Strings.IntegerTooBig, "BigInteger");
			}

			// Transform digits into another base
			uint[] outputArray = ToString(BigInteger._digits, BigInteger._length, numberBase, ref outputLength);

			// Output everything to the string builder
			StringBuilder outputBuilder = new StringBuilder((int)(outputLength * lengthCoef + 1));

			// Maybe append minus sign
			if (BigInteger._negative)
			{
				outputBuilder.Append(Constants.DigitsMinusChar);
			}

			// Output all digits
			for (uint i = outputLength - 1; i < outputLength; --i)
			{
				if (!isBigBase)
				{
					// Output char-by-char for bases up to covered by alphabet
					outputBuilder.Append(alphabet[(int)outputArray[i]]);
				}
				else
				{
					// Output digits in bracets for bigger bases
					outputBuilder.Append(Constants.DigitOpeningBracet);
					outputBuilder.Append(outputArray[i].ToString());
					outputBuilder.Append(Constants.DigitClosingBracet);
				}
			}

			return outputBuilder.ToString();
		}

		/// <summary>
		/// Converts digits from internal representaion into given base.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="length">Big integer length.</param>
		/// <param name="numberBase">Base to use for output.</param>
		/// <param name="outputLength">Calculated output length (will be corrected inside).</param>
		/// <returns>Conversion result (later will be transformed to string).</returns>
		virtual public uint[] ToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
		{
			// Default implementation - always call pow2 converter if numberBase is pow of 2
			return numberBase == 1U << Bits.Msb(numberBase)
				? _pow2StringConverter.ToString(digits, length, numberBase, ref outputLength)
				: null;
		}
	}
}
