// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Security
{
	public enum AuthenticationState
	{
		/// <summary>
		/// The security context is not authenticated.
		/// </summary>
		NotAuthenticated,

		/// <summary>
		/// The security context is pending authentication.
		/// </summary>
		Pending,

		/// <summary>
		/// The security context is in the progress of authenticating a user.
		/// </summary>
		InProgress,

		/// <summary>
		/// The security context is fully authenticated.
		/// </summary>
		Authenticated
	}
}

