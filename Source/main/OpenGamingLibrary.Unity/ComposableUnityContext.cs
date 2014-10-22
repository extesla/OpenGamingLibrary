// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using UnityEngine;
using OpenGamingLibrary.Ioc;

namespace OpenGamingLibrary.Unity
{
	/// <summary>
	/// </summary>
	/// <author name="Sean.Quinn" />
	public class ComposableUnityContext<TServiceContainer> : UnityContext, ICompositionRoot
		where TServiceContainer : IServiceContainer, new()
	{
	
		/// <inheritdoc/>
		/// <remarks>
		/// </remarks>
		public override void Refresh ()
		{
			Log.Trace("START: Refresh.");
			var container = new TServiceContainer();
			Compose(container);

			RegisteredServiceContainer.Assign(container);
			Log.Trace("END: Refresh.");
		}

		/// <inheritdoc />
		/// <remarks>
		/// Compose is stubbed out here, if you creating a context for your game by
		/// extending the UnityContext, you must implement the composition method,
		/// otherwise you will get a very angry error. See below.
		/// </remarks>
		public virtual void Compose(IServiceContainer container)
		{
			throw new System.NotImplementedException("The Compose method must be implemented for all composition roots.");
		}
	}
}