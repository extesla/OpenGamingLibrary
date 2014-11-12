using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class MulOpFhtTest
	{
		const int StartLength     = 256;
		const int LengthIncrement = 101;
		const int RepeatCount     = 10;
		
		const int RandomStartLength = 256;
		const int RandomEndLength   = 1000;
		const int RandomRepeatCount = 50;
		
		int _length = StartLength;
		
		public uint[] GetAllOneDigits(int length)
		{
			uint[] digits = new uint[length];
			for (int i = 0; i < digits.Length; ++i) {
				digits[i] = 0x7F7F7F7F;
			}
			return digits;
		}
		
		public uint[] GetRandomDigits(int length)
		{
			Random random = new Random();
			uint[] digits = new uint[length];
			for (int i = 0; i < digits.Length; ++i) {
				digits[i] = (uint)random.Next() * 2U;
			}
			return digits;
		}

		public uint[] GetRandomDigits()
		{
			return GetRandomDigits(new Random().Next(RandomStartLength, RandomEndLength));
		}
		
		
		[Fact]
		public void CompareWithClassic()
		{
			TestHelper.Repeat(
				RepeatCount,
				delegate
				{
					BigInteger x = new BigInteger(GetAllOneDigits(_length), true);
					BigInteger classic = BigInteger.Multiply(x, x, MultiplyMode.Classic);
					BigInteger fht = BigInteger.Multiply(x, x, MultiplyMode.AutoFht);

					Assert.True(classic == fht);

					_length += LengthIncrement;
				});
		}

		[Fact]
		public void SmallLargeCompareWithClassic()
		{
			BigInteger x = new BigInteger(GetAllOneDigits(50000), false);
			BigInteger y = new BigInteger(GetAllOneDigits(512), false);
			BigInteger classic = BigInteger.Multiply(x, y, MultiplyMode.Classic);
			BigInteger fht = BigInteger.Multiply(x, y, MultiplyMode.AutoFht);

			Assert.True(classic == fht);
		}

		[Fact]
		public void CompareWithClassicRandom()
		{
			TestHelper.Repeat(
				RandomRepeatCount,
				delegate
				{
					BigInteger x = new BigInteger(GetRandomDigits(), false);
					BigInteger classic = BigInteger.Multiply(x, x, MultiplyMode.Classic);
					BigInteger fht = BigInteger.Multiply(x, x, MultiplyMode.AutoFht);

					Assert.True(classic == fht);
				});
		}
	}
}
