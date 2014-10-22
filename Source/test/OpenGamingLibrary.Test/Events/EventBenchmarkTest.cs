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
using System.Diagnostics;
using OpenGamingLibrary.Events;
using Xunit;

namespace OpenGamingLibrary.Test.Events
{

	/// <summary>
	/// The subscriber to the events used in testing; this listener will have
	/// the <code>OnEvent</code> method triggered when an <see cref="System.EventArgs" />
	/// message is invoked.
	/// </summary>
	class EventTestListener
	{
		static int counter = 0;
		int id;

		public void OnEvent(object sender, EventArgs e)
		{
			id = counter++;
			if (id % 100000 == 0) {
				string message = string.Format("{0}: Received. From {1}.", id, sender);
				Console.WriteLine(message);
			}
		}
	}
	
	/// <summary>
	/// The <code>NativeEventTestProvider</code> encapsulates the event being
	/// used for the native event testing and method which triggers that event
	/// being fired.
	/// </summary>
	/// <remarks>
	/// The term "Provider" is used to make a reference to the class that
	/// provides access to the event. Typically the provider of an event will
	/// be some object or service.
	/// </remarks>
	/// <author name="Sean W. Quinn" />
	//internal class NativeEventTestProvider : EventProvider<EventArgs> {

	/// <summary>
	/// The <code>WeakEventTestProvider</code>...
	/// </summary>
	/// <remarks>
	/// The term "Provider" is used to make a reference to the class that
	/// provides access to the event. Typically the provider of an event will
	/// be some object or service.
	/// </remarks>
	/// <author name="Sean W. Quinn" />
	public sealed class EventBenchmarkTest
	{
		private readonly EventRegistry eventRegistry = new EventRegistry();
		private readonly EventTestListener eventListener = new EventTestListener();

		private const int loopCount = 1000000; // 1M

		[Fact]
		public void TestNativeEventBenchmarks()
		{
			BenchmarkAddNativeEvent();
			BenchmarkFiredNativeEvent();
		}
		
		void BenchmarkAddNativeEvent()
		{
			var stopWatch = new Stopwatch();

			stopWatch.Start();
			for (int n = 0; n < loopCount; n++)
			{
				eventRegistry.Subscribe<EventArgs>(eventListener.OnEvent);
			}
			stopWatch.Stop();
			
			long ms = stopWatch.ElapsedMilliseconds;
			Console.WriteLine("Added " + loopCount + " normal listeners to notifier: "
				+ String.Format("{0:F8}", ConvertMillisecondsToSeconds(ms)) + " seconds.");
		}
		
		void BenchmarkFiredNativeEvent()
		{
			var stopWatch = new Stopwatch();
			
			stopWatch.Start();
			eventRegistry.Publish<EventArgs>(new EventArgs());
			stopWatch.Stop();
			
			long ms = stopWatch.ElapsedMilliseconds;
			Console.WriteLine("Fired " + loopCount + " normal listeners to notifier: "
				+ String.Format("{0:F8}", ConvertMillisecondsToSeconds(ms)) + " seconds.");
		}		
			
		double ConvertNanosecondsToSeconds(double nanoSeconds)
		{
			// There are 1,000,000,000 (1B) nanoseconds in a second, therefore...
			return nanoSeconds * 0.000000001;
		}
		
		double ConvertMillisecondsToSeconds(double ms)
		{
			// There are 1,000 ms in a second, therefore...
			return ms * 0.001;
		}
	}
}

