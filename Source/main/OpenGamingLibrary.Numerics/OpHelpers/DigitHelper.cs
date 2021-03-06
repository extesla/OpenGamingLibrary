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

namespace OpenGamingLibrary.Numerics.OpHelpers
{
	/// <summary>
	/// Contains big integer uint[] digits utilitary methods.
	/// </summary>
	static internal class DigitHelper
	{
		#region Working with digits length methods

		/// <summary>
		/// Returns real length of digits array (excluding leading zeroes).
		/// </summary>
		/// <param name="digits">Big ingeter digits.</param>
		/// <param name="length">Initial big integers length.</param>
		/// <returns>Real length.</returns>
		static public uint GetRealDigitsLength(uint[] digits, uint length)
		{
			for (; length > 0 && digits[length - 1] == 0; --length);
			return length;
		}

		/// <summary>
		/// Returns real length of digits array (excluding leading zeroes).
		/// </summary>
		/// <param name="digits">Big ingeter digits.</param>
		/// <param name="length">Initial big integers length.</param>
		/// <returns>Real length.</returns>
		static unsafe public uint GetRealDigitsLength(uint* digits, uint length)
		{
			for (; length > 0 && digits[length - 1] == 0; --length);
			return length;
		}

		/// <summary>
		/// Determines <see cref="BigInteger" /> object with lower length.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="smallerInt">Resulting smaller big integer (by length only).</param>
		/// <param name="biggerInt">Resulting bigger big integer (by length only).</param>
		static public void GetMinMaxLengthObjects(BigInteger int1, BigInteger int2, out BigInteger smallerInt, out BigInteger biggerInt)
		{
			if (int1._length < int2._length)
			{
				smallerInt = int1;
				biggerInt = int2;
			}
			else
			{
				smallerInt = int2;
				biggerInt = int1;
			}
		}

		#endregion Working with digits length methods

		#region Signed to unsigned+sign conversion methods

		/// <summary>
		/// Converts int value to uint digit and value sign.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <param name="resultValue">Resulting unsigned part.</param>
		/// <param name="negative">Resulting sign.</param>
		static public void ToUInt32WithSign(int value, out uint resultValue, out bool negative)
		{
			negative = value < 0;
			resultValue = !negative
				? (uint)value
				: value != int.MinValue ? (uint)-value : int.MaxValue + 1U;
		}

		/// <summary>
		/// Converts long value to ulong digit and value sign.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <param name="resultValue">Resulting unsigned part.</param>
		/// <param name="negative">Resulting sign.</param>
		static public void ToUInt64WithSign(long value, out ulong resultValue, out bool negative)
		{
			negative = value < 0;
			resultValue = !negative
				? (ulong)value
				: value != long.MinValue ? (ulong)-value : long.MaxValue + 1UL;
		}

		#endregion Signed to unsigned+sign conversion methods

		#region Working with digits directly methods

		/// <summary>
		/// Sets digits in given block to given value.
		/// </summary>
		/// <param name="block">Block start pointer.</param>
		/// <param name="blockLength">Block length.</param>
		/// <param name="value">Value to set.</param>
		static unsafe public void SetBlockDigits(uint* block, uint blockLength, uint value)
		{
			for (uint* blockEnd = block + blockLength; block < blockEnd; *block++ = value) ;
		}

		/// <summary>
		/// Sets digits in given block to given value.
		/// </summary>
		/// <param name="block">Block start pointer.</param>
		/// <param name="blockLength">Block length.</param>
		/// <param name="value">Value to set.</param>
		unsafe static public void SetBlockDigits(double* block, uint blockLength, double value)
		{
			for (double* blockEnd = block + blockLength; block < blockEnd; *block++ = value) ;
		}

		/// <summary>
		/// Copies digits from one block to another.
		/// </summary>
		/// <param name="blockFrom">From block start pointer.</param>
		/// <param name="blockTo">To block start pointer.</param>
		/// <param name="count">Count of dwords to copy.</param>
		static unsafe public void DigitsBlockCopy(uint* blockFrom, uint* blockTo, uint count)
		{
			for (uint* blockFromEnd = blockFrom + count; blockFrom < blockFromEnd; *blockTo++ = *blockFrom++) ;
		}

		#endregion Working with digits directly methods
	}
}
