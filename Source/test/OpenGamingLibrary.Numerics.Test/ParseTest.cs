using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ParseTest
	{
		[Fact]
		public void Zero()
		{
			BigInteger int1 = BigInteger.Parse("0");
			Assert.True(int1 == 0);
		}
		
		[Fact]
		public void WhiteSpace()
		{
			BigInteger int1 = BigInteger.Parse("  7 ");
			Assert.True(int1 == 7);
		}
		
		[Fact]
		public void Sign()
		{
			BigInteger int1 = BigInteger.Parse("-7");
			Assert.True(int1 == -7);
			int1 = BigInteger.Parse("+7");
			Assert.True(int1 == 7);
		}
		
		[Fact]
		public void Base()
		{
			BigInteger int1 = BigInteger.Parse("abcdef", 16);
			Assert.True(int1 == 0xabcdef);
			int1 = BigInteger.Parse("100", 8);
			Assert.True(int1 == 64);
			int1 = BigInteger.Parse("0100");
			Assert.True(int1 == 64);
			int1 = BigInteger.Parse("0100000000000");
			Assert.True(int1 == 0x200000000UL);
			int1 = BigInteger.Parse("0xabcdef");
			Assert.True(int1 == 0xabcdef);
			int1 = BigInteger.Parse("0XABCDEF");
			Assert.True(int1 == 0xabcdef);
			int1 = BigInteger.Parse("020000000000");
			Assert.True(int1 == 0x80000000);
		}
		
		[Fact]
		public void Null()
		{
			Assert.Throws<ArgumentNullException>(() => BigInteger.Parse(null));
		}
		
		[Fact]
		public void InvalidFormat()
		{
			Assert.Throws<FormatException>(() => BigInteger.Parse("-123-"));
		}
		
		[Fact]
		public void InvalidFormat2()
		{
			Assert.Throws<FormatException>(() => BigInteger.Parse("abc"));
		}
		
		[Fact]
		public void InvalidFormat3()
		{
			Assert.Throws<FormatException>(() => BigInteger.Parse("987", 2));
		}
		
		[Fact]
		public void BigDec()
		{
			BigInteger BigInteger = BigInteger.Parse("34589238954389567586547689234723587070897800300450823748275895896384753238944985");
			Assert.Equal(BigInteger.ToString(), "34589238954389567586547689234723587070897800300450823748275895896384753238944985");
		}
	}
}
