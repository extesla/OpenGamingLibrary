using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ModOpTest
	{
		[Fact]
		public void Simple()
		{
			BigInteger int1 = 16;
			BigInteger int2 = 5;
			Assert.True(int1 % int2 == 1);
		}
		
		[Fact]
		public void Neg()
		{
			BigInteger int1 = -16;
			BigInteger int2 = 5;
			Assert.True(int1 % int2 == -1);
			int1 = 16;
			int2 = -5;
			Assert.True(int1 % int2 == 1);
			int1 = -16;
			int2 = -5;
			Assert.True(int1 % int2 == -1);
		}
		
		[Fact]
		public void Zero()
		{
			BigInteger int1 = 0;
			BigInteger int2 = 25;
			Assert.True(int1 % int2 == 0);
			int1 = 0;
			int2 = -25;
			Assert.True(int1 % int2 == 0);
			int1 = 16;
			int2 = 25;
			Assert.True(int1 % int2 == 16);
			int1 = -16;
			int2 = 25;
			Assert.True(int1 % int2 == -16);
			int1 = 16;
			int2 = -25;
			Assert.True(int1 % int2 == 16);
			int1 = -16;
			int2 = -25;
			Assert.True(int1 % int2 == -16);
			int1 = 50;
			int2 = 25;
			Assert.True(int1 % int2 == 0);
			int1 = -50;
			int2 = -25;
			Assert.True(int1 % int2 == 0);
		}
		
		[Fact]
		public void ZeroException()
		{
			BigInteger int1 = 0;
			BigInteger int2 = 0;
			Assert.Throws<DivideByZeroException>(() => int1 = int1 % int2);
		}
		
		[Fact]
		public void Big()
		{
			BigInteger int1 = new BigInteger(new uint[] {0, 0, 0x80000000U, 0x7fffffffU}, false);
			BigInteger int2 = new BigInteger(new uint[] {1, 0, 0x80000000U}, false);
			BigInteger intM = new BigInteger(new uint[] {2, 0xffffffffU, 0x7fffffffU}, false);
			Assert.True(int1 % int2 == intM);
		}
		
		[Fact]
		public void BigDec()
		{
			BigInteger int1 = new BigInteger("100000000000000000000000000000000000000000000");
			BigInteger int2 = new BigInteger("100000000000000000000000000000000000000000");
			Assert.True(int1 % int2 == 0);
		}
		
		[Fact]
		public void BigDecNeg()
		{
			BigInteger int1 = new BigInteger("-100000000000000000000000000000000000000000001");
			BigInteger int2 = new BigInteger("100000000000000000000000000000000000000000");
			Assert.True(int1 % int2 == -1);
		}
	}
}
