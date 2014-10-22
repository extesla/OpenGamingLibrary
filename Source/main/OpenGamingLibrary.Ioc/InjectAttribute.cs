// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;

namespace OpenGamingLibrary.Ioc
{
	/// <summary>
	/// Explicit container injection attribute.
	/// </summary>
	/// <remarks>
	/// The <c>InjectAttribute</c> may be used to explicitly inject a service
	/// into a property or parameter outside of the context of a constructor
	/// (the preferred methodology of dependency injection).
	/// </remarks>
	[AttributeUsage(AttributeTargets.Parameter|AttributeTargets.Property)]
	public class InjectAttribute : Attribute
	{
		#region Properties
		/// <summary>
		/// Gets or sets the name of the service to be injected.
		/// </summary>
		/// <value>The name of the service.</value>
		public string ServiceName { get; set; }
		#endregion

		public InjectAttribute () : this(string.Empty)
		{
			// Empty.
		}

		public InjectAttribute (string serviceName)
		{
			ServiceName = serviceName;
		}
	}
}

