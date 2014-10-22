// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Ioc
{

	/// <summary>
	/// The contract for all adapted inversion of control containers.
	/// </summary>
	/// <author>Sean Quinn</author>
	public interface IServiceContainerAdapter<TContainer> : IServiceContainer
	{

		/// <summary>
		/// The underlying container, exposed to permit more advanced usage
		/// beyond what is provided for by the service container adapter.
		/// </summary>
		/// <value>The container.</value>
		TContainer Container { get; }
	}
}

