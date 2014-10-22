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
using System.Reflection;
using OpenGamingLibrary.Ioc;

namespace OpenGamingLibrary.Ioc.SimpleInjector
{

	/// <summary>
	/// Simple injector container adapter.
	/// </summary>
	public class SimpleInjectorAdapter : IServiceContainerAdapter<global::SimpleInjector.Container>
	{

		#region Fields

		private readonly global::SimpleInjector.Container container;

		#endregion

		#region Properties

		/// <inheritdoc/>
		/// <remarks>
		/// The container returned is the SimpleInjector container.
		/// </remarks>
		public global::SimpleInjector.Container Container
		{
			get { return container; }
		}

		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="global::SimpleInjector.Container"/>
		/// class using a default instance of the <see cref="global::SimpleInjector.Container"/>
		/// class.
		/// </summary>
		public SimpleInjectorAdapter () : this(new global::SimpleInjector.Container())
		{
			// Empty.
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="global::SimpleInjector.Container"/>
		/// class using the specified instance of the <see cref="global::SimpleInjector.Container"/> 
		/// </summary>
		/// <param name="container">Container.</param>
		public SimpleInjectorAdapter (global::SimpleInjector.Container container)
		{
			this.container = container;
		}

		public void Dispose ()
		{
			throw new NotImplementedException();
		}

		#region IServiceFactory implementation

		/// <inheritdoc/>
		/// <remarks>
		/// Returns the instance of the <paramref name="serviceType"/> from the
		/// SimpleInjector container.
		/// </remarks>
		public object GetInstance (Type serviceType)
		{
			return container.GetInstance(serviceType);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Returns the instance of the <typeref name="serviceType"/> from the
		/// SimpleInjector container.
		/// </remarks>
		public TService GetInstance<TService> () where TService : class
		{
			return container.GetInstance<TService>();
		}

		#endregion

		#region IServiceRegistry implementation

		/// <inheritdoc/>
		/// <remarks>
		/// </remarks>
		public void Register<TService, TImplementation> ()
			where TService : class
			where TImplementation : class, TService
		{
			container.RegisterSingle<TService, TImplementation>();
		}

		/// <inheritdoc/>
		/// <remarks>
		/// SimpleInjector ignores the supplied <paramref name="serviceName"/>.
		/// </remarks>
		public void Register<TService, TImplementation> (string serviceName)
			where TService : class
			where TImplementation : class, TService
		{
			container.RegisterSingle<TService, TImplementation>();
		}

		/// <inheritdoc />
		/// <remarks>
		/// </remarks>
		public void Register<TService, TImplementation> (TImplementation instance)
			where TService : class
			where TImplementation : class, TService
		{
			container.RegisterSingle<TImplementation>(instance);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Registers all of the types in an assembly by way of the composition
		/// root. SimpleInjector does not have this feature by default, so this
		/// instead calls an extension method added to SimpleInjector by this
		/// library.
		/// </remarks>
		public void RegisterAssembly (Assembly assembly)
		{
			throw new NotImplementedException("SimpleInjectorAdapter does not support registering types by assembly, yet.");
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Pass-thru to SimpleInjector's own container verification method.
		/// </remarks>
		public void Verify ()
		{
			container.Verify();
		}

		#endregion
	}
}

