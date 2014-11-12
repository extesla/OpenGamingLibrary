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

namespace OpenGamingLibrary.Numerics.Multipliers
{
	/// <summary>
	/// Multiplier class interface.
	/// </summary>
	internal interface IMultiplier
	{
		/// <summary>
		/// Multiplies two big integers.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <returns>Resulting big integer.</returns>
		BigInteger Multiply(BigInteger int1, BigInteger int2);
		
		/// <summary>
		/// Multiplies two big integers represented by their digits.
		/// </summary>
		/// <param name="digits1">First big integer digits.</param>
		/// <param name="length1">First big integer real length.</param>
		/// <param name="digits2">Second big integer digits.</param>
		/// <param name="length2">Second big integer real length.</param>
		/// <param name="digitsRes">Where to put resulting big integer.</param>
		/// <returns>Resulting big integer real length.</returns>
		uint Multiply(uint[] digits1, uint length1, uint[] digits2, uint length2, uint[] digitsRes);
		
		/// <summary>
		/// Multiplies two big integers using pointers.
		/// </summary>
		/// <param name="digitsPtr1">First big integer digits.</param>
		/// <param name="length1">First big integer length.</param>
		/// <param name="digitsPtr2">Second big integer digits.</param>
		/// <param name="length2">Second big integer length.</param>
		/// <param name="digitsResPtr">Resulting big integer digits.</param>
		/// <returns>Resulting big integer length.</returns>
		unsafe uint Multiply(uint* digitsPtr1, uint length1, uint* digitsPtr2, uint length2, uint* digitsResPtr);
	}
}
