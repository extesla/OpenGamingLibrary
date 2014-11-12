using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class EqualsOpTest
	{
		[Fact]
		public void Equals2BigInteger()
		{
			BigInteger int1 = new BigInteger(8);
			BigInteger int2 = new BigInteger(8);
			Assert.True(int1.Equals(int2));
		}
		[Fact]
		public void EqualsZeroBigInteger()
		{
			Assert.False(new BigInteger(0) == 1);
		}
		
		[Fact]
		public void EqualsIntBigInteger()
		{
			BigInteger int1 = new BigInteger(8);
			Assert.True(int1 == 8);
		}
		
		[Fact]
		public void EqualsNullBigInteger()
		{
			BigInteger int1 = new BigInteger(8);
			Assert.False(int1 == null);
		}
		
		[Fact]
		public void Equals2BigIntegerOp()
		{
			BigInteger int1 = new BigInteger(8);
			BigInteger int2 = new BigInteger(8);
			Assert.True(int1 == int2);
		}
	}
}
