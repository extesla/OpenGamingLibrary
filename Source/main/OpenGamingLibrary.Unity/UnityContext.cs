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
using UnityEngine;
using Common.Logging;
using OpenGamingLibrary;
using OpenGamingLibrary.Ioc;

namespace OpenGamingLibrary.Unity
{
	public class UnityContext : MonoBehaviour, IContext, ILoggable<ILog>
	{
		#region Fields
		private readonly ILog log = LogManager.GetCurrentClassLogger();
		#endregion

		#region Properties
		/// <inheritdoc />
		/// <remarks>
		/// </remarks>
		public ILog Log
		{
			get { return log; }
		}

		/// <inheritdoc />
		/// <remarks>
		/// </remarks>
		public string Name
		{
			get { return "UnityContext"; }
		}

		/// <inheritdoc/>
		/// <remarks>
		/// The <c>UnityContext</c> must be the top level context; therefore
		/// a value of <c>null</c> is returned for the parent context.
		/// </remarks>
		public IContext ParentContext { get { return null; } }

		/// <inheritdoc/>
		/// <remarks>
		/// The context's started time will be set when the context is first
		/// loaded.
		/// </remarks>
		public System.DateTime Started { get; private set; }
		#endregion

		/// <summary>
		/// Awake is called when the script instance is being loaded, and used to
		/// initialize variables and state before the game starts. Awake is called
		/// once during the lifetime of the script.
		/// </summary>
		/// <remarks>
		/// </remarks>
		void Awake()
		{
			Refresh();
			DontDestroyOnLoad(this);
		}

		public void Dispose ()
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		/// <remarks>
		/// </remarks>
		public virtual void Refresh()
		{
			// Empty.
		}
	}
}

