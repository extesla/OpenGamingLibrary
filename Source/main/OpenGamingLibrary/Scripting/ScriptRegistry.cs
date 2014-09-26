// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;
using System.Collections.Generic;

namespace OpenGamingLibrary.Scripting
{
	public class ScriptRegistry : IDisposable
	{

		#region Fields
		private readonly Dictionary<Type, IScript> values = new Dictionary<Type, IScript>();
		#endregion

		#region Properties
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

