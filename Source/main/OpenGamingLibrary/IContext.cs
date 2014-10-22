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

namespace OpenGamingLibrary
{

	/// <summary>
	/// A class which defines a set of data used by a task, i.e. application,
	/// in relation to the work being performed.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Under most circumstances an application, game, etc. will have one
	/// context; or at least one active context. There are cases where a piece
	/// of software will have a production context, as well as a context for
	/// testing the systems and subsystems.
	/// </para>
	/// <para>
	/// The context in this case becomes the framework by which the rest of
	/// an application is configured. A context becomes responsible for
	/// triggering the configuration of subsystems, populating inversion of
	/// control containers, and otherwise setting up everything that will
	/// eventually be needed by the application.
	/// </para>
	/// <para>
	/// Contexts can be hierarchical, wherein different contexts can be
	/// responsible for the configuration of different subsystems (e.g. data
	/// access, security, etc.) or can be represented by different configuration
	/// inputs (e.g. XML, code, etc.).
	/// </para>
	/// </remarks>
	/// <author>Sean Quinn</author>
	public interface IContext : IDisposable
	{

		/// <summary>
		/// Gets the name of the context.
		/// </summary>
		/// <value>The name of the context.</value>
		string Name { get; }

		/// <summary>
		/// Gets the parent context, or <see cref="null"/>  if there is no
		/// parent context
		/// </summary>
		/// <remarks>
		/// If the parent context is <see cref="null"/>, then this context
		/// is the root of any context hierarchy.
		/// </remarks>
		/// <value>The parent context.</value>
		IContext ParentContext { get; }

		/// <summary>
		/// Gets the date and time when this context was first loaded.
		/// </summary>
		/// <value>The <see cref="System.DateTime"/> representing when this
		/// context was first loaded.
		/// </value>
		DateTime Started { get; }

		/// <summary>
		/// Load or refresh the representation of the configuration.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The configuration may be represented by a persistent model such as
		/// an XML file, properties file, or relational database schema. Or the
		/// configuration could be represented in code by a compositional root.
		/// </para>
		/// <para>
		/// Refresh should always be called as part of the start of a context,
		/// as such it destroy already created singletons if it fails; to avoid
		/// dangling resources. In other words, after invocation of this method,
		/// either all or no singletons at all should be instantiated.
		/// </remarks>
		void Refresh();
	}
}

