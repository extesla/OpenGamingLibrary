// Copyright (C) 2012 Open Gaming Foundation
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
//
using System;
using System.Collections.Generic;

namespace OpenGamingLibrary.Collections
{

	/// <summary>
	/// An unordered set.
	/// </summary>
	/// <remarks>>
	/// Provides an implementation of an unordered set collection, utilizing
	/// a dictionary as its backend.
	/// </remarks>
	/// <exception cref='NotImplementedException'>
	/// Is thrown when a requested operation is not implemented for a given type.
	/// </exception>
	/// <author>Sean Quinn</author>
	public class Set<TValue> : ISet<TValue>
	{
		private Dictionary<TValue, object> values = new Dictionary<TValue, object>();

		public int Count {
			get { return values.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public Set ()
		{
			// Empty
		}

		public void Add (TValue item)
		{
			values.Add(item, null);
		}

		public void Clear ()
		{
			values.Clear();
		}

		public bool Contains (TValue item)
		{
			return values.ContainsKey(item);
		}

		public void CopyTo (TValue[] array, int arrayIndex)
		{
			values.Keys.CopyTo(array, arrayIndex);
		}

		public bool Remove (TValue item)
		{
			return values.Remove(item);
		}

		public override string ToString ()
		{
			return string.Format("[Set]");
		}

		public override bool Equals (object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}

		public IEnumerator<TValue> GetEnumerator ()
		{
			return values.Keys.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException();
		}
	}
}

