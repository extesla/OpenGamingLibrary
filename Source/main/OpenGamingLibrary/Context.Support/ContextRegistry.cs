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
//using OpenGamingLibrary.Logging;

namespace OpenGamingLibrary.Context.Support
{

	/// <summary>
	/// Provides access to a central registry of
	/// <see cref="OpenGamingLibrary.IContext"/>s.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>ContextRegistry</c> is implemented as a singleton to provide
	/// access to one or more contexts. Contexts are cached.
	/// </para>
	/// </remarks>
	public sealed class ContextRegistry
	{

		/// <summary>
		/// The context registry holder; a lazy implementation of the singleton
		/// design pattern.
		/// </summary>
		private sealed class ContextRegistryHolder
		{
			static ContextRegistryHolder()
			{
			}

			internal static readonly ContextRegistry instance = new ContextRegistry();
		}

		#region Static Fields
		/// <summary>
		/// The name of the root context.
		/// </summary>
		private static string rootContextName = null;

		/// <summary>
		/// An object upon which thread-safe operations can lock and use for
		/// synchronization.
		/// </summary>
		private static readonly object sync = new object();
		#endregion

		#region Static Properties
		/// <summary>
		/// Gets the singleton instance of the context registry.
		/// </summary>
		/// <value>The singleton instance of the context registry.</value>
		public static ContextRegistry Instance
		{
			get { return ContextRegistryHolder.instance; }
		}
		#endregion

		#region Fields
		/// <summary>
		/// The cached collection of contexts, by name.
		/// </summary>
		private IDictionary<string, IContext> contextMap = new Dictionary<string, IContext>();
		#endregion

		static ContextRegistry()
		{
		}

		/// <summary>
		/// Private constructor, initializes a new instance of the
		/// <see cref="OpenGamingLibrary.Context.Support.ContextRegistry"/> class.
		/// </summary>
		private ContextRegistry ()
		{
		}

		/// <summary>
		/// This event is fired, if ContextRegistry.Clear() is called. Clients
		/// may register to get informed
		/// </summary>
		/// <remarks>
		/// <para>>
		/// This event is fired while still holding a lock on the Registry.
		/// </para>
		/// <para>
		/// The <c>sender</c> parameter of the event handler is sent as
		/// <c>typeof(ContextRegistry)</c>, EventArgs are not used.
		/// </para>
		/// </remarks>
		public static event EventHandler Cleared;

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public static void Clear()
		{
			lock (sync)
			{
				ICollection<IContext> contexts = new List<IContext>(Instance.contextMap.Values);
				foreach (IContext ctx in contexts)
				{
					ctx.Dispose();
				}

				/*
				if (log.IsWarnEnabled)
				{
					if (instance.contextMap.Count > 0)
					{
						string msg = string.Format("Not all contexts were removed from the registry during cleanup. Verify that base.Dispose() was called when overriding the disposal method on contexts.");
						log.Warn(msg);
					}
				}
				 */

				Instance.contextMap.Clear();
				rootContextName = null;
				// mark section dirty - force re-read from disk next time
				//ConfigurationUtils.RefreshSection(AbstractApplicationContext.ContextSectionName);
				//DynamicCodeManager.Clear();
				if (Cleared != null)
				{
					Cleared(typeof(ContextRegistry), EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <returns>The context.</returns>
		public static IContext GetContext()
		{
			lock (sync)
			{
				if (rootContextName == null)
				{
					throw new ContextException("No context registered. Use the 'RegisterContext' method.");
				}
				return GetContext(rootContextName);
			}
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <returns>The context.</returns>
		/// <param name="name">Name.</param>
		public static IContext GetContext(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("The context name passed to GetContext method cannot be null or empty.");
			}

			lock (sync)
			{
				IContext ctx;
				if (!Instance.contextMap.TryGetValue(name, out ctx))
				{
					string msg = string.Format("No context registered under name '{0}'. Use the 'RegisterContext' method.", name);
					throw new ContextException(msg);
				}

				/*
				if (log.IsDebugEnabled)
				{
					log.Debug(string.Format("Returning context: '{0}' registered under name: '{1}'", ctx, name));
				}
				*/
				return ctx;
			}
		}


		/// <summary>
		/// Determines if a context is registered to the specified name.
		/// </summary>
		/// <param name="name">The context name.</param>
		/// <returns><c>true</c> if a context is registered to the specified
		/// name; otherwise, <c>false</c>.</returns>
		public static bool IsContextRegistered(string name)
		{
			lock (sync)
			{
				IContext temp;
				Instance.contextMap.TryGetValue(name, out temp);
				return temp != null;
			}
		}

		/// <summary> 
		/// Registers an instance of an
		/// <see cref="OpenGamingLibrary.IContext"/>. 
		/// </summary> 
		/// <remarks>
		/// <p>
		/// This is usually called via a
		/// <see cref="OpenGamingLibrary.Context.Support.ContextHandler"/> inside a .NET
		/// application configuration file.
		/// </p>
		/// </remarks>
		/// <param name="context">The application context to be registered.</param>
		/// <exception cref="OpenGamingLibrary.ContextException">
		/// If a context has previously been registered using the same name
		/// </exception>
		public static void RegisterContext(IContext context)
		{
			lock (sync)
			{
				IContext ctx;
				if (Instance.contextMap.TryGetValue(context.Name, out ctx))
				{
					throw new ContextException(string.Format("Existing context '{0}' already registered under name '{1}'.", ctx, context.Name));
				}

				Instance.contextMap[context.Name] = context;

				/*
				if (log.IsDebugEnabled)
				{
					log.Debug(String.Format("Registering context '{0}' under name '{1}'.", context, context.Name));
				}
				*/

				if (rootContextName == null)
				{
					rootContextName = context.Name;
				}
			}
		}

		/// <summary>
		/// Removes the context from the registry 
		/// </summary>
		/// <remarks>
		/// Has no effect if the context wasn't registered
		/// </remarks>
		/// <param name="context">´the context to remove from the registry</param>
		private static void UnregisterContext(IContext context)
		{
			if (context == null)
			{
				throw new ArgumentException("Unable to unregister a null context.");
			}

			lock (sync)
			{
				if (IsContextRegistered(context.Name))
				{
					Instance.contextMap.Remove(context.Name);
					if (rootContextName == context.Name)
					{
						rootContextName = null;
					}
				}
			}
		}
	}
}

