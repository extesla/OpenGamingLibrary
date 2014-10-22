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

namespace OpenGamingLibrary.Ioc
{

	/// <summary>
	/// The registered service container for the application.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When an application registers objects to a service container they need
	/// to be stored some place. The purpose of a service container, as it is
	/// constructed by the composition root, is to present a collection of
	/// services that can be made available to the whole of the application
	/// from a central source.
	/// </para>
	/// <para>
	/// The <c>RegisteredServiceContainer</c> is one option for that source.
	/// </para>
	/// <para>
	/// The <c>RegisteredServiceContainer</c> is a singleton instance that
	/// reflects a service container that has been registered with it. The
	/// underlying service container is effectively immutable (it is not exposed
	/// for modification). You can only retrieve elements from the registered
	/// container.
	/// </para>
	/// </remarks>
	/// <example>
	/// Assigning a service container to the RegisteredServiceContainer from
	/// a composition root.
	/// <code>
	/// public class CompositionRoot : ICompositionRoot
	/// {
	///   public CompositionRoot()
	///   {
	///     RegisteredServiceContainer.Compose(() => Compose(new SimpleInjectorAdapter()));
	///   }
	/// 
	///   public void Compose(IServiceContainer container)
	///   {
	///     ...
	///   }
	/// }
	/// </code>
	/// </example>
	public sealed class RegisteredServiceContainer
	{
		#region Static Fields
		/// <summary>
		/// The instance of the <see cref="RegisteredServiceContainer"/>
		/// </summary>
		private static readonly RegisteredServiceContainer instance = new RegisteredServiceContainer();
		#endregion

		#region Static Properties
		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static RegisteredServiceContainer Instance
		{
			get { return instance; }
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the service container.
		/// </summary>
		/// <value>The service container.</value>
		private IServiceContainer ServiceContainer { get; set; }
		#endregion

		/// <summary>
		/// Static constructor for the <see cref="OpenGamingLibrary.Ioc.RegisteredServiceContainer"/>.
		/// </summary>
		/// <remarks>
		/// The pressences of the static constructor for the <c>RegisteredServiceContainer</c>
		/// class enforces laziness in the initialization of the object.
		/// </remarks>
		static RegisteredServiceContainer ()
		{
			// Empty.
		}

		private RegisteredServiceContainer ()
		{
			//
		}

		public static void Assign(IServiceContainer container)
		{
			Instance.ServiceContainer = container;
		}

		/// <summary>
		/// Gets the instance of a registered service by the passed
		/// <paramref name="serviceType"/>
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="serviceType">Service type.</param>
		public static object GetInstance (Type serviceType)
		{
			if (Instance.ServiceContainer == null)
			{
				throw new NullReferenceException("No service container has been registered.");
			}
			return Instance.ServiceContainer.GetInstance(serviceType);
		}

		/// <summary>
		/// Gets the instance of the registered service by the passed
		/// <typeparamref name="TService" />.
		/// </summary>
		/// <remarks>
		/// </remarks>
		public static TService GetInstance<TService> () where TService : class
		{
			if (Instance.ServiceContainer == null)
			{
				throw new NullReferenceException("No service container has been registered.");
			}
			return Instance.ServiceContainer.GetInstance<TService>();
		}

		public static void Reset()
		{
			if (Instance.ServiceContainer != null)
			{
				Instance.ServiceContainer.Dispose();
				Instance.ServiceContainer = null;
			}
		}
	}
}

