using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class CustomAlphabetTest
	{
		[Fact]
		public void AlphabetNull()
		{
			Assert.Throws<ArgumentNullException>(() => BigInteger.Parse("", 20, null));
		}

		[Fact]
		public void AlphabetShort()
		{
			Assert.Throws<ArgumentException>(() => BigInteger.Parse("", 20, "1234"));
		}

		[Fact]
		public void AlphabetRepeatingChars()
		{
			Assert.Throws<ArgumentException>(() => BigInteger.Parse("", 20, "0123456789ABCDEFGHIJ0"));
		}
		
		[Fact]
		public void Parse()
		{
			Assert.Equal(19 * 20 + 18, (int)BigInteger.Parse("JI", 20, "0123456789ABCDEFGHIJ"));
		}

		[Fact]
		public void ToStringTest()
		{
			Assert.Equal("JI", new BigInteger(19 * 20 + 18).ToString(20, "0123456789ABCDEFGHIJ"));
		}
	}
}
