// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Ioc
{

	/// <summary>
	/// A contract for using the composition root pattern in an application. The
	/// composition root is the (preferably) unique location where modules are
	/// composed together. The composition root is a component of an
	/// application's infrastructure.
	/// </summary>
	/// <remarks>
	/// <para>
	/// While the composition root is the location wherein the services and
	/// dependencies are composed, it does not necessarily indicate the location
	/// in which they are stored. In many cases the root
	/// <see cref="OpenGamingLibrary.IContext"/> will also be the composition
	/// root, however retrieving services via the context is cumbersome and
	/// not always practical.
	/// </para>
	/// </remarks>
	/// <author>Sean Quinn</author>
	public interface ICompositionRoot
	{

		/// <summary>
		/// Compose services in the application's object graph by adding those
		/// services to the specified <paramref name="container" />.
		/// </summary>
		/// <param name="container">The registry.</param>
		void Compose(IServiceContainer container);
	}
}

