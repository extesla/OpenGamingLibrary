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

namespace OpenGamingLibrary.Numerics.Dividers
{
	/// <summary>
	/// Divider class interface.
	/// </summary>
	internal interface IDivider
	{
		/// <summary>
		/// Divides one <see cref="BigInteger" /> by another.
		/// </summary>
		/// <param name="int1">First big integer.</param>
		/// <param name="int2">Second big integer.</param>
		/// <param name="modRes">Remainder big integer.</param>
		/// <param name="resultFlags">Which operation results to return.</param>
		/// <returns>Divident big integer.</returns>
		BigInteger DivMod(BigInteger int1, BigInteger int2, out BigInteger modRes, DivModResultFlags resultFlags);

		/// <summary>
		/// Divides two big integers.
		/// Also modifies <paramref name="digits1" /> and <paramref name="length1"/> (it will contain remainder).
		/// </summary>
		/// <param name="digits1">First big integer digits.</param>
		/// <param name="digitsBuffer1">Buffer for first big integer digits. May also contain remainder. Can be null - in this case it's created if necessary.</param>
		/// <param name="length1">First big integer length.</param>
		/// <param name="digits2">Second big integer digits.</param>
		/// <param name="digitsBuffer2">Buffer for second big integer digits. Only temporarily used. Can be null - in this case it's created if necessary.</param>
		/// <param name="length2">Second big integer length.</param>
		/// <param name="digitsRes">Resulting big integer digits.</param>
		/// <param name="resultFlags">Which operation results to return.</param>
		/// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
		/// <returns>Resulting big integer length.</returns>
		uint DivMod(
			uint[] digits1,
			uint[] digitsBuffer1,
			ref uint length1,
			uint[] digits2,
			uint[] digitsBuffer2,
			uint length2,
			uint[] digitsRes,
			DivModResultFlags resultFlags,
			int cmpResult);

		/// <summary>
		/// Divides two big integers.
		/// Also modifies <paramref name="digitsPtr1" /> and <paramref name="length1"/> (it will contain remainder).
		/// </summary>
		/// <param name="digitsPtr1">First big integer digits.</param>
		/// <param name="digitsBufferPtr1">Buffer for first big integer digits. May also contain remainder.</param>
		/// <param name="length1">First big integer length.</param>
		/// <param name="digitsPtr2">Second big integer digits.</param>
		/// <param name="digitsBufferPtr2">Buffer for second big integer digits. Only temporarily used.</param>
		/// <param name="length2">Second big integer length.</param>
		/// <param name="digitsResPtr">Resulting big integer digits.</param>
		/// <param name="resultFlags">Which operation results to return.</param>
		/// <param name="cmpResult">Big integers comparsion result (pass -2 if omitted).</param>
		/// <returns>Resulting big integer length.</returns>
		unsafe uint DivMod(
			uint* digitsPtr1,
			uint* digitsBufferPtr1,
			ref uint length1,
			uint* digitsPtr2,
			uint* digitsBufferPtr2,
			uint length2,
			uint* digitsResPtr,
			DivModResultFlags resultFlags,
			int cmpResult);
	}
}