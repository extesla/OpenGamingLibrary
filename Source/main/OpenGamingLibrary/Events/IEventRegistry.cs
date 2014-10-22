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

namespace OpenGamingLibrary.Events
{
	/// <summary>
	/// A registry that manages subscriptions to and the publishing of events.
	/// </summary>
	/// <author>Sean Quinn</author>
	public interface IEventRegistry
	{

		/// <summary>
		/// Fires the event.
		/// </summary>
		/// <param name='eventArgs'>
		/// Event arguments.
		/// </param>
		/// <typeparam name='TEventArgs'>
		/// The 1st type parameter.
		/// </typeparam>
		void Publish<TEventArgs>(TEventArgs eventArgs)
			where TEventArgs : EventArgs;

		/// <summary>
		/// Register the specified eventProvider.
		/// </summary>
		/// <param name='eventProvider'>
		/// Event provider.
		/// </param>
		/// <typeparam name='TEventProvider'>
		/// The 1st type parameter.
		/// </typeparam>
		void Register<TEventProvider>(TEventProvider eventProvider)
			where TEventProvider : IEventProvider;
		
		/// <summary>
		/// Subscribe the specified eventHandler.
		/// </summary>
		/// <param name='eventHandler'>
		/// Event handler.
		/// </param>
		/// <typeparam name='TEventArgs'>
		/// The 1st type parameter.
		/// </typeparam>
		void Subscribe<TEventArgs>(EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs;
		
		/// <summary>
		/// Unsubscribe the specified eventHandler.
		/// </summary>
		/// <param name='eventHandler'>
		/// Event handler.
		/// </param>
		/// <typeparam name='TEventArgs'>
		/// The 1st type parameter.
		/// </typeparam>
		void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs;
	}
}

