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
using OpenGamingLibrary.Numerics.OpHelpers;

namespace OpenGamingLibrary.Numerics.Multipliers
{
	/// <summary>
	/// Multiplies using "classic" algorithm.
	/// </summary>
	sealed internal class ClassicMultiplier : MultiplierBase
	{
		/// <summary>
		/// Multiplies two big integers using pointers.
		/// </summary>
		/// <param name="digitsPtr1">First big integer digits.</param>
		/// <param name="length1">First big integer length.</param>
		/// <param name="digitsPtr2">Second big integer digits.</param>
		/// <param name="length2">Second big integer length.</param>
		/// <param name="digitsResPtr">Resulting big integer digits.</param>
		/// <returns>Resulting big integer length.</returns>
		override unsafe public uint Multiply(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr)
		{
			ulong c;

			// External cycle must be always smaller
			if (length1 < length2)
			{
				// First must be bigger - swap
				uint lengthTemp = length1;
				length1 = length2;
				length2 = lengthTemp;

				uint* ptrTemp = digitsPtr1;
				digitsPtr1 = digitsPtr2;
				digitsPtr2 = ptrTemp;
			}

			// Prepare end pointers
			uint* digitsPtr1End = digitsPtr1 + length1;
			uint* digitsPtr2End = digitsPtr2 + length2;

			// We must always clear first "length1" digits in result
			DigitHelper.SetBlockDigits(digitsResPtr, length1, 0U);

			// Perform digits multiplication
			uint* ptr1, ptrRes = null;
			for (; digitsPtr2 < digitsPtr2End; ++digitsPtr2, ++digitsResPtr)
			{
				// Check for zero (sometimes may help). There is no sense to make this check in internal cycle -
				// it would give performance gain only here
				if (*digitsPtr2 == 0) continue;

				c = 0;
				for (ptr1 = digitsPtr1, ptrRes = digitsResPtr; ptr1 < digitsPtr1End; ++ptr1, ++ptrRes)
				{
					c += (ulong)*digitsPtr2 * *ptr1 + *ptrRes;
					*ptrRes = (uint)c;
					c >>= 32;
				}
				*ptrRes = (uint)c;
			}

			uint newLength = length1 + length2;
			if (newLength > 0 && (ptrRes == null || *ptrRes == 0))
			{
				--newLength;
			}
			return newLength;
		}
	}
}
