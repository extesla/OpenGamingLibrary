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

namespace OpenGamingLibrary.Numerics.Utils
{
	/// <summary>
	/// Constants used in <see cref="BigInteger" /> and helping classes.
	/// </summary>
	static internal class Constants
	{
		#region .cctor

		static Constants()
		{
			BaseCharToDigits = StrRepHelper.CharDictionaryFromAlphabet(new string(BaseUpperChars), 16U);
			for (int i = 10; i < BaseLowerChars.Length; i++)
			{
				BaseCharToDigits.Add(BaseLowerChars[i], (uint)i);
			}
		}

		#endregion .cctor


		#region ToString constants

		/// <summary>
		/// Chars used to parse/output big integers (upper case).
		/// </summary>
		public static readonly char[] BaseUpperChars = "0123456789ABCDEF".ToCharArray();
		
		/// <summary>
		/// Chars used to parse/output big integers (lower case).
		/// </summary>
		public static readonly char[] BaseLowerChars = "0123456789abcdef".ToCharArray();

		/// <summary>
		/// Standard char->digit dictionary.
		/// </summary>
		static readonly public IDictionary<char, uint> BaseCharToDigits;
		
		/// <summary>
		/// Digit opening bracet (used for bases bigger then 16).
		/// </summary>
		public const char DigitOpeningBracet = '{';
		
		/// <summary>
		/// Digit closing bracet (used for bases bigger then 16).
		/// </summary>
		public const char DigitClosingBracet = '}';
		
		/// <summary>
		/// Minus char (-).
		/// </summary>
		public const char DigitsMinusChar = '-';
		
		/// <summary>
		/// Natural logarithm of digits base (log(2^32)).
		/// </summary>
		public static readonly double DigitBaseLog = System.Math.Log(1UL << DigitBitCount);
		
		#endregion ToString constants
		
		#region Pools constants
		
		/// <summary>
		/// Minimal Log2(array length) which will be pooled using any array pool.
		/// </summary>
		public const int MinPooledArraySizeLog2 = 17;
		
		/// <summary>
		/// Maximal Log2(array length) which will be pooled using any array pool.
		/// </summary>
		public const int MaxPooledArraySizeLog2 = 31;
		
		/// <summary>
		/// Maximal allowed array pool items count in each stack.
		/// </summary>
		public const int MaxArrayPoolCount = 1024;
		
		#endregion Pools constants
		
		#region FHT constants
		
		/// <summary>
		/// <see cref="BigInteger" /> length from which FHT is used (in auto-FHT mode).
		/// Before this length usual multiply algorithm works faster.
		/// </summary>
		public const uint AutoFhtLengthLowerBound = 1U << 9;
		
		/// <summary>
		/// <see cref="BigInteger" /> length 'till which FHT is used (in auto-FHT mode).
		/// After this length using of FHT may be unsafe due to big precision errors.
		/// </summary>
		public const uint AutoFhtLengthUpperBound = 1U << 26;

		/// <summary>
		/// Number of lower digits used to check FHT multiplication result validity.
		/// </summary>
		public const uint FhtValidityCheckDigitCount = 10;

		#endregion FHT constants

		#region Newton constants

		/// <summary>
		/// <see cref="BigInteger" /> length from which Newton approach is used (in auto-Newton mode).
		/// Before this length usual divide algorithm works faster.
		/// </summary>
		public const uint AutoNewtonLengthLowerBound = 1U << 13;

		/// <summary>
		/// <see cref="BigInteger" /> length 'till which Newton approach is used (in auto-Newton mode).
		/// After this length using of fast division may be slow.
		/// </summary>
		public const uint AutoNewtonLengthUpperBound = 1U << 26;

		#endregion Newton constants

		#region Parsing constants

		/// <summary>
		/// <see cref="BigInteger" /> length from which fast parsing is used (in Fast parsing mode).
		/// Before this length usual parsing algorithm works faster.
		/// </summary>
		public const uint FastParseLengthLowerBound = 32;
		
		/// <summary>
		/// <see cref="BigInteger" /> length 'till which fast parsing is used (in Fast parsing mode).
		/// After this length using of parsing will be slow.
		/// </summary>
		public const uint FastParseLengthUpperBound = uint.MaxValue;
		
		#endregion Parsing constants
		
		#region ToString convertion constants
		
		/// <summary>
		/// <see cref="BigInteger" /> length from which fast convertion is used (in Fast convert mode).
		/// Before this length usual convertion algorithm works faster.
		/// </summary>
		public const uint FastConvertLengthLowerBound = 16;
		
		/// <summary>
		/// <see cref="BigInteger" /> length 'till which fast convertion is used (in Fast convert mode).
		/// After this length using of convertion will be slow.
		/// </summary>
		public const uint FastConvertLengthUpperBound = uint.MaxValue;
		
		#endregion ToString convertion constants
		
		/// <summary>
		/// Count of bits in one <see cref="BigInteger" /> digit.
		/// </summary>
		public const int DigitBitCount = 32;
		
		/// <summary>
		/// Maximum count of bits which can fit in <see cref="BigInteger" />.
		/// </summary>
		public const ulong MaxBitCount = uint.MaxValue * 32UL;
		
		/// <summary>
		/// 2^<see cref="DigitBitCount"/>.
		/// </summary>
		public const ulong BitCountStepOf2 = 1UL << 32;
	}
}
