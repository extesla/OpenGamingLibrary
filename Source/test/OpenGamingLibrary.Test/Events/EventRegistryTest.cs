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
using Xunit;
using OpenGamingLibrary.Events;

namespace OpenGamingLibrary.Test.Events
{

	internal class FooEventArgs : EventArgs
	{
		public int Count { get; set; }

		public FooEventArgs()
		{
			Count = 0;
		}
	}

	/// <summary>
	/// Test for event dispatching.
	/// </summary>
	public class EventRegistryTest
	{
		void IncrementCount(object sender, FooEventArgs e)
		{
			e.Count++;
		}

		[Fact]
		public void TestEventArgsPublish ()
		{
			var registry = new EventRegistry();

			var actual = 0;
			registry.Subscribe<EventArgs>((object sender, EventArgs e) => actual++);
			Assert.True(registry.Contains(typeof(EventArgs)));

			var eventArgs = new EventArgs();
			registry.Publish(eventArgs);
			Assert.Equal(1, actual);
		}

		[Fact]
		public void TestEventArgsSubscribe ()
		{
			var registry = new EventRegistry();

			var actual = 0;
			registry.Subscribe<EventArgs>((object sender, EventArgs e) => actual++);
			Assert.True(registry.Contains(typeof(EventArgs)));
		}

		[Fact]
		public void TestEventArgsUnsubscribe ()
		{
			var registry = new EventRegistry();
			var actual = 0;

			// ** Create the event handler...
			EventHandler<EventArgs> eh = (object sender, EventArgs e) => actual++;

			// ** Subscribe and assert that the event handler was in fact added to the registry...
			registry.Subscribe<EventArgs>(eh);
			Assert.True(registry.Contains(typeof(EventArgs)));

			// ** Unsubscribe the event handler; the provider created from it should still be there...
			registry.Unsubscribe<EventArgs>(eh);
			Assert.True(registry.Contains(typeof(EventArgs)));

			// ** Publish the event, the actual value should be unmodified (because the handler was removed).
			var eventArgs = new EventArgs();
			registry.Publish(eventArgs);
			Assert.Equal(0, actual);
		}

		[Fact]
		public void TestFooEventArgsPublish ()
		{
			var registry = new EventRegistry();

			// ** Verify that the event was subscribed
			registry.Subscribe<FooEventArgs>(IncrementCount);
			Assert.True(registry.Contains(typeof(FooEventArgs)));

			var eventArgs = new FooEventArgs();

			// ** Verify that the event was published.
			registry.Publish(eventArgs);
			Assert.Equal(1, eventArgs.Count);
		}

		[Fact]
		public void TestFooEventArgsSubscribe ()
		{
			var registry = new EventRegistry();

			// ** Verify that the event was subscribed
			registry.Subscribe<FooEventArgs>(IncrementCount);
			Assert.True(registry.Contains(typeof(FooEventArgs)));
		}

		[Fact]
		public void TestFooEventArgsUnsubscribe ()
		{
			var registry = new EventRegistry();

			// ** Verify that the event was subscribed
			registry.Subscribe<FooEventArgs>(IncrementCount);
			Assert.True(registry.Contains(typeof(FooEventArgs)));

			// ** Verify that the event was unsubscribed
			var eventArgs = new FooEventArgs();

			registry.Unsubscribe<FooEventArgs>(IncrementCount);
			registry.Publish(eventArgs);
			Assert.True(registry.Contains(typeof(FooEventArgs)));
			Assert.Equal(0, eventArgs.Count);
		}
	}
}

