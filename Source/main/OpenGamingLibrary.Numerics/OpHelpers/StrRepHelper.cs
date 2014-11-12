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

namespace OpenGamingLibrary.Numerics.OpHelpers
{
	/// <summary>
	/// Helps to work with <see cref="BigInteger" /> string representations.
	/// </summary>
	static internal class StrRepHelper
	{
		/// <summary>
		/// Returns digit for given char.
		/// </summary>
		/// <param name="charToDigits">Char->digit dictionary.</param>
		/// <param name="ch">Char which represents big integer digit.</param>
		/// <param name="numberBase">String representation number base.</param>
		/// <returns>Digit.</returns>
		/// <exception cref="FormatException"><paramref name="ch" /> is not in valid format.</exception>
		static public uint GetDigit(IDictionary<char, uint> charToDigits, char ch, uint numberBase)
		{
			if (charToDigits == null)
			{
				throw new ArgumentNullException("charToDigits");
			}

			// Try to identify this digit
			uint digit;
			if (!charToDigits.TryGetValue(ch, out digit))
			{
				throw new FormatException(Strings.ParseInvalidChar);
			}
			if (digit >= numberBase)
			{
				throw new FormatException(Strings.ParseTooBigDigit);
			}
			return digit;
		}

		/// <summary>
		/// Verfies string alphabet provider by user for validity.
		/// </summary>
		/// <param name="alphabet">Alphabet.</param>
		/// <param name="numberBase">String representation number base.</param>
		static public void AssertAlphabet(string alphabet, uint numberBase)
		{
			if (alphabet == null)
			{
				throw new ArgumentNullException("alphabet");
			}

			// Ensure that alphabet has enough characters to represent numbers in given base
			if (alphabet.Length < numberBase)
			{
				throw new ArgumentException(string.Format(Strings.AlphabetTooSmall, numberBase), "alphabet");
			}

			// Ensure that all the characters in alphabet are unique
			char[] sortedChars = alphabet.ToCharArray();
			Array.Sort(sortedChars);
			for (int i = 0; i < sortedChars.Length; i++)
			{
				if (i > 0 && sortedChars[i] == sortedChars[i - 1])
				{
					throw new ArgumentException(Strings.AlphabetRepeatingChars, "alphabet");
				}
			}
		}

		/// <summary>
		/// Generates char->digit dictionary from alphabet.
		/// </summary>
		/// <param name="alphabet">Alphabet.</param>
		/// <param name="numberBase">String representation number base.</param>
		/// <returns>Char->digit dictionary.</returns>
		static public IDictionary<char, uint> CharDictionaryFromAlphabet(string alphabet, uint numberBase)
		{
			AssertAlphabet(alphabet, numberBase);
			Dictionary<char, uint> charToDigits = new Dictionary<char, uint>((int)numberBase);
			for (int i = 0; i < numberBase; i++)
			{
				charToDigits.Add(alphabet[i], (uint)i);
			}
			return charToDigits;
		}
	}
}
