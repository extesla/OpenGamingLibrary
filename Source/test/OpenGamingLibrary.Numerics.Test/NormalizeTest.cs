using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class NormalizeTest
	{
		[Fact]
		public void Zero()
		{
			BigInteger int1 = new BigInteger(7) - 7;
			int1.Normalize();
			Assert.True(int1 == 0);
		}
		
		[Fact]
		public void Simple()
		{
			BigInteger int1 = new BigInteger(8);
			int1 *= int1;
			int1.Normalize();
			Assert.True(int1 == 64);
		}
	}
}
