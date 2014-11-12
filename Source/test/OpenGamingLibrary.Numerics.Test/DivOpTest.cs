using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class DivOpTest
	{
		[Fact]
		public void Simple()
		{
			BigInteger int1 = 16;
			BigInteger int2 = 5;
			Assert.True(int1 / int2 == 3);
		}
		
		[Fact]
		public void Neg()
		{
			BigInteger int1 = -16;
			BigInteger int2 = 5;
			Assert.True(int1 / int2 == -3);
			int1 = 16;
			int2 = -5;
			Assert.True(int1 / int2 == -3);
			int1 = -16;
			int2 = -5;
			Assert.True(int1 / int2 == 3);
		}
		
		[Fact]
		public void Zero()
		{
			BigInteger int1 = 0;
			BigInteger int2 = 25;
			Assert.True(int1 / int2 == 0);
			int1 = 0;
			int2 = -25;
			Assert.True(int1 / int2 == 0);
			int1 = 16;
			int2 = 25;
			Assert.True(int1 / int2 == 0);
			int1 = -16;
			int2 = 25;
			Assert.True(int1 / int2 == 0);
			int1 = 16;
			int2 = -25;
			Assert.True(int1 / int2 == 0);
			int1 = -16;
			int2 = -25;
			Assert.True(int1 / int2 == 0);
		}
		
		[Fact]
		public void ZeroException()
		{
			BigInteger int1 = 0;
			BigInteger int2 = 0;
			Assert.Throws<DivideByZeroException>(() => int1 = int1 / int2);
		}
		
		[Fact]
		public void Big()
		{
			BigInteger int1 = new BigInteger(new uint[] {0, 0, 0x80000000U, 0x7fffffffU}, false);
			BigInteger int2 = new BigInteger(new uint[] {1, 0, 0x80000000U}, false);
			Assert.True(int1 / int2 == 0xfffffffeU);
		}
		
		[Fact]
		public void Big2()
		{
			BigInteger int1 = new BigInteger("4574586690780877990306040650779005020012387464357");
			BigInteger int2 = new BigInteger("856778798907978995905496597069809708960893");
			BigInteger int3 = new BigInteger("8567787989079799590496597069809708960893");
			BigInteger int4 = int1 * int2 + int3;
			Assert.True(int4 / int2 == int1);
			Assert.True(int4 % int2 == int3);
		}
		
		[Fact]
		public void BigDec()
		{
			BigInteger int1 = new BigInteger("100000000000000000000000000000000000000000000");
			BigInteger int2 = new BigInteger("100000000000000000000000000000000000000000");
			Assert.True(int1 / int2 == 1000);
		}
		
		[Fact]
		public void BigDecNeg()
		{
			BigInteger int1 = new BigInteger("-100000000000000000000000000000000000000000000");
			BigInteger int2 = new BigInteger("100000000000000000000000000000000000000000");
			Assert.True(int1 / int2 == -1000);
		}
	}
}
