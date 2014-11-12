using System;
using System.IO;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class AddOpTest
	{
		[Fact]
		public void Add2BigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			BigInteger int2 = new BigInteger(5);
			Assert.True(int1 + int2 == 8);
		}
		
		[Fact]
		public void Add2BigIntegerNeg()
		{
			BigInteger int1 = new BigInteger(-3);
			BigInteger int2 = new BigInteger(-5);
			Assert.True(int1 + int2 == -8);
		}
		
		[Fact]
		public void AddIntBigInteger()
		{
			BigInteger BigInteger = new BigInteger(3);
			Assert.True(BigInteger + 5 == 8);
		}
		
		[Fact]
		public void AddBigIntegerInt()
		{
			BigInteger BigInteger = new BigInteger(3);
			Assert.True(5 + BigInteger == 8);
		}
		
		[Fact]
		public void AddNullBigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			Assert.Throws<ArgumentNullException>(() => int1 = int1 + null);
		}
		
		[Fact]
		public void Add0BigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			Assert.True(int1 + 0 == 3);
			Assert.True(int1 + new BigInteger(0) == 3);
		}

		[Fact]
		public void Add0BigIntegerNeg()
		{
			BigInteger int1 = new BigInteger(-3);
			Assert.True(int1 + 0 == -3);
			Assert.True(int1 + new BigInteger(0) == -3);
			Assert.True(new BigInteger(0) + new BigInteger(-1) == -1);
		}
		
		[Fact]
		public void Add2BigBigInteger()
		{
			BigInteger int1 = new BigInteger(new uint[] { 1, 2, 3 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 3, 4, 5 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 4, 6, 8 }, false);
			Assert.True(int1 + int2 == int3);
		}
		
		[Fact]
		public void Add2BigBigIntegerC()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue - 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 0, 1 }, false);
			Assert.True(int1 + int2 == int3);
		}
		
		[Fact]
		public void Add2BigBigIntegerC2()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue - 1, uint.MaxValue - 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			Assert.True(int1 + int2 == int3);
		}
		
		[Fact]
		public void Add2BigBigIntegerC3()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 1, 1 }, false);
			Assert.True(int1 + int2 == int3);
		}
		
		[Fact]
		public void Add2BigBigIntegerC4()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue, 1, 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 1, 2, 1 }, false);
			Assert.True(int1 + int2 == int3);
		}
		
		[Fact]
		public void Fibon()
		{
			BigInteger int1 = new BigInteger(1);
			BigInteger int2 = int1;
			BigInteger int3 = null;
			for (int i = 0; i < 10000; ++i) {
				int3 = int1 + int2;
				int1 = int2;
				int2 = int3;
			}
		}
		
		[Fact]
		public void AddSub()
		{
			BigInteger int1 = new BigInteger(2);
			BigInteger int2 = new BigInteger(-2);
			Assert.True(int1 + int2 == 0);
		}
		
		// Simple output (hex Fibonacci numbers). Uncomment to see
		//[Fact]
		//public void FibonOut()
		//{
		//  uint numberBase = 16;
		//  using (StreamWriter file = File.CreateText(@"C:\fibon.txt"))
		//  {
		//    BigInteger int1 = new BigInteger(1);
		//    file.WriteLine(int1.ToString(numberBase));

		//    BigInteger int2 = int1;
		//    file.WriteLine(int2.ToString(numberBase));

		//    BigInteger int3 = null;
		//    for (int i = 0; i < 1000; ++i)
		//    {
		//      int3 = int1 + int2;
		//      file.WriteLine(int3.ToString(numberBase));
		//      int1 = int2;
		//      int2 = int3;
		//    }
		//  }
		//}
	}
}
