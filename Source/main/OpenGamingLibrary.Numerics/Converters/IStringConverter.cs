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

namespace OpenGamingLibrary.Numerics.Converters
{
	/// <summary>
	/// ToString converter class interface.
	/// </summary>
	internal interface IStringConverter
	{
		/// <summary>
		/// Returns string representation of <see cref="BigInteger" /> object in given base.
		/// </summary>
		/// <param name="BigInteger">Big integer to convert.</param>
		/// <param name="numberBase">Base of system in which to do output.</param>
		/// <param name="alphabet">Alphabet which contains chars used to represent big integer, char position is coresponding digit value.</param>
		/// <returns>Object string representation.</returns>
		string ToString(BigInteger BigInteger, uint numberBase, char[] alphabet);
		
		/// <summary>
		/// Converts digits from internal representaion into given base.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="length">Big integer length.</param>
		/// <param name="numberBase">Base to use for output.</param>
		/// <param name="outputLength">Calculated output length (will be corrected inside).</param>
		/// <returns>Conversion result (later will be transformed to string).</returns>
		uint[] ToString(uint[] digits, uint length, uint numberBase, ref uint outputLength);
	}
}
