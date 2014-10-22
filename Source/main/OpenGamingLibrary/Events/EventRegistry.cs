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
using System.Linq;
using Common.Logging;

namespace OpenGamingLibrary.Events
{

	/// <summary>
	/// The event dispatcher is responsible for managing event providers and for
	/// raising the events on them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The event dispatcher contains a collection of key-value pairs that allow
	/// a developer to register event providers by their <see cref="System.Type" />.
	/// Developers can subscribe delegates and event handlers to the events on
	/// the event provider.
	/// </para>
	/// <para>
	/// An event provider may be registered at the time of first event
	/// subscription, either by passing in a reference to the event provider or
	/// by passing an already instantiated instance of the provider into the
	/// subscription method.
	/// </para>
	/// </remarks>
	/// <author>Sean Quinn</author>
	public class EventRegistry : IEventRegistry, ILoggable<ILog>
	{

		#region Fields
		/// <summary>
		/// The dictionary of event providers.
		/// </summary>
		private readonly Dictionary<Type, IEventProvider> eventProviders = new Dictionary<Type, IEventProvider>();

		/// <summary>
		/// The log.
		/// </summary>
		private readonly ILog log = LogManager.GetCurrentClassLogger();
		#endregion

		#region Properties
		private Dictionary<Type, IEventProvider> EventProviders
		{
			get { return eventProviders; }
		}

		/// <inheritdoc />
		/// <remarks>
		/// </remarks>
		public ILog Log
		{
			get { return log; }
		}
		#endregion

		/// <summary>
		/// Contains this instance.
		/// </summary>
		/// <typeparam name="TEventArgs">The 1st type parameter.</typeparam>
		public virtual bool Contains(Type type)
		{
			var results = EventProviders.Where(x => type.Equals(x.Value.EventType));
			return results != null && results.Any();
		}

		/// <summary>
		/// Fires an event, if and only if that is contained in the internal map
		/// of registered event providers.
		/// </summary>
		/// <param name="eventArgs">
		/// The event handler.
		/// </param>
		/// <param name="eventArgs">
		/// The event arguments.
		/// </param>
		public virtual void Publish<TEventArgs>(TEventArgs eventArgs)
			where TEventArgs : EventArgs
		{
			var type = typeof(EventProvider<TEventArgs>);
			if (!EventProviders.ContainsKey(type))
			{
				Log.Trace(string.Format("No event of type: {0} registered with {1}.", type, this));
				return;
			}

			var eventProvider = (EventProvider<TEventArgs>) EventProviders[type];
			var eventCopy = (EventHandler<TEventArgs>) eventProvider.GetEventHandler();
			if (eventCopy != null)
			{
				lock (eventCopy)
				{
					Log.Trace(string.Format("Publishing event: {0} with arguments: {1}", type, eventArgs));
					eventCopy(this, eventArgs);
				}
			}
		}

		/// <summary>
		/// Register the specified event provider.
		/// </summary>
		/// <param name='eventProvider'>
		/// Event provider.
		/// </param>
		/// <typeparam name='TEventProvider'>
		/// The type parameter, identifying the type of the event provider.
		/// </typeparam>
		public virtual void Register<TEventProvider>(TEventProvider eventProvider)
			where TEventProvider : IEventProvider
		{
			EventProviders.Add(typeof(TEventProvider), eventProvider);
		}

		/// <summary>
		/// Subscribe the specified event handler with the identified event
		/// provider. If the event provider is not already registered with the
		/// event dispatcher a new provider will be instantiated and registered.
		/// </summary>
		/// <param name='eventHandler'>
		/// Event handler.
		/// </param>
		/// <typeparam name='TEventProvider'>
		/// The type of the event provider.
		/// </typeparam>
		/// <typeparam name='TEventArgs'>
		/// The type of the event arguments that will be used in the handler.
		/// </typeparam>
		public virtual void Subscribe<TEventArgs>(EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs
		{
			var type = typeof(EventProvider<TEventArgs>);
			if (!EventProviders.ContainsKey(type))
			{
				Log.Trace(string.Format("Registering new event provider for event type: {0}.", type));
				Register (new EventProvider<TEventArgs>());
			}

			var eventProvider = (EventProvider<TEventArgs>) EventProviders[type];

			Log.Trace(string.Format("Subscribing event handler: {0} or event type: {1}.", eventHandler, type));
			eventProvider.Add(eventHandler);
		}

		/// <summary>
		/// Unsubscribe the specified eventProvider and eventHandler.
		/// </summary>
		/// <param name='eventHandler'>
		/// Event provider.
		/// </param>
		/// <param name='eventHandler'>
		/// Event handler.
		/// </param>
		/// <typeparam name='TEventArgs'>
		/// The 1st type parameter.
		/// </typeparam>
		public virtual void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs
		{
			var type = typeof(EventProvider<TEventArgs>);
			if (!EventProviders.ContainsKey(type))
			{
				Log.Trace(string.Format("No event of type: {0} to unsubscribe.", type));
				return;
			}

			var eventProvider = (EventProvider<TEventArgs>) EventProviders[type];

			Log.Trace(string.Format("Unsubscribing event handler: {0} or event type: {1}.", eventHandler, type));
			eventProvider.Remove(eventHandler);
		}
	}
}
