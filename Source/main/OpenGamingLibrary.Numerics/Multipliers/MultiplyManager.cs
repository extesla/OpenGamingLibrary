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

namespace OpenGamingLibrary.Numerics.Multipliers
{
	/// <summary>
	/// Used to retrieve needed multiplier.
	/// </summary>
	static internal class MultiplyManager
	{
		#region Fields

		/// <summary>
		/// Classic multiplier instance.
		/// </summary>
		static readonly public IMultiplier ClassicMultiplier;

		/// <summary>
		/// FHT multiplier instance.
		/// </summary>
		static readonly public IMultiplier AutoFhtMultiplier;

		#endregion Fields

		#region Constructors

		// .cctor
		static MultiplyManager()
		{
			// Create new classic multiplier instance
			IMultiplier classicMultiplier = new ClassicMultiplier();

			// Fill publicity visible multiplier fields
			ClassicMultiplier = classicMultiplier;
			AutoFhtMultiplier = new AutoFhtMultiplier(classicMultiplier);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Returns multiplier instance for given multiply mode.
		/// </summary>
		/// <param name="mode">Multiply mode.</param>
		/// <returns>Multiplier instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="mode" /> is out of range.</exception>
		static public IMultiplier GetMultiplier(MultiplyMode mode)
		{
			switch (mode)
			{
				case MultiplyMode.AutoFht:
					return AutoFhtMultiplier;
				case MultiplyMode.Classic:
					return ClassicMultiplier;
				default:
					throw new ArgumentOutOfRangeException("mode");
			}
		}

		/// <summary>
		/// Returns current multiplier instance.
		/// </summary>
		/// <returns>Current multiplier instance.</returns>
		static public IMultiplier GetCurrentMultiplier()
		{
			return GetMultiplier(BigInteger.GlobalSettings.MultiplyMode);
		}

		#endregion Methods
	}
}
