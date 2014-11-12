using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class SubOpTest
	{
		[Fact]
		public void Sub2BigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			BigInteger int2 = new BigInteger(5);
			Assert.True(int1 - int2 == -2);
		}
		[Fact]
		public void Sub2BigIntegerNeg()
		{
			BigInteger int1 = new BigInteger(-3);
			BigInteger int2 = new BigInteger(-5);
			Assert.True(int1 - int2 == 2);
		}
		
		[Fact]
		public void SubIntBigInteger()
		{
			BigInteger BigInteger = new BigInteger(3);
			Assert.True(BigInteger - 5 == -2);
		}
		
		[Fact]
		public void SubBigIntegerInt()
		{
			BigInteger BigInteger = new BigInteger(3);
			Assert.True(5 - BigInteger == 2);
		}
		
		[Fact]
		public void SubNullBigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			Assert.Throws<ArgumentNullException>(() => int1 = int1 - null);
		}
		
		[Fact]
		public void Sub0BigInteger()
		{
			BigInteger int1 = new BigInteger(3);
			Assert.True(int1 - 0 == 3);
			Assert.True(0 - int1 == -3);
			Assert.True(int1 - new BigInteger(0) == 3);
			Assert.True(new BigInteger(0) - int1 == -3);
		}

		[Fact]
		public void Sub0BigIntegerNeg()
		{
			BigInteger int1 = new BigInteger(-3);
			Assert.True(int1 - 0 == -3);
			Assert.True(0 - int1 == 3);
			Assert.True(int1 - new BigInteger(0) == -3);
			Assert.True(new BigInteger(0) - int1 == 3);
		}
		
		[Fact]
		public void Sub2BigBigInteger()
		{
			BigInteger int1 = new BigInteger(new uint[] { 1, 2, 3 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 3, 4, 5 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 2, 2, 2 }, true);
			Assert.True(int1 - int2 == int3);
		}
		
		[Fact]
		public void Sub2BigBigIntegerC()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue - 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 0, 1 }, false);
			Assert.True(int1 == int3 - int2);
		}
		
		[Fact]
		public void Sub2BigBigIntegerC2()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue - 1, uint.MaxValue - 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			Assert.True(int1 == int3 - int2);
		}
		
		[Fact]
		public void Sub2BigBigIntegerC3()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 1, 1 }, false);
			Assert.True(int1 == int3 - int2);
		}
		
		[Fact]
		public void Sub2BigBigIntegerC4()
		{
			BigInteger int1 = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue, 1, 1 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int3 = new BigInteger(new uint[] { 0, 1, 2, 1 }, false);
			Assert.True(int1 == int3 - int2);
		}
		
		[Fact]
		public void SubAdd()
		{
			BigInteger int1 = new BigInteger(2);
			BigInteger int2 = new BigInteger(-3);
			Assert.True(int1 - int2 == 5);
		}
	}
}
