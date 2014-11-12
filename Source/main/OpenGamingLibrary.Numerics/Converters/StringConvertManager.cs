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
using OpenGamingLibrary.Numerics.Utils;

namespace OpenGamingLibrary.Numerics.Converters
{
	/// <summary>
	/// Used to retrieve needed ToString converter.
	/// </summary>
	static internal class StringConvertManager
	{
		#region Fields

		/// <summary>
		/// Classic converter instance.
		/// </summary>
		static readonly public IStringConverter ClassicStringConverter;

		/// <summary>
		/// Fast converter instance.
		/// </summary>
		static readonly public IStringConverter FastStringConverter;

		#endregion Fields

		#region Constructors

		// .cctor
		static StringConvertManager()
		{
			// Create new pow2 converter instance
			IStringConverter pow2StringConverter = new Pow2StringConverter();

			// Create new classic converter instance
			IStringConverter classicStringConverter = new ClassicStringConverter(pow2StringConverter);

			// Fill publicity visible converter fields
			ClassicStringConverter = classicStringConverter;
			FastStringConverter = new FastStringConverter(pow2StringConverter, classicStringConverter);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Returns ToString converter instance for given ToString mode.
		/// </summary>
		/// <param name="mode">ToString mode.</param>
		/// <returns>Converter instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="mode" /> is out of range.</exception>
		static public IStringConverter GetStringConverter(ToStringMode mode)
		{
			switch (mode)
			{
				case ToStringMode.Fast:
					return FastStringConverter;
				case ToStringMode.Classic:
					return ClassicStringConverter;
				default:
					throw new ArgumentOutOfRangeException("mode");
			}
		}

		#endregion Methods
	}
}
