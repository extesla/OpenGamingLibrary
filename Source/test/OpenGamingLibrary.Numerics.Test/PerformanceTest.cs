using System;
using System.Diagnostics;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{

	public class PerformanceTest
	{
		[Fact]
		public void Multiply128BitNumbers()
		{
			BigInteger int1 = new BigInteger(new uint[] { 47668, 58687, 223234234, 42424242 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 5674356, 34656476, 45667, 678645646 }, false);

			Stopwatch stopwatch = Stopwatch.StartNew();
			
			TestHelper.Repeat(
				100000,
				delegate
					{
						BigInteger.Multiply(int1, int2, MultiplyMode.Classic);
					}
			);

			stopwatch.Stop();
			Console.WriteLine(stopwatch.ElapsedMilliseconds);
		}
	}
}
