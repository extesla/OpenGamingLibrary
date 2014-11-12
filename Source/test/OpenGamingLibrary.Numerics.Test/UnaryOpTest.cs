using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class UnaryOpTest
	{
		[Fact]
		public void Plus()
		{
			BigInteger BigInteger = 77;
			Assert.Equal(BigInteger, +BigInteger);
		}
		
		[Fact]
		public void Minus()
		{
			BigInteger BigInteger = 77;
			Assert.True(-BigInteger == -77);
		}
		
		[Fact]
		public void Zero()
		{
			BigInteger BigInteger = 0;
			Assert.Equal(BigInteger, +BigInteger);
			Assert.Equal(BigInteger, -BigInteger);
		}
		
		[Fact]
		public void PlusPlus()
		{
			BigInteger BigInteger = 77;
			Assert.True(BigInteger++ == 77);
			Assert.True(++BigInteger == 79);
		}
		
		[Fact]
		public void MinusMinus()
		{
			BigInteger BigInteger = 77;
			Assert.True(BigInteger-- == 77);
			Assert.True(--BigInteger == 75);
		}
	}
}
