using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ToStringTest
	{
		[Fact]
		public void VerySimple()
		{
			BigInteger BigInteger = new BigInteger(11);
			Assert.Equal(BigInteger.ToString(), "11");
		}
		
		[Fact]
		public void Simple()
		{
			BigInteger BigInteger = new BigInteger(12345670);
			Assert.Equal(BigInteger.ToString(), "12345670");
		}
		
		[Fact]
		public void Zero()
		{
			BigInteger BigInteger = new BigInteger();
			Assert.Equal(BigInteger.ToString(), "0");
		}
		
		[Fact]
		public void Neg()
		{
			BigInteger BigInteger = new BigInteger(int.MinValue);
			Assert.Equal(BigInteger.ToString(), int.MinValue.ToString());
		}
		
		[Fact]
		public void Big()
		{
			BigInteger BigInteger = new BigInteger(int.MaxValue);
			BigInteger += BigInteger += BigInteger += BigInteger;
			long longX = int.MaxValue;
			longX += longX += longX += longX;
			Assert.Equal(BigInteger.ToString(), longX.ToString());
		}
		
		[Fact]
		public void Binary()
		{
			BigInteger BigInteger = new BigInteger(19);
			Assert.Equal(BigInteger.ToString(2), "10011");
		}
		
		[Fact]
		public void Octal()
		{
			BigInteger BigInteger = new BigInteger(100);
			Assert.Equal(BigInteger.ToString(8), "144");
		}
		
		[Fact]
		public void Octal2()
		{
			BigInteger BigInteger = new BigInteger(901);
			Assert.Equal(BigInteger.ToString(8), "1605");
		}
		
		[Fact]
		public void Octal3()
		{
			BigInteger BigInteger = 0x80000000;
			Assert.Equal(BigInteger.ToString(8), "20000000000");
			BigInteger = 0x100000000;
			Assert.Equal(BigInteger.ToString(8), "40000000000");
		}
		
		[Fact]
		public void Hex()
		{
			BigInteger BigInteger = new BigInteger(0xABCDEF);
			Assert.Equal(BigInteger.ToString(16), "ABCDEF");
		}
		
		[Fact]
		public void HexLower()
		{
			BigInteger BigInteger = new BigInteger(0xFF00FF00FF00FF);
			Assert.Equal(BigInteger.ToString(16, false), "ff00ff00ff00ff");
		}
		
		[Fact]
		public void OtherBase()
		{
			BigInteger BigInteger = new BigInteger(-144);
			Assert.Equal(BigInteger.ToString(140), "-{1}{4}");
		}
	}
}
