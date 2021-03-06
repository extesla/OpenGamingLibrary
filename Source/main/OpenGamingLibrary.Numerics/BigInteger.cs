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
using System.Globalization;
using OpenGamingLibrary.Numerics.Converters;
using OpenGamingLibrary.Numerics.Dividers;
using OpenGamingLibrary.Numerics.Multipliers;
using OpenGamingLibrary.Numerics.OpHelpers;
using OpenGamingLibrary.Numerics.Parsers;
using OpenGamingLibrary.Numerics.Settings;
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics
{
	/// <summary>
	/// Numeric class which represents arbitrary-precision integers.
	/// </summary>
	public sealed class BigInteger :
		IEquatable<BigInteger>, IEquatable<int>, IEquatable<uint>, IEquatable<long>, IEquatable<ulong>,
		IComparable, IComparable<BigInteger>, IComparable<int>, IComparable<uint>, IComparable<long>, IComparable<ulong>
	{
#if DEBUG

		/// <summary>
		/// Lock for maximal error during FHT rounding (debug-mode only).
		/// </summary>
		static readonly internal object _maxFhtRoundErrorLock = new object();

		/// <summary>
		/// Maximal error during FHT rounding (debug-mode only).
		/// </summary>
		static public double MaxFhtRoundError;

#endif

		#region Static fields

		static BigIntegerGlobalSettings _globalSettings = new BigIntegerGlobalSettings();

		#endregion Static fields

		#region Internal Fields
		/// <summary>
		/// The big integer digits.
		/// </summary>
		internal uint[] _digits = new uint[0];

		/// <summary>
		/// The big integer digits length, e.g. the number of digits.
		/// </summary>
		internal uint _length = 0;

		/// <summary>
		/// The big integer sign, e.g. "-" if <c>true</c>.
		/// </summary>
		internal bool _negative;
		internal ToStringMode _toStringMode = ToStringMode.Fast;
		internal bool _autoNormalize = false;
		#endregion

		#region Properties
		public ToStringMode ToStringMode
		{
			get { return _toStringMode; }
			set { _toStringMode = value; }
		}

		public bool AutoNormalize
		{
			get { return _autoNormalize; }
			set { _autoNormalize = value; }
		}
		#endregion

		#region Constructors
		public BigInteger() : this(0)
		{
		}

		public BigInteger(byte[] value)
		{
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			throw new NotImplementedException("Not yet implemented");
		}

		public BigInteger(double value)
		{
			if (double.IsInfinity(value)) {
				throw new OverflowException("BigInteger cannot represent infinity.");
			}

			if (double.IsNaN(value)) {
				throw new OverflowException("The value is not a number.");
			}
			// TODO: support double
			// _digits = 0;
			// DigitHelper.ToDoubleWithSign(value, out _digits[0], out _negative);
			throw new NotImplementedException("Not yet implemented");
		}

		public BigInteger(decimal value)
		{
			int[] bits = decimal.GetBits(value);
			int num = 3;
			while (num > 0 && bits[num - 1] == 0) {
				num--;
			}

			if (num == 0) {
				// TODO: Assign to zero.
				//return
			}
			if (num == 1 && bits[0] > 0) {
				//this._sign = bits[0];
				//this._sign *= (((bits[3] & -2147483648) != 0) ? -1 : 1);
				//this._bits = null;
				//return
			}
			//this._bits = new uint[num];
			//this._bits[0] = (uint)bits[0];
			//if (num > 1)
			//{
			//	this._bits[1] = (uint)bits[1];
			//}
			//if (num > 2)
			//{
			//	this._bits[2] = (uint)bits[2];
			//}
			//this._sign = (((bits[3] & -2147483648) != 0) ? -1 : 1);
			throw new NotImplementedException("Not yet implemented");
		}

		public BigInteger(float value)
		{
			if (double.IsInfinity(value)) {
				throw new OverflowException("BigInteger cannot represent infinity.");
			}

			if (double.IsNaN(value)) {
				throw new OverflowException("The value is not a number.");
			}
			// TODO: support double
			// _digits = 0;
			// DigitHelper.ToDoubleWithSign(value, out _digits[0], out _negative);
			throw new NotImplementedException("Not yet implemented");
		}

		/// <summary>
		/// Creates new big integer from integer value.
		/// </summary>
		/// <param name="value">Integer value to create big integer from.</param>
		public BigInteger(int value)
		{
			if (value != 0)
			{
				// Prepare internal fields
				_digits = new uint[_length = 1];

				// Fill the only big integer digit
				DigitHelper.ToUInt32WithSign(value, out _digits[0], out _negative);
			}
		}

		/// <summary>
		/// Creates new big integer from unsigned integer value.
		/// </summary>
		/// <param name="value">Unsigned integer value to create big integer from.</param>
		[CLSCompliant(false)]
		public BigInteger(uint value)
		{
			if (value != 0)
			{
				// Prepare internal fields
				_digits = new uint[] { value };
				_length = 1;
			}
		}

		/// <summary>
		/// Creates new big integer from long value.
		/// </summary>
		/// <param name="value">Long value to create big integer from.</param>
		public BigInteger(long value)
		{
			if (value != 0)
			{
				// Fill the only big integer digit
				ulong newValue;
				DigitHelper.ToUInt64WithSign(value, out newValue, out _negative);
				InitFromUlong(newValue);
			}
		}

		/// <summary>
		/// Creates new big integer from unsigned long value.
		/// </summary>
		/// <param name="value">Unsigned long value to create big integer from.</param>
		[CLSCompliant(false)]
		public BigInteger(ulong value)
		{
			if (value != 0)
			{
				InitFromUlong(value);
			}
		}

		/// <summary>
		/// Creates new big integer from array of it's "digits".
		/// Digit with lower index has less weight.
		/// </summary>
		/// <param name="digits">Array of <see cref="BigInteger" /> digits.</param>
		/// <param name="negative">True if this number is negative.</param>
		/// <exception cref="ArgumentNullException"><paramref name="digits" /> is a null reference.</exception>
		[CLSCompliant(false)]
		public BigInteger(uint[] digits, bool negative)
		{
			// Exceptions
			if (digits == null)
			{
				throw new ArgumentNullException("values");
			}

			InitFromDigits(digits, negative, DigitHelper.GetRealDigitsLength(digits, (uint)digits.LongLength));
		}


		/// <summary>
		/// Creates new <see cref="BigInteger" /> from string.
		/// </summary>
		/// <param name="value">Number as string.</param>
		public BigInteger(string value)
		{
			BigInteger BigInteger = Parse(value);
			InitFromBigInteger(BigInteger);
		}

		/// <summary>
		/// Creates new <see cref="BigInteger" /> from string.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		[CLSCompliant(false)]
		public BigInteger(string value, uint numberBase)
		{
			BigInteger BigInteger = Parse(value, numberBase);
			InitFromBigInteger(BigInteger);
		}


		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="value">Value to copy from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		public BigInteger(BigInteger value)
		{
			// Exceptions
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			InitFromBigInteger(value);
		}


		/// <summary>
		/// Creates new empty big integer with desired sign and length.
		/// 
		/// For internal use.
		/// </summary>
		/// <param name="length">Desired digits length.</param>
		/// <param name="negative">Desired integer sign.</param>
		internal BigInteger(uint length, bool negative)
		{
			_digits = new uint[_length = length];
			_negative = negative;
		}

		/// <summary>
		/// Creates new big integer from array of it's "digits" but with given length.
		/// Digit with lower index has less weight.
		/// 
		/// For internal use.
		/// </summary>
		/// <param name="digits">Array of <see cref="BigInteger" /> digits.</param>
		/// <param name="negative">True if this number is negative.</param>
		/// <param name="length">Length to use for internal digits array.</param>
		/// <exception cref="ArgumentNullException"><paramref name="digits" /> is a null reference.</exception>
		internal BigInteger(uint[] digits, bool negative, uint length)
		{
			// Exceptions
			if (digits == null)
			{
				throw new ArgumentNullException("values");
			}

			InitFromDigits(digits, negative, length);
		}

		#endregion Constructors

		#region Static public properties

		/// <summary>
		/// <see cref="BigInteger" /> global settings.
		/// </summary>
		static public BigIntegerGlobalSettings GlobalSettings
		{
			get { return _globalSettings; }
		}

		#endregion Static public properties

		#region Operators

		#region operator==

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, false) == 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) == 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		static public bool operator ==(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) == 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsinged integer and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if equals.</returns>
		[CLSCompliant(false)]
		static public bool operator ==(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) == 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="BigInteger" /> object and returns true if their internal state is equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if equals.</returns>
		[CLSCompliant(false)]
		static public bool operator ==(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) == 0;
		}

		#endregion operator==

		#region operator!=

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, false) != 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) != 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		static public bool operator !=(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) != 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsigned integer and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsigned integer.</param>
		/// <returns>True if not equals.</returns>
		[CLSCompliant(false)]
		static public bool operator !=(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) != 0;
		}

		/// <summary>
		/// Compares unsigned integer with <see cref="BigInteger" /> object and returns true if their internal state is not equal.
		/// </summary>
		/// <param name="int1">First unsigned integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if not equals.</returns>
		[CLSCompliant(false)]
		static public bool operator !=(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) != 0;
		}

		#endregion operator!=

		#region operator>

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, true) > 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) > 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		static public bool operator >(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) < 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsigned integer and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsigned integer.</param>
		/// <returns>True if first is greater.</returns>
		[CLSCompliant(false)]
		static public bool operator >(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) > 0;
		}

		/// <summary>
		/// Compares unsigned integer with <see cref="BigInteger" /> object and returns true if first is greater.
		/// </summary>
		/// <param name="int1">First unsigned integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater.</returns>
		[CLSCompliant(false)]
		static public bool operator >(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) < 0;
		}

		#endregion operator>

		#region operator>=

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, true) >= 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) >= 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		static public bool operator >=(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) <= 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsinged integer and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		[CLSCompliant(false)]
		static public bool operator >=(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) >= 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="BigInteger" /> object and returns true if first is greater or equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is greater or equal.</returns>
		[CLSCompliant(false)]
		static public bool operator >=(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) <= 0;
		}

		#endregion operator>=

		#region operator<

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, true) < 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) < 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		static public bool operator <(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) > 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsinged integer and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is lighter.</returns>
		[CLSCompliant(false)]
		static public bool operator <(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) < 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="BigInteger" /> object and returns true if first is lighter.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter.</returns>
		[CLSCompliant(false)]
		static public bool operator <(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) > 0;
		}

		#endregion operator<

		#region operator<=

		/// <summary>
		/// Compares two <see cref="BigInteger" /> objects and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(BigInteger int1, BigInteger int2)
		{
			return OpHelper.Cmp(int1, int2, true) <= 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with integer and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(BigInteger int1, int int2)
		{
			return OpHelper.Cmp(int1, int2) <= 0;
		}

		/// <summary>
		/// Compares integer with <see cref="BigInteger" /> object and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		static public bool operator <=(int int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) >= 0;
		}

		/// <summary>
		/// Compares <see cref="BigInteger" /> object with unsinged integer and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second unsinged integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		[CLSCompliant(false)]
		static public bool operator <=(BigInteger int1, uint int2)
		{
			return OpHelper.Cmp(int1, int2) <= 0;
		}

		/// <summary>
		/// Compares unsinged integer with <see cref="BigInteger" /> object and returns true if first is lighter or equal.
		/// </summary>
		/// <param name="int1">First unsinged integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>True if first is lighter or equal.</returns>
		[CLSCompliant(false)]
		static public bool operator <=(uint int1, BigInteger int2)
		{
			return OpHelper.Cmp(int2, int1) >= 0;
		}

		#endregion operator<=

		#region operator+ and operator-

		/// <summary>
		/// Adds one <see cref="BigInteger" /> object to another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Addition result.</returns>
		static public BigInteger operator +(BigInteger int1, BigInteger int2)
		{
			return OpHelper.AddSub(int1, int2, false);
		}

		/// <summary>
		/// Subtracts one <see cref="BigInteger" /> object from another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Subtraction result.</returns>
		static public BigInteger operator -(BigInteger int1, BigInteger int2)
		{
			return OpHelper.AddSub(int1, int2, true);
		}

		#endregion operator+ and operator-

		#region operator*

		/// <summary>
		/// Multiplies one <see cref="BigInteger" /> object on another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Multiply result.</returns>
		static public BigInteger operator *(BigInteger int1, BigInteger int2)
		{
			return MultiplyManager.GetCurrentMultiplier().Multiply(int1, int2);
		}

		#endregion operator*

		#region operator/ and operator%

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object by another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Division result.</returns>
		static public BigInteger operator /(BigInteger int1, BigInteger int2)
		{
			BigInteger modRes;
			return DivideManager.GetCurrentDivider().DivMod(int1, int2, out modRes, DivModResultFlags.Div);
		}

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object by another and returns division modulo.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Modulo result.</returns>
		static public BigInteger operator %(BigInteger int1, BigInteger int2)
		{
			BigInteger modRes;
			DivideManager.GetCurrentDivider().DivMod(int1, int2, out modRes, DivModResultFlags.Mod);
			return modRes;
		}

		#endregion operator/ and operator%

		#region operator<< and operator>>

		/// <summary>
		/// Shifts <see cref="BigInteger" /> object on selected bits count to the left.
		/// </summary>
		/// <param name="BigInteger">Big integer.</param>
		/// <param name="shift">Bits count.</param>
		/// <returns>Shifting result.</returns>
		static public BigInteger operator <<(BigInteger BigInteger, int shift)
		{
			return OpHelper.Sh(BigInteger, shift, true);
		}

		/// <summary>
		/// Shifts <see cref="BigInteger" /> object on selected bits count to the right.
		/// </summary>
		/// <param name="BigInteger">Big integer.</param>
		/// <param name="shift">Bits count.</param>
		/// <returns>Shifting result.</returns>
		static public BigInteger operator >>(BigInteger BigInteger, int shift)
		{
			return OpHelper.Sh(BigInteger, shift, false);
		}

		#endregion operator<< and operator>>

		#region +, -, ++, -- unary operators

		/// <summary>
		/// Returns the same <see cref="BigInteger" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>The same value, but new object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public BigInteger operator +(BigInteger value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return new BigInteger(value);
		}

		/// <summary>
		/// Returns the same <see cref="BigInteger" /> value, but with other sign.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>The same value, but with other sign.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public BigInteger operator -(BigInteger value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			BigInteger newValue = new BigInteger(value);
			if (newValue._length != 0)
			{
				newValue._negative = !newValue._negative;
			}
			return newValue;
		}

		/// <summary>
		/// Returns increased <see cref="BigInteger" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>Increased value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public BigInteger operator ++(BigInteger value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return value + 1U;
		}

		/// <summary>
		/// Returns decreased <see cref="BigInteger" /> value.
		/// </summary>
		/// <param name="value">Initial value.</param>
		/// <returns>Decreased value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		static public BigInteger operator --(BigInteger value)
		{
			// Exception
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException("value");
			}

			return value - 1U;
		}

		#endregion +, -, ++, -- unary operators

		#region Conversion operators

		#region To BigInteger (Implicit)

		/// <summary>
		/// Implicitly converts <see cref="Int32" /> to <see cref="BigInteger" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator BigInteger(int value)
		{
			return new BigInteger(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt32" /> to <see cref="BigInteger" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public implicit operator BigInteger(uint value)
		{
			return new BigInteger(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt16" /> to <see cref="BigInteger" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public implicit operator BigInteger(ushort value)
		{
			return new BigInteger(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="Int64" /> to <see cref="BigInteger" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public implicit operator BigInteger(long value)
		{
			return new BigInteger(value);
		}

		/// <summary>
		/// Implicitly converts <see cref="UInt64" /> to <see cref="BigInteger" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public implicit operator BigInteger(ulong value)
		{
			return new BigInteger(value);
		}

		#endregion To BigInteger (Implicit)

		#region From BigInteger (Explicit)
		static public explicit operator byte(BigInteger value)
		{
			throw new NotImplementedException();
		}

		static public explicit operator decimal(BigInteger value)
		{
			throw new NotImplementedException();
		}

		static public explicit operator double(BigInteger value)
		{
			throw new NotImplementedException();
		}

		static public explicit operator float(BigInteger value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Explicitly converts <see cref="BigInteger" /> to <see cref="int" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator int(BigInteger value)
		{
			int res = (int)(uint)value;
			return value._negative ? -res : res;
		}

		/// <summary>
		/// Explicitly converts <see cref="BigInteger" /> to <see cref="uint" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public explicit operator uint(BigInteger value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if (value._length == 0) return 0;
			return value._digits[0];
		}

		/// <summary>
		/// Explicitly converts <see cref="BigInteger" /> to <see cref="long" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		static public explicit operator long(BigInteger value)
		{
			long res = (long)(ulong)value;
			return value._negative ? -res : res;
		}

		/// <summary>
		/// Explicitly converts <see cref="BigInteger" /> to <see cref="ulong" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public explicit operator ulong(BigInteger value)
		{
			ulong res = (uint)value;
			if (value._length > 1)
			{
				res |= (ulong)value._digits[1] << Constants.DigitBitCount;
			}
			return res;
		}

		/// <summary>
		/// Explicitly converts <see cref="BigInteger" /> to <see cref="ushort" />.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Conversion result.</returns>
		[CLSCompliant(false)]
		static public explicit operator ushort(BigInteger value)
		{
			return (ushort)(uint)value;
		}

		#endregion From BigInteger (Explicit)

		#endregion Conversion operators

		#endregion Operators

		#region Math static methods

		#region Multiply

		/// <summary>
		/// Multiplies one <see cref="BigInteger" /> object on another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="mode">Multiply mode set explicitly.</param>
		/// <returns>Multiply result.</returns>
		static public BigInteger Multiply(BigInteger int1, BigInteger int2, MultiplyMode mode)
		{
			return MultiplyManager.GetMultiplier(mode).Multiply(int1, int2);
		}

		#endregion Multiply

		#region Divide/modulo

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object by another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="mode">Divide mode.</param>
		/// <returns>Division result.</returns>
		static public BigInteger Divide(BigInteger int1, BigInteger int2, DivideMode mode)
		{
			BigInteger modRes;
			return DivideManager.GetDivider(mode).DivMod(int1, int2, out modRes, DivModResultFlags.Div);
		}

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object by another and returns division modulo.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="mode">Divide mode.</param>
		/// <returns>Modulo result.</returns>
		static public BigInteger Modulo(BigInteger int1, BigInteger int2, DivideMode mode)
		{
			BigInteger modRes;
			DivideManager.GetDivider(mode).DivMod(int1, int2, out modRes, DivModResultFlags.Mod);
			return modRes;
		}

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object on another.
		/// Returns both divident and remainder
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="modRes">Remainder big integer.</param>
		/// <returns>Division result.</returns>
		static public BigInteger DivideModulo(BigInteger int1, BigInteger int2, out BigInteger modRes)
		{
			return DivideManager.GetCurrentDivider().DivMod(int1, int2, out modRes, DivModResultFlags.Div | DivModResultFlags.Mod);
		}

		/// <summary>
		/// Divides one <see cref="BigInteger" /> object on another.
		/// Returns both divident and remainder
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="modRes">Remainder big integer.</param>
		/// <param name="mode">Divide mode.</param>
		/// <returns>Division result.</returns>
		static public BigInteger DivideModulo(BigInteger int1, BigInteger int2, out BigInteger modRes, DivideMode mode)
		{
			return DivideManager.GetDivider(mode).DivMod(int1, int2, out modRes, DivModResultFlags.Div | DivModResultFlags.Mod);
		}

		#endregion Divide/modulo

		#region Pow

		/// <summary>
		/// Returns a specified big integer raised to the specified power.
		/// </summary>
		/// <param name="value">Number to raise.</param>
		/// <param name="power">Power.</param>
		/// <returns>Number in given power.</returns>
		[CLSCompliant(false)]
		static public BigInteger Pow(BigInteger value, uint power)
		{
			return OpHelper.Pow(value, power, GlobalSettings.MultiplyMode);
		}

		/// <summary>
		/// Returns a specified big integer raised to the specified power.
		/// </summary>
		/// <param name="value">Number to raise.</param>
		/// <param name="power">Power.</param>
		/// <param name="multiplyMode">Multiply mode set explicitly.</param>
		/// <returns>Number in given power.</returns>
		[CLSCompliant(false)]
		static public BigInteger Pow(BigInteger value, uint power, MultiplyMode multiplyMode)
		{
			return OpHelper.Pow(value, power, multiplyMode);
		}

		#endregion Pow

		#endregion Math static methods

		#region ToString override

		/// <summary>
		/// Returns decimal string representation of this <see cref="BigInteger" /> object.
		/// </summary>
		/// <returns>Decimal number in string.</returns>
		override public string ToString()
		{
			return ToString(10U, true);
		}

		public string ToString(CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns string representation of this <see cref="BigInteger" /> object in given base.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <returns>Object string representation.</returns>
		[CLSCompliant(false)]
		public string ToString(uint numberBase)
		{
			return ToString(numberBase, true);
		}

		/// <summary>
		/// Returns string representation of this <see cref="BigInteger" /> object in given base.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="upperCase">Use uppercase for bases from 11 to 16 (which use letters A-F).</param>
		/// <returns>Object string representation.</returns>
		[CLSCompliant(false)]
		public string ToString(uint numberBase, bool upperCase)
		{
			return StringConvertManager.GetStringConverter(ToStringMode)
				.ToString(this, numberBase, upperCase ? Constants.BaseUpperChars : Constants.BaseLowerChars);
		}

		/// <summary>
		/// Returns string representation of this <see cref="BigInteger" /> object in given base using custom alphabet.
		/// </summary>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Object string representation.</returns>
		[CLSCompliant(false)]
		public string ToString(uint numberBase, string alphabet)
		{
			StrRepHelper.AssertAlphabet(alphabet, numberBase);
			return StringConvertManager.GetStringConverter(ToStringMode)
				.ToString(this, numberBase, alphabet.ToCharArray());
		}
		#endregion ToString override

		#region Parsing methods

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object in decimal base.
		/// If number starts from "0" then it's treated as octal; if number starts fropm "0x"
		/// then it's treated as hexadecimal.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <returns>Parsed object.</returns>
		static public BigInteger Parse(string value)
		{
			return ParseManager.GetCurrentParser().Parse(value, 10U, Constants.BaseCharToDigits, true);
		}

		/// <summary>
		/// Parse the specified value and culture.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="culture">Culture.</param>
		static public BigInteger Parse(string value, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <returns>Parsed object.</returns>
		[CLSCompliant(false)]
		static public BigInteger Parse(string value, uint numberBase)
		{
			return ParseManager.GetCurrentParser().Parse(value, numberBase, Constants.BaseCharToDigits, false);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object using custom alphabet.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Parsed object.</returns>
		[CLSCompliant(false)]
		static public BigInteger Parse(string value, uint numberBase, string alphabet)
		{
			return ParseManager.GetCurrentParser()
				.Parse(value, numberBase, StrRepHelper.CharDictionaryFromAlphabet(alphabet, numberBase), false);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object in decimal base.
		/// If number starts from "0" then it's treated as octal; if number starts fropm "0x"
		/// then it's treated as hexadecimal.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="mode">Parse mode.</param>
		/// <returns>Parsed object.</returns>
		static public BigInteger Parse(string value, ParseMode mode)
		{
			return ParseManager.GetParser(mode).Parse(value, 10U, Constants.BaseCharToDigits, true);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="mode">Parse mode.</param>
		/// <returns>Parsed object.</returns>
		[CLSCompliant(false)]
		static public BigInteger Parse(string value, uint numberBase, ParseMode mode)
		{
			return ParseManager.GetParser(mode).Parse(value, numberBase, Constants.BaseCharToDigits, false);
		}

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object using custom alphabet.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <param name="mode">Parse mode.</param>
		/// <returns>Parsed object.</returns>
		[CLSCompliant(false)]
		static public BigInteger Parse(string value, uint numberBase, string alphabet, ParseMode mode)
		{
			return ParseManager.GetParser(mode)
				.Parse(value, numberBase, StrRepHelper.CharDictionaryFromAlphabet(alphabet, numberBase), false);
		}

		#endregion Parsing methods

		#region IEquatable/Equals/GetHashCode implementation/overrides

		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another big integer.
		/// </summary>
		/// <param name="n">Big integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(BigInteger n)
		{
			return base.Equals(n) || this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another integer.
		/// </summary>
		/// <param name="n">Integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(int n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another unsigned integer.
		/// </summary>
		/// <param name="n">Unsigned integer to compare with.</param>
		/// <returns>True if equals.</returns>
		[CLSCompliant(false)]
		public bool Equals(uint n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another long integer.
		/// </summary>
		/// <param name="n">Long integer to compare with.</param>
		/// <returns>True if equals.</returns>
		public bool Equals(long n)
		{
			return this == n;
		}

		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another unsigned long integer.
		/// </summary>
		/// <param name="n">Unsigned long integer to compare with.</param>
		/// <returns>True if equals.</returns>
		[CLSCompliant(false)]
		public bool Equals(ulong n)
		{
			return this == n;
		}


		/// <summary>
		/// Returns equality of this <see cref="BigInteger" /> with another object.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>True if equals.</returns>
		override public bool Equals(object obj)
		{
			return obj is BigInteger && Equals((BigInteger)obj);
		}

		/// <summary>
		/// Returns hash code for this <see cref="BigInteger" /> object.
		/// </summary>
		/// <returns>Object hash code.</returns>
		override public int GetHashCode()
		{
			switch (_length)
			{
				case 0:
					return 0;
				case 1:
					return (int)(_digits[0] ^ _length ^ (_negative ? 1 : 0));
				default:
					return (int)(_digits[0] ^ _digits[_length - 1] ^ _length ^ (_negative ? 1 : 0));
			}
		}

		#endregion Equals/GetHashCode implementation/overrides

		#region IComparable implementation

		/// <summary>
		/// Compares current object with another big integer.
		/// </summary>
		/// <param name="n">Big integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(BigInteger n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another integer.
		/// </summary>
		/// <param name="n">Integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(int n)
		{
			return OpHelper.Cmp(this, n);
		}

		/// <summary>
		/// Compares current object with another unsigned integer.
		/// </summary>
		/// <param name="n">Unsigned integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		[CLSCompliant(false)]
		public int CompareTo(uint n)
		{
			return OpHelper.Cmp(this, n);
		}

		/// <summary>
		/// Compares current object with another long integer.
		/// </summary>
		/// <param name="n">Long integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		public int CompareTo(long n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another unsigned long integer.
		/// </summary>
		/// <param name="n">Unsigned long integer to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="n" />, -1 if object is smaller than <paramref name="n" />, 0 if they are equal.</returns>
		[CLSCompliant(false)]
		public int CompareTo(ulong n)
		{
			return OpHelper.Cmp(this, n, true);
		}

		/// <summary>
		/// Compares current object with another object.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>1 if object is bigger than <paramref name="obj" />, -1 if object is smaller than <paramref name="obj" />, 0 if they are equal.</returns>
		public int CompareTo(object obj)
		{
			if (obj is BigInteger)
			{
				return CompareTo((BigInteger)obj);
			}
			else if (obj is int)
			{
				return CompareTo((int)obj);
			}
			else if (obj is uint)
			{
				return CompareTo((uint)obj);
			}
			else if (obj is long)
			{
				return CompareTo((long)obj);
			}
			else if (obj is ulong)
			{
				return CompareTo((ulong)obj);
			}

			throw new ArgumentException(Strings.CantCmp, "obj");
		}

		#endregion IComparable implementation

		#region Other public methods

		/// <summary>
		/// Frees extra space not used by digits.
		/// </summary>
		public void Normalize()
		{
			if (_digits.LongLength > _length)
			{
				uint[] newDigits = new uint[_length];
				Array.Copy(_digits, newDigits, _length);
				_digits = newDigits;
			}

			if (_length == 0)
			{
				_negative = false;
			}
		}

		/// <summary>
		/// Retrieves this <see cref="BigInteger" /> internal state as digits array and sign.
		/// Can be used for serialization and other purposes.
		/// Note: please use constructor instead to clone <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="digits">Digits array.</param>
		/// <param name="negative">Is negative integer.</param>
		[CLSCompliant(false)]
		public void GetInternalState(out uint[] digits, out bool negative)
		{
			digits = new uint[_length];
			Array.Copy(_digits, digits, _length);

			negative = _negative;
		}

		#endregion Other public methods

		#region Init utilitary methods
		/// <summary>
		/// Initializes class instance from <see cref="UInt64" /> value.
		/// Doesn't initialize sign.
		/// For internal use.
		/// </summary>
		/// <param name="value">Unsigned long value.</param>
		void InitFromUlong(ulong value)
		{
			// Divide ulong into 2 uint values
			uint low = (uint)value;
			uint high = (uint)(value >> Constants.DigitBitCount);

			// Prepare internal fields
			if (high == 0)
			{
				_digits = new uint[] { low };
			}
			else
			{
				_digits = new uint[] { low, high };
			}
			_length = (uint)_digits.Length;
		}

		/// <summary>
		/// Initializes class instance from another <see cref="BigInteger" /> value.
		/// For internal use.
		/// </summary>
		/// <param name="value">Big integer value.</param>
		void InitFromBigInteger(BigInteger value)
		{
			_digits = value._digits;
			_length = value._length;
			_negative = value._negative;
		}

		/// <summary>
		/// Initializes class instance from digits array.
		/// For internal use.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="negative">Big integer sign.</param>
		/// <param name="length">Big integer length.</param>
		void InitFromDigits(uint[] digits, bool negative, uint length)
		{
			_digits = new uint[_length = length];
			Array.Copy(digits, _digits, System.Math.Min((uint)digits.LongLength, length));
			if (length != 0)
			{
				_negative = negative;
			}
		}

		#endregion Init utilitary methods

		#region Other utilitary methods

		/// <summary>
		/// Frees extra space not used by digits only if auto-normalize is set for the instance.
		/// </summary>
		internal void TryNormalize()
		{
			if (AutoNormalize)
			{
				Normalize();
			}
		}

		#endregion

		public byte[] ToByteArray()
		{
			return DigitConverter.ToBytes(_digits);
		}
	}
}
