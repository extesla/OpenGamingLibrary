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

namespace OpenGamingLibrary.Numerics.Settings
{
	/// <summary>
	/// <see cref="BigInteger" /> global settings.
	/// </summary>
	sealed public class BigIntegerGlobalSettings
	{
		#region Private fields

		volatile MultiplyMode _multiplyMode = MultiplyMode.AutoFht;
		volatile DivideMode _divideMode = DivideMode.AutoNewton;
		volatile ParseMode _parseMode = ParseMode.Fast;
		volatile ToStringMode _toStringMode = ToStringMode.Fast;
		volatile bool _autoNormalize = false;
		volatile bool _applyFhtValidityCheck = true;

		#endregion Private fields

		// Class can be created only inside this assembly
		internal BigIntegerGlobalSettings() {}

		#region Public properties

		/// <summary>
		/// Multiply operation mode used in all <see cref="BigInteger" /> instances.
		/// Set to auto-FHT by default.
		/// </summary>
		public MultiplyMode MultiplyMode
		{
			get { return _multiplyMode; }
			set { _multiplyMode = value; }
		}

		/// <summary>
		/// Divide operation mode used in all <see cref="BigInteger" /> instances.
		/// Set to auto-Newton by default.
		/// </summary>
		public DivideMode DivideMode
		{
			get { return _divideMode; }
			set { _divideMode = value; }
		}

		/// <summary>
		/// Parse mode used in all <see cref="BigInteger" /> instances.
		/// Set to Fast by default.
		/// </summary>
		public ParseMode ParseMode
		{
			get { return _parseMode; }
			set { _parseMode = value; }
		}

		/// <summary>
		/// To string conversion mode used in all <see cref="BigInteger" /> instances.
		/// Set to Fast by default.
		/// </summary>
		public ToStringMode ToStringMode
		{
			get { return _toStringMode; }
			set { _toStringMode = value; }
		}

		/// <summary>
		/// If true then each operation is ended with big integer normalization.
		/// Set to false by default.
		/// </summary>
		public bool AutoNormalize
		{
			get { return _autoNormalize; }
			set { _autoNormalize = value; }
		}

		/// <summary>
		/// If true then FHT multiplication result is always checked for validity
		/// by multiplying integers lower digits using classic algorithm and comparing with FHT result.
		/// Set to true by default.
		/// </summary>
		public bool ApplyFhtValidityCheck
		{
			get { return _applyFhtValidityCheck; }
			set { _applyFhtValidityCheck = value; }
		}

		#endregion Public properties
	}
}
