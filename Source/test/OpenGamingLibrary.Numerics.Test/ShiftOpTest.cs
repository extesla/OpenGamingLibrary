using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ShiftOpTest
	{
		[Fact]
		public void Zero()
		{
			BigInteger int1 = new BigInteger(0);
			Assert.True(int1 << 100 == 0);
			Assert.True(int1 >> 100 == 0);
		}
		
		[Fact]
		public void SimpleAndNeg()
		{
			BigInteger int1 = new BigInteger(8);
			Assert.True(int1 << 0 == int1 >> 0 && int1 << 0 == 8);
			Assert.True(int1 << 32 == int1 >> -32 && int1 << 32 == new BigInteger(new uint[] { 0, 8 }, false));
		}
		
		[Fact]
		public void Complex()
		{
			BigInteger int1 = new BigInteger("0x0080808080808080");
			Assert.True((int1 << 4).ToString(16) == "808080808080800");
			Assert.True(int1 >> 36 == 0x80808);
		}
		
		[Fact]
		public void BigShift()
		{
			BigInteger int1 = 8;
			Assert.True(int1 >> 777 == 0);
		}

		[Fact]
		public void MassiveShift()
		{
			for (int i = 1; i < 2000; i++)
			{
				BigInteger n = i;
				n = n << i;
				n = n >> i;
				Assert.Equal(new BigInteger(i), n);
			}
		}
	}
}
