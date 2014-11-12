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
using OpenGamingLibrary.Numerics.Converters;
using OpenGamingLibrary.Numerics.OpHelpers;

namespace OpenGamingLibrary.Numerics.Converters
{
	/// <summary>
	/// Classic ToString converting algorithm using division (O[n^2]).
	/// </summary>
	sealed internal class ClassicStringConverter : StringConverterBase
	{
		#region Constructor

		/// <summary>
		/// Creates new <see cref="ClassicStringConverter" /> instance.
		/// </summary>
		/// <param name="pow2StringConverter">Converter for pow2 case.</param>
		public ClassicStringConverter(IStringConverter pow2StringConverter) : base(pow2StringConverter) {}

		#endregion Constructor

		/// <summary>
		/// Converts digits from internal representaion into given base.
		/// </summary>
		/// <param name="digits">Big integer digits.</param>
		/// <param name="length">Big integer length.</param>
		/// <param name="numberBase">Base to use for output.</param>
		/// <param name="outputLength">Calculated output length (will be corrected inside).</param>
		/// <returns>Conversion result (later will be transformed to string).</returns>
		override public uint[] ToString(uint[] digits, uint length, uint numberBase, ref uint outputLength)
		{
			uint[] outputArray = base.ToString(digits, length, numberBase, ref outputLength);

			// Maybe base method already converted this number
			if (outputArray != null) return outputArray;

			// Create an output array for storing of number in other base
			outputArray = new uint[outputLength + 1];

			// Make a copy of initial data
			uint[] digitsCopy = new uint[length];
			Array.Copy(digits, digitsCopy, length);

			// Calculate output numbers by dividing
			uint outputIndex;
			for (outputIndex = 0; length > 0; ++outputIndex)
			{
				length = DigitOpHelper.DivMod(digitsCopy, length, numberBase, digitsCopy, out outputArray[outputIndex]);
			}

			outputLength = outputIndex;
			return outputArray;
		}
	}
}
