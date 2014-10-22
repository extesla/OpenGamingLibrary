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
	/// The <c>EventProvider</c> class is a concrete implementation of the
	/// <see cref="IEventProvider" /> interface. Event providers use a provider
	/// design pattern to encapsulate .NET framework events and the event
	/// handlers for those events.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <example>
	/// <code>
	/// EventProvider<EventArgs> myEventProvider = new EventProvider<EventArgs>();
	/// eventDispatcher.Subscribe<EventArgs>(myEventProvider, this.OnEvent);
	/// </code>
	/// </example>
	/// <author>Sean Quinn</author>
	public class EventProvider<TEventArgs> : IEventProvider
		where TEventArgs : EventArgs
	{

		#region Fields
		/// <summary>
		/// The event handler.
		/// </summary>
		EventHandler<TEventArgs> eh;
		#endregion

		#region Properties
		/// <summary>
		/// The event that will be triggered as part of the execution of this
		/// event provider.
		/// </summary>
		/// <value>The aggregate, multicast, event handlers.</value>
		protected event EventHandler<TEventArgs> Event
		{
			add { eh += value; }
			remove { eh -= value; }
		}

		/// <summary>
		/// Gets the type of the event.
		/// </summary>
		/// <value>The type of the event.</value>
		public Type EventType { get; private set; }
		#endregion

		/// <summary>
		/// Initializes the current instance of the event provider.
		/// </summary>
		/// <remarks>
		/// The no-argument constructor is necessary for all event providers, as
		/// the <see cref="OpenGamingLibrary.Events.EventRegistry" /> will use
		/// this constructor to instantiate a new provider if only a reference
		/// to the type is passed, not an concrete method to construct a new <c>EventProvider</c> whenever an
		/// event provider is s
		/// </remarks>
		public EventProvider()
		{
			EventType = typeof(TEventArgs);
		}

		public object GetEventHandler()
		{
			return eh;
		}

		/// <inheritdoc />
		public void Add(object obj)
		{
			if (!(obj is EventHandler<TEventArgs>))
			{
				throw new ArgumentException(string.Format("Unable to add object to event. "
						+ "Object must be of the type: {0}.", typeof(EventHandler<TEventArgs>)));
			}
			Add((EventHandler<TEventArgs>) obj);
		}

		/// <summary>
		/// Add the specified callback.
		/// </summary>
		/// <param name='eventHandler'>
		/// Callback.
		/// </param>
		public virtual void Add(EventHandler<TEventArgs> eventHandler)
		{
			Event += eventHandler;
		}

		/// <inheritdoc />
		public void Remove(object obj)
		{
			if (!(obj is EventHandler))
			{
				throw new ArgumentException("Unable to remove object to event. Object must be an EventHandler.");
			}
			Remove((EventHandler<TEventArgs>) obj);
		}

		/// <summary>
		/// Remove the specified callback.
		/// </summary>
		/// <param name='eventHandler'>
		/// Callback.
		/// </param>
		public virtual void Remove(EventHandler<TEventArgs> eventHandler)
		{
			Event -= eventHandler;
		}
	}
}

