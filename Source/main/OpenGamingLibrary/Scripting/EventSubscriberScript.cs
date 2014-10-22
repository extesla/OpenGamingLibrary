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

namespace OpenGamingLibrary.Scripting
{

	/// <summary>
	/// Event subscriber script.
	/// </summary>
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

