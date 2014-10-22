// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System.Reflection;

namespace OpenGamingLibrary.Ioc
{

	/// <summary>
	/// Interface that defines the contract by which classes implementing it
	/// must adhere to when registering new services with a container.
	/// </summary>
	/// <author>Sean Quinn</author>
	public interface IServiceRegistry
	{

		/// <summary>
		/// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
		/// </summary>
		/// <param name="serviceType">The service type to register.</param>
		/// <param name="implementingType">The implementing type.</param>
		//void Register(Type serviceType, Type implementingType);

		/// <summary>
		/// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
		/// </summary>
		/// <param name="serviceType">The service type to register.</param>
		/// <param name="implementingType">The implementing type.</param>
		/// <param name="serviceName">The name of the service.</param>
		//void Register(Type serviceType, Type implementingType, string serviceName);

		/// <summary>
		/// Register the <typeparamref name="TService"/> with the <typeparamref name="TImplementation">.
		/// </summary>
		/// <example>
		/// container.Register<IFoo, Foo>();
		/// </example>
		/// <typeparam name="TService">The service type to register.</typeparam>
		/// <typeparam name="TImplementation">The implementing type.</typeparam>
		void Register<TService, TImplementation> ()
			where TService : class
			where TImplementation : class, TService;

		/// <summary>
		/// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
		/// </summary>
		/// <typeparam name="TService">The service type to register.</typeparam>
		/// <typeparam name="TImplementation">The implementing type.</typeparam>
		/// <param name="serviceName">The name of the service.</param>
		void Register<TService, TImplementation>(string serviceName)
			where TService : class
			where TImplementation : class, TService;

		/// <summary>
		/// Register the <typeparamref name="TService"/> with the <typeparamref name="TImplementation">.
		/// </summary>
		/// <example>
		/// container.Register<IFoo, Foo>();
		/// </example>
		/// <param name="instance">The instance of the implementation.</typeparam>
		/// <typeparam name="TService">The service type to register.</typeparam>
		/// <typeparam name="TImplementation">The implementing type.</typeparam>
		void Register<TService, TImplementation> (TImplementation instance)
			where TService : class
			where TImplementation : class, TService;

		/// <summary>
		/// Registers services from the given <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly">The assembly to be scanned for services.</param>        
		/// <remarks>
		/// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
		/// will be used to configure the container.
		/// </remarks>     
		void RegisterAssembly(Assembly assembly);

		/// <summary>
		/// Verifies the state of the container.
		/// </summary>
		void Verify();
	}
}

