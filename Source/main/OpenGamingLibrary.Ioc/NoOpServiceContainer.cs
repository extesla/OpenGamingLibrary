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
	/// No op service container.
	/// </summary>
	public class NoOpServiceContainer : IServiceContainer
	{

		#region IDisposable implementation
		/// <inheritdoc />
		public void Dispose ()
		{
		}

		#endregion

		#region IServiceRegistry implementation
		/// <inheritdoc />
		public void Register<TService, TImplementation> ()
			where TService : class
			where TImplementation : class, TService
		{
		}

		/// <inheritdoc />
		public void Register<TService, TImplementation> (string serviceName)
			where TService : class
			where TImplementation : class, TService
		{
		}

		/// <inheritdoc />
		public void Register<TService, TImplementation> (TImplementation instance)
			where TService : class
			where TImplementation : class, TService
		{
		}

		/// <inheritdoc />
		public void RegisterAssembly (System.Reflection.Assembly assembly)
		{
		}

		/// <inheritdoc />
		public void Verify ()
		{
		}

		#endregion

		#region IServiceResolver implementation
		/// <inheritdoc />
		public object GetInstance (Type serviceType)
		{
			return null;
		}

		/// <inheritdoc />
		public TService GetInstance<TService> () where TService : class
		{
			return null;
		}

		#endregion
	}
}

