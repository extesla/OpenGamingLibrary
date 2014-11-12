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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Parsers
{
	/// <summary>
	/// Base class for parsers.
	/// Contains default implementations of parse operation over <see cref="BigInteger" /> instances.
	/// </summary>
	abstract internal class ParserBase : IParser
	{
		#region Private constants

		// Regex pattern used on parsing stage to determine number sign and/or base
		const string ParseRegexPattern = "(?<Sign>[+-]?)((?<BaseHex>0[Xx])|(?<BaseOct>0))?";

		// Regex object used on parsing stage to determine number sign and/or base
		static readonly Regex ParseRegex = new Regex(ParseRegexPattern, RegexOptions.Compiled);

		#endregion Private constants

		#region Private fields

		IParser _pow2Parser; // parser for pow2 case

		#endregion Private fields

		#region Constructor

		/// <summary>
		/// Creates new <see cref="ParserBase" /> instance.
		/// </summary>
		/// <param name="pow2Parser">Parser for pow2 case.</param>
		public ParserBase(IParser pow2Parser)
		{
			_pow2Parser = pow2Parser;
		}

		#endregion Constructor

		/// <summary>
		/// Parses provided string representation of <see cref="BigInteger" /> object.
		/// </summary>
		/// <param name="value">Number as string.</param>
		/// <param name="numberBase">Number base.</param>
		/// <param name="charToDigits">Char->digit dictionary.</param>
		/// <param name="checkFormat">Check actual format of number (0 or 0x at start).</param>
		/// <returns>Parsed object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is a null reference.</exception>
		/// <exception cref="ArgumentException"><paramref name="numberBase" /> is less then 2 or more then 16.</exception>
		/// <exception cref="FormatException"><paramref name="value" /> is not in valid format.</exception>
		virtual public BigInteger Parse(string value, uint numberBase, IDictionary<char, uint> charToDigits, bool checkFormat)
		{
			// Exceptions
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (charToDigits == null)
			{
				throw new ArgumentNullException("charToDigits");
			}
			if (numberBase < 2 || numberBase > charToDigits.Count)
			{
				throw new ArgumentException(Strings.ParseBaseInvalid, "numberBase");
			}

			// Initially determine start and end indices (Trim is too slow)
			int startIndex = 0;
			for (; startIndex < value.Length && char.IsWhiteSpace(value[startIndex]); ++startIndex) ;
			int endIndex = value.Length - 1;
			for (; endIndex >= startIndex && char.IsWhiteSpace(value[endIndex]); --endIndex) ;

			bool negative = false; // number sign
			bool stringNotEmpty = false; // true if string is already guaranteed to be non-empty

			// Determine sign and/or base
			Match match = ParseRegex.Match(value, startIndex, endIndex - startIndex + 1);
			if (match.Groups["Sign"].Value == "-")
			{
				negative = true;
			}
			if (match.Groups["BaseHex"].Length != 0)
			{
				if (checkFormat)
				{
					// 0x before the number - this is hex number
					numberBase = 16U;
				}
				else
				{
					// This is an error
					throw new FormatException(Strings.ParseInvalidChar);
				}
			}
			else if (match.Groups["BaseOct"].Length != 0)
			{
				if (checkFormat)
				{
					// 0 before the number - this is octal number
					numberBase = 8U;
				}

				stringNotEmpty = true;
			}

			// Skip leading sign and format
			startIndex += match.Length;

			// If on this stage string is empty, this may mean an error
			if (startIndex > endIndex && !stringNotEmpty)
			{
				throw new FormatException(Strings.ParseNoDigits);
			}

			// Iterate thru string and skip all leading zeroes
			for (; startIndex <= endIndex && value[startIndex] == '0'; ++startIndex) ;

			// Return zero if length is zero
			if (startIndex > endIndex) return new BigInteger(0);

			// Determine length of new digits array and create new BigInteger object with given length
			int valueLength = endIndex - startIndex + 1;
			uint digitsLength = (uint)System.Math.Ceiling(System.Math.Log(numberBase) / Constants.DigitBaseLog * valueLength);
			BigInteger newInt = new BigInteger(digitsLength, negative);

			// Now we have only (in)valid string which consists from numbers only.
			// Parse it
			newInt._length = Parse(value, startIndex, endIndex, numberBase, charToDigits, newInt._digits);

			return newInt;
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
		virtual public uint Parse(string value, int startIndex, int endIndex, uint numberBase, IDictionary<char, uint> charToDigits, uint[] digitsRes)
		{
			// Default implementation - always call pow2 parser if numberBase is pow of 2
			return numberBase == 1U << Bits.Msb(numberBase)
				? _pow2Parser.Parse(value, startIndex, endIndex, numberBase, charToDigits, digitsRes)
				: 0;
		}
	}
}
