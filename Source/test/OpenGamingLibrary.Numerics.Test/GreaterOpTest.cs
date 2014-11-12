using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class GreaterOpTest
	{
		[Fact]
		public void Simple()
		{
			BigInteger int1 = new BigInteger(7);
			BigInteger int2 = new BigInteger(8);
			Assert.True(int2 > int1);
		}
		
		[Fact]
		public void SimpleFail()
		{
			BigInteger int1 = new BigInteger(8);
			Assert.False(7 > int1);
		}
		
		[Fact]
		public void Big()
		{
			BigInteger int1 = new BigInteger(new uint[] { 1, 2 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 2, 3 }, true);
			Assert.True(int1 > int2);
		}
		
		[Fact]
		public void BigFail()
		{
			BigInteger int1 = new BigInteger(new uint[] { 1, 2 }, false);
			BigInteger int2 = new BigInteger(new uint[] { 1, 2, 3 }, true);
			Assert.False(int2 > int1);
		}
		
		[Fact]
		public void EqualValues()
		{
			BigInteger int1 = new BigInteger(new uint[] { 1, 2, 3 }, true);
			BigInteger int2 = new BigInteger(new uint[] { 1, 2, 3 }, true);
			Assert.False(int2 > int1);
		}
	}
}
