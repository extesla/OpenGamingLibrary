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
using System.Collections.Generic;
using Common.Logging;

namespace OpenGamingLibrary.Scripting
{

	/// <summary>
	/// Script registry.
	/// </summary>
	public class ScriptRegistry : IDisposable
	{

		#region Fields
		/// <summary>
		/// Dictionary of scripts by type.
		/// </summary>
		private readonly Dictionary<Type, IScript> values = new Dictionary<Type, IScript>();

		/// <summary>
		/// The log.
		/// </summary>
		private readonly ILog log = LogManager.GetCurrentClassLogger();
		#endregion

		#region Properties
		/// <inheritdoc />
		/// <remarks>
		/// </remarks>
		public ILog Log
		{
			get { return log; }
		}

		/// <summary>
		/// Gets or sets the <see cref="OpenGamingLibrary.Scripting.ScriptRegistry"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public IScript this [Type index]
		{
			get { return values[index]; }
			set { values[index] = value; }
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get { return values.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the keys.
		/// </summary>
		/// <value>The keys.</value>
		public ICollection<Type> Types
		{
			get { return values.Keys; }
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The values.</value>
		public ICollection<IScript> Scripts
		{
			get { return values.Values; }
		}
		#endregion

		public ScriptRegistry ()
		{
		}

		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public void Add (IScript item)
		{
			values.Add(item.GetType(), item);
		}

		public void Clear ()
		{
			foreach (IScript value in values.Values) {
				value.Dispose();
			}
			values.Clear();
		}

		public bool Contains (Type type)
		{
			return values.ContainsKey(type);
		}

		public void Dispose()
		{
			Clear();
		}

		public bool Remove (Type key)
		{
			if (values.ContainsKey(key)) {
				IScript script = values[key];
				script.Dispose();
			}
			return values.Remove(key);
		}

		public bool TryGetValue (Type key, out IScript value)
		{
			return values.TryGetValue(key, out value);
		}

		public IEnumerator<Type> GetEnumerator ()
		{
			return values.Keys.GetEnumerator();
		}
	}
}

