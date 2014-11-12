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
	/// Contains helping methods to with with bits in dword (<see cref="UInt32" />).
	/// </summary>
	[CLSCompliant(false)]
	static public class Bits
	{
		/// <summary>
		/// Returns number of leading zero bits in int.
		/// </summary>
		/// <param name="x">Int value.</param>
		/// <returns>Number of leading zero bits.</returns>
		static public int Nlz(uint x)
		{
			if (x == 0) return 32;
			
			int n = 1;
			if ((x >> 16) == 0) { n += 16; x <<= 16; }
			if ((x >> 24) == 0) { n +=  8; x <<=  8; }
			if ((x >> 28) == 0) { n +=  4; x <<=  4; }
			if ((x >> 30) == 0) { n +=  2; x <<=  2; }
			return n - (int)(x >> 31);
		}
		
		/// <summary>
		/// Counts position of the most significant bit in int.
		/// Can also be used as Floor(Log2(<paramref name="x" />)).
		/// </summary>
		/// <param name="x">Int value.</param>
		/// <returns>Position of the most significant one bit (-1 if all zeroes).</returns>
		static public int Msb(uint x)
		{
			return 31 - Nlz(x);
		}
		
		/// <summary>
		/// Ceil(Log2(<paramref name="x" />)).
		/// </summary>
		/// <param name="x">Int value.</param>
		/// <returns>Ceil of the Log2.</returns>
		static public int CeilLog2(uint x)
		{
			int msb = Msb(x);
			if (x != 1U << msb)
			{
				++msb;
			}
			return msb;
		}
	}
}
