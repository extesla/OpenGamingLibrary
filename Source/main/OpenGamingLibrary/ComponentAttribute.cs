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

namespace OpenGamingLibrary
{

	/// <summary>
	/// Indicates that an annotated class is a "component".
	/// </summary>
	/// <remarks>
	/// <para>
	/// Classes marked as components are considered as candidates for future
	/// features such as auto-detection when using attribute-based configuration
	/// and assembly scanning.
	/// </para>
	/// <para>
	/// Other class-level annotations may be considered as identifying a
	/// component as well, typically a special kind of component: e.g. a 
	/// Repository attribute.
	/// </para>
	/// <para>
	/// Inspired by the Spring.NET <c>ComponentAttribute</c>
	/// </para>
	/// </remarks>
	/// <author>Sean Quinn</author>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
	[Serializable]
	public class ComponentAttribute : Attribute
	{

		#region Fields
		/// <summary>
		/// The name of the component.
		/// </summary>
		private string name = "";
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the name of the component
		/// </summary>
		/// <value>The name of the component.</value>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="ComponentAttribute"/> class.
		/// </summary>
		public ComponentAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ComponentAttribute"/> class.
		/// </summary>
		/// <param name="name">The name of the component.</param>
		public ComponentAttribute(string name)
		{
			this.name = name;
		}
	}
}

