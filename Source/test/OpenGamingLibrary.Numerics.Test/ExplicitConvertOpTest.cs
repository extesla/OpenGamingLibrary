using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ExplicitConvertOpTest
	{
		[Fact]
		public void ConvertToInt()
		{
			int n = 1234567890;
			BigInteger BigInteger = n;
			Assert.Equal(n, (int)BigInteger);

			n = -n;
			BigInteger = n;
			Assert.Equal(n, (int)BigInteger);

			n = 0;
			BigInteger = n;
			Assert.Equal(n, (int)BigInteger);

			n = 1234567890;
			uint un = (uint)n;
			BigInteger = new BigInteger(new uint[] { un, un, un }, false);
			Assert.Equal(n, (int)BigInteger);
			BigInteger = new BigInteger(new uint[] { un, un, un }, true);
			Assert.Equal(-n, (int)BigInteger);
		}

		[Fact]
		public void ConvertToUint()
		{
			uint n = 1234567890;
			BigInteger BigInteger = n;
			Assert.Equal(n, (uint)BigInteger);

			n = 0;
			BigInteger = n;
			Assert.Equal(n, (uint)BigInteger);

			n = 1234567890;
			BigInteger = new BigInteger(new uint[] { n, n, n }, false);
			Assert.Equal(n, (uint)BigInteger);
		}

		[Fact]
		public void ConvertToLong()
		{
			long n = 1234567890123456789;
			BigInteger BigInteger = n;
			Assert.Equal(n, (long)BigInteger);

			n = -n;
			BigInteger = n;
			Assert.Equal(n, (long)BigInteger);

			n = 0;
			BigInteger = n;
			Assert.Equal(n, (long)BigInteger);

			uint un = 1234567890;
			n = (long)(un | (ulong)un << 32);
			BigInteger = new BigInteger(new uint[] { un, un, un, un, un }, false);
			Assert.Equal(n, (long)BigInteger);
			BigInteger = new BigInteger(new uint[] { un, un, un, un, un }, true);
			Assert.Equal(-n, (long)BigInteger);

			int ni = 1234567890;
			n = ni;
			BigInteger = ni;
			Assert.Equal(n, (long)BigInteger);
		}

		[Fact]
		public void ConvertToUlong()
		{
			ulong n = 1234567890123456789;
			BigInteger BigInteger = n;
			Assert.Equal(n, (ulong)BigInteger);

			n = 0;
			BigInteger = n;
			Assert.Equal(n, (ulong)BigInteger);

			uint un = 1234567890;
			n = un | (ulong)un << 32;
			BigInteger = new BigInteger(new uint[] { un, un, un, un, un }, false);
			Assert.Equal(n, (ulong)BigInteger);

			n = un;
			BigInteger = un;
			Assert.Equal(n, (ulong)BigInteger);
		}

		[Fact]
		public void ConvertToUshort()
		{
			ushort n = 12345;
			BigInteger BigInteger = n;
			Assert.Equal(n, (ushort)BigInteger);

			n = 0;
			BigInteger = n;
			Assert.Equal(n, (ushort)BigInteger);
		}


		[Fact]
		public void ConvertNullToInt()
		{
			int n;
			Assert.Throws<ArgumentNullException>(() => n = (int)(BigInteger)null);
		}

		[Fact]
		public void ConvertNullToUint()
		{
			uint n;
			Assert.Throws<ArgumentNullException>(() => n = (uint)(BigInteger)null);
		}

		[Fact]
		public void ConvertNullToLong()
		{
			long n;
			Assert.Throws<ArgumentNullException>(() => n = (long)(BigInteger)null);
		}

		[Fact]
		public void ConvertNullToUlong()
		{
			ulong n;
			Assert.Throws<ArgumentNullException>(() => n = (ulong)(BigInteger)null);
		}

		[Fact]
		public void ConvertNullToUshort()
		{
			ushort n;
			Assert.Throws<ArgumentNullException>(() => n = (ushort)(BigInteger)null);
		}
	}
}
