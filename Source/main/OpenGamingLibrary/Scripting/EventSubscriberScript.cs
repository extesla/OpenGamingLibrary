// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Scripting
{
	public abstract class EventSubscriberScript : IScript
	{

		/// <summary>
		/// Subscribe this instance.
		/// </summary>
		protected abstract void Subscribe ();

		/// <summary>
		/// Unsubscribe this instance.
		/// </summary>
		protected abstract void Unsubscribe ();

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="OpenGamingLibrary.Scripting.EventSubscriberScript"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="OpenGamingLibrary.Scripting.EventSubscriberScript"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="OpenGamingLibrary.Scripting.EventSubscriberScript"/> so the garbage collector can reclaim the memory
		/// that the <see cref="OpenGamingLibrary.Scripting.EventSubscriberScript"/> was occupying.</remarks>
		public virtual void Dispose()
		{
			Unsubscribe();
		}
	}
}

