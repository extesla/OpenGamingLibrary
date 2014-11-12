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

namespace OpenGamingLibrary.Numerics
{
	/// <summary>
	/// Converts <see cref="BigInteger"/> digits to/from byte array.
	/// </summary>
	[CLSCompliant(false)]
	static public class DigitConverter
	{
		/// <summary>
		/// Converts big integer digits to bytes.
		/// </summary>
		/// <param name="digits"><see cref="BigInteger" /> digits.</param>
		/// <returns>Resulting bytes.</returns>
		/// <remarks>
		/// Digits can be obtained using <see cref="BigInteger.GetInternalState" /> method.
		/// </remarks>
		static public byte[] ToBytes(uint[] digits)
		{
			if (digits == null)
			{
				throw new ArgumentNullException("digits");
			}

			byte[] bytes = new byte[digits.Length * 4];
			Buffer.BlockCopy(digits, 0, bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary>
		/// Converts bytes to big integer digits.
		/// </summary>
		/// <param name="bytes">Bytes.</param>
		/// <returns>Resulting <see cref="BigInteger" /> digits.</returns>
		/// <remarks>
		/// Big integer can be created from digits using <see cref="BigInteger(uint[], bool)" /> constructor.
		/// </remarks>
		static public uint[] FromBytes(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			if (bytes.Length % 4 != 0)
			{
				throw new ArgumentException(Strings.DigitBytesLengthInvalid, "bytes");
			}

			uint[] digits = new uint[bytes.Length / 4];
			Buffer.BlockCopy(bytes, 0, digits, 0, bytes.Length);
			return digits;
		}
	}
}
