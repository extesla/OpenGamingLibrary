// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Ioc
{

	/// <summary>
	/// The service resolver is a contract which assists in the resolution of
	/// registered services.
	/// </summary>
	/// <author>Sean Quinn</author>
	public interface IServiceResolver
	{

		/// <summary>
		/// Gets an instance of a given <paramref name="serviceType"/>.
		/// </summary>
		/// <param name="serviceType">The service type.</param>
		/// <returns>The instance.</returns>
		object GetInstance (Type serviceType);

		/// <summary>
		/// Gets an instance of a given <typeparamref name="TService" />.
		/// </summary>
		/// <returns>The instance.</returns>
		/// <typeparam name="TService">The service type.</typeparam>
		TService GetInstance<TService> () where TService : class;
	}
}

