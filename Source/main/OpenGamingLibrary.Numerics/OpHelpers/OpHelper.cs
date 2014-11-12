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
using OpenGamingLibrary.Numerics.Multipliers;
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.OpHelpers
{
	/// <summary>
	/// Contains helping methods for operations over <see cref="BigInteger" />.
	/// </summary>
	static internal class OpHelper
	{
		#region Add operation

		/// <summary>
		/// Adds two big integers.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Resulting big integer.</returns>
		/// <exception cref="ArgumentException"><paramref name="int1" /> or <paramref name="int2" /> is too big for add operation.</exception>
		static public BigInteger Add(BigInteger int1, BigInteger int2)
		{
			// Process zero values in special way
			if (int2._length == 0) return new BigInteger(int1);
			if (int1._length == 0)
			{
				BigInteger x = new BigInteger(int2);
				x._negative = int1._negative; // always get sign of the first big integer
				return x;
			}

			// Determine big int with lower length
			BigInteger smallerInt;
			BigInteger biggerInt;
			DigitHelper.GetMinMaxLengthObjects(int1, int2, out smallerInt, out biggerInt);

			// Check for add operation possibility
			if (biggerInt._length == uint.MaxValue)
			{
				throw new ArgumentException(Strings.IntegerTooBig);
			}

			// Create new big int object of needed length
			BigInteger newInt = new BigInteger(biggerInt._length + 1, int1._negative);

			// Do actual addition
			newInt._length = DigitOpHelper.Add(
				biggerInt._digits,
				biggerInt._length,
				smallerInt._digits,
				smallerInt._length,
				newInt._digits);

			// Normalization may be needed
			newInt.TryNormalize();

			return newInt;
		}

		#endregion Add operation

		#region Subtract operation

		/// <summary>
		/// Subtracts two big integers.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Resulting big integer.</returns>
		static public BigInteger Sub(BigInteger int1, BigInteger int2)
		{
			// Process zero values in special way
			if (int1._length == 0) return new BigInteger(int2._digits, true);
			if (int2._length == 0) return new BigInteger(int1);

			// Determine lower big int (without sign)
			BigInteger smallerInt;
			BigInteger biggerInt;
			int compareResult = DigitOpHelper.Cmp(int1._digits, int1._length, int2._digits, int2._length);
			if (compareResult == 0) return new BigInteger(0); // integers are equal
			if (compareResult < 0)
			{
				smallerInt = int1;
				biggerInt = int2;
			}
			else
			{
				smallerInt = int2;
				biggerInt = int1;
			}

			// Create new big int object
			BigInteger newInt = new BigInteger(biggerInt._length, ReferenceEquals(int1, smallerInt) ^ int1._negative);

			// Do actual subtraction
			newInt._length = DigitOpHelper.Sub(
				biggerInt._digits,
				biggerInt._length,
				smallerInt._digits,
				smallerInt._length,
				newInt._digits);

			// Normalization may be needed
			newInt.TryNormalize();

			return newInt;
		}

		#endregion Subtract operation

		#region Add/Subtract operation - common methods

		/// <summary>
		/// Adds/subtracts one <see cref="BigInteger" /> to/from another.
		/// Determines which operation to use basing on operands signs.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="subtract">Was subtraction initially.</param>
		/// <returns>Add/subtract operation result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference.</exception>
		static public BigInteger AddSub(BigInteger int1, BigInteger int2, bool subtract)
		{
			// Exceptions
			if (ReferenceEquals(int1, null))
			{
				throw new ArgumentNullException("int1", Strings.CantBeNull);
			}
			else if (ReferenceEquals(int2, null))
			{
				throw new ArgumentNullException("int2", Strings.CantBeNull);
			}

			// Determine real operation type and result sign
			return subtract ^ int1._negative == int2._negative ? Add(int1, int2) : Sub(int1, int2);
		}

		#endregion Add/Subtract operation - common methods

		#region Power operation

		/// <summary>
		/// Returns a specified big integer raised to the specified power.
		/// </summary>
		/// <param name="value">Number to raise.</param>
		/// <param name="power">Power.</param>
		/// <param name="multiplyMode">Multiply mode set explicitly.</param>
		/// <returns>Number in given power.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public BigInteger Pow(BigInteger value, uint power, MultiplyMode multiplyMode)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			// Return one for zero pow
			if (power == 0) return 1;

			// Return the number itself from a power of one
			if (power == 1) return new BigInteger(value);

			// Return zero for a zero
			if (value._length == 0) return new BigInteger(0);

			// Get first one bit
			int msb = Bits.Msb(power);

			// Get multiplier
			IMultiplier multiplier = MultiplyManager.GetMultiplier(multiplyMode);

			// Do actual raising
			BigInteger res = value;
			for (uint powerMask = 1U << (msb - 1); powerMask != 0; powerMask >>= 1)
			{
				// Always square
				res = multiplier.Multiply(res, res);

				// Maybe mul
				if ((power & powerMask) != 0)
				{
					res = multiplier.Multiply(res, value);
				}
			}
			return res;
		}

		#endregion Power operation

		#region Compare operation

		/// <summary>
		/// Compares 2 <see cref="BigInteger" /> objects.
		/// Returns "-2" if any argument is null, "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />,
		/// "0" if equal and "1" if &gt;.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="throwNullException">Raises or not <see cref="NullReferenceException" />.</param>
		/// <returns>Comparsion result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="int1" /> or <paramref name="int2" /> is a null reference and <paramref name="throwNullException" /> is set to true.</exception>
		static public int Cmp(BigInteger int1, BigInteger int2, bool throwNullException)
		{
			// If one of the operands is null, throw exception or return -2
			bool isNull1 = ReferenceEquals(int1, null);
			bool isNull2 = ReferenceEquals(int2, null);
			if (isNull1 || isNull2)
			{
				if (throwNullException)
				{
					throw new ArgumentNullException(isNull1 ? "int1" : "int2", Strings.CantBeNullCmp);
				}
				else
				{
					return isNull1 && isNull2 ? 0 : -2;
				}
			}

			// Compare sign
			if (int1._negative && !int2._negative) return -1;
			if (!int1._negative && int2._negative) return 1;

			// Compare presentation
			return DigitOpHelper.Cmp(int1._digits, int1._length, int2._digits, int2._length) * (int1._negative ? -1 : 1);
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object to int.
		/// Returns "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />, "0" if equal and "1" if &gt;.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>Comparsion result.</returns>
		static public int Cmp(BigInteger int1, int int2)
		{
			// Special processing for zero
			if (int2 == 0) return int1._length == 0 ? 0 : (int1._negative ? -1 : 1);
			if (int1._length == 0) return int2 > 0 ? -1 : 1;

			// Compare presentation
			if (int1._length > 1) return int1._negative ? -1 : 1;
			uint digit2;
			bool negative2;
			DigitHelper.ToUInt32WithSign(int2, out digit2, out negative2);

			// Compare sign
			if (int1._negative && !negative2) return -1;
			if (!int1._negative && negative2) return 1;

			return int1._digits[0] == digit2 ? 0 : (int1._digits[0] < digit2 ^ negative2 ? -1 : 1);
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object to unsigned int.
		/// Returns "-1" if <paramref name="int1" /> &lt; <paramref name="int2" />, "0" if equal and "1" if &gt;.
		/// For internal use.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsigned integer.</param>
		/// <returns>Comparsion result.</returns>
		static public int Cmp(BigInteger int1, uint int2)
		{
			// Special processing for zero
			if (int2 == 0) return int1._length == 0 ? 0 : (int1._negative ? -1 : 1);
			if (int1._length == 0) return -1;

			// Compare presentation
			if (int1._negative) return -1;
			if (int1._length > 1) return 1;
			return int1._digits[0] == int2 ? 0 : (int1._digits[0] < int2 ? -1 : 1);
		}

		#endregion Compare operation

		#region Shift operation

		/// <summary>
		/// Shifts <see cref="BigInteger" /> object.
		/// Determines which operation to use basing on shift sign.
		/// </summary>
		/// <param name="BigInteger">Big integer.</param>
		/// <param name="shift">Bits count to shift.</param>
		/// <param name="toLeft">If true the shifting to the left.</param>
		/// <returns>Bitwise shift operation result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="BigInteger" /> is a null reference.</exception>
		static public BigInteger Sh(BigInteger BigInteger, long shift, bool toLeft)
		{
			// Exceptions
			if (ReferenceEquals(BigInteger, null))
			{
				throw new ArgumentNullException("BigInteger", Strings.CantBeNullOne);
			}

			// Zero can't be shifted
			if (BigInteger._length == 0) return new BigInteger(0);

			// Can't shift on zero value
			if (shift == 0) return new BigInteger(BigInteger);

			// Determine real bits count and direction
			ulong bitCount;
			bool negativeShift;
			DigitHelper.ToUInt64WithSign(shift, out bitCount, out negativeShift);
			toLeft ^= negativeShift;

			// Get position of the most significant bit in BigInteger and amount of bits in BigInteger
			int msb = Bits.Msb(BigInteger._digits[BigInteger._length - 1]);
			ulong BigIntegerBitCount = (ulong)(BigInteger._length - 1) * Constants.DigitBitCount + (ulong)msb + 1UL;

			// If shifting to the right and shift is too big then return zero
			if (!toLeft && bitCount >= BigIntegerBitCount) return new BigInteger(0);

			// Calculate new bit count
			ulong newBitCount = toLeft ? BigIntegerBitCount + bitCount : BigIntegerBitCount - bitCount;

			// If shifting to the left and shift is too big to fit in big integer, throw an exception
			if (toLeft && newBitCount > Constants.MaxBitCount)
			{
				throw new ArgumentException(Strings.IntegerTooBig, "BigInteger");
			}

			// Get exact length of new big integer (no normalize is ever needed here).
			// Create new big integer with given length
			uint newLength = (uint)(newBitCount / Constants.DigitBitCount + (newBitCount % Constants.DigitBitCount == 0 ? 0UL : 1UL));
			BigInteger newInt = new BigInteger(newLength, BigInteger._negative);

			// Get full and small shift values
			uint fullDigits = (uint)(bitCount / Constants.DigitBitCount);
			int smallShift = (int)(bitCount % Constants.DigitBitCount);

			// We can just copy (no shift) if small shift is zero
			if (smallShift == 0)
			{
				if (toLeft)
				{
					Array.Copy(BigInteger._digits, 0, newInt._digits, fullDigits, BigInteger._length);
				}
				else
				{
					Array.Copy(BigInteger._digits, fullDigits, newInt._digits, 0, newLength);
				}
			}
			else
			{
				// Do copy with real shift in the needed direction
				if (toLeft)
				{
					DigitOpHelper.Shr(BigInteger._digits, 0, BigInteger._length, newInt._digits, fullDigits + 1, Constants.DigitBitCount - smallShift);
				}
				else
				{
					// If new result length is smaller then original length we shouldn't lose any digits
					if (newLength < (BigInteger._length - fullDigits))
					{
						newLength++;
					}

					DigitOpHelper.Shr(BigInteger._digits, fullDigits, newLength, newInt._digits, 0, smallShift);
				}
			}

			return newInt;
		}

		#endregion Shift operation
	}
}
