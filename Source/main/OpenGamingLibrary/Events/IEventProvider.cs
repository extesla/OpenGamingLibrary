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

namespace OpenGamingLibrary.Events {

	/// <summary>
	/// An event provider is any object that encompasses one or more events that
	/// can be raised. Event providers are registered with an
	/// <see cref="IEventRegistry" /> and fired from the same event dispatcher.
	/// </summary>
	/// <author>Sean Quinn</author>
	public interface IEventProvider
	{

		/// <summary>
		/// Gets the type of the event.
		/// </summary>
		/// <value>The type of the event.</value>
		Type EventType { get; }

		/// <summary>
		/// Adds an event handler to the event provider.
		/// </summary>
		/// <param name='obj'>
		/// The event handler.
		/// </param>
		void Add(object obj);

		/// <summary>
		/// Returns the underlying event handler.
		/// </summary>
		/// <returns>
		/// The underlying event handler.
		/// </returns>
		object GetEventHandler();

		/// <summary>
		/// Remove an event handler from the event provider.
		/// </summary>
		/// <param name='obj'>
		/// The event handler.
		/// </param>
		void Remove(object obj);
	}
}

