using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class DivOpNewtonTest
	{
		const int StartLength     = 1024;
		const int LengthIncrement = 101;
		const int RepeatCount     = 10;
		
		const int RandomStartLength = 1024;
		const int RandomEndLength   = 2048;
		const int RandomRepeatCount = 25;
		
		int _length = StartLength;
		
		public uint[] GetAllOneDigits(int length)
		{
			uint[] digits = new uint[length];
			for (int i = 0; i < digits.Length; ++i) {
				digits[i] = 0xFFFFFFFF;
			}
			return digits;
		}
		
		public uint[] GetRandomDigits(out uint[] digits2)
		{
			Random random = new Random();
			uint[] digits = new uint[random.Next(RandomStartLength, RandomEndLength)];
			digits2 = new uint[digits.Length / 2];
			byte[] bytes = new byte[4];
			for (int i = 0; i < digits.Length; ++i) {
				random.NextBytes(bytes);
				digits[i] = BitConverter.ToUInt32(bytes, 0);
				if (i < digits2.Length) {
					random.NextBytes(bytes);
					digits2[i] = BitConverter.ToUInt32(bytes, 0);
				}
			}
			return digits;
		}
		
		
		[Fact]
		public void CompareWithClassic()
		{
			TestHelper.Repeat(RepeatCount,
				delegate
				{
					BigInteger x = new BigInteger(GetAllOneDigits(_length), true);
					BigInteger x2 = new BigInteger(GetAllOneDigits(_length / 2), true);

					BigInteger classicMod, fastMod;
					BigInteger classic = BigInteger.DivideModulo(x, x2, out classicMod, DivideMode.Classic);
					BigInteger fast = BigInteger.DivideModulo(x, x2, out fastMod, DivideMode.AutoNewton);

					Assert.True(classic == fast);
					Assert.True(classicMod == fastMod);

					_length += LengthIncrement;
				});
		}

		[Fact]
		public void CompareWithClassicRandom()
		{
			TestHelper.Repeat(RandomRepeatCount,
				delegate
				{
					uint[] digits2;
					BigInteger x = new BigInteger(GetRandomDigits(out digits2), false);
					BigInteger x2 = new BigInteger(digits2, false);

					BigInteger classicMod, fastMod;
					BigInteger classic = BigInteger.DivideModulo(x, x2, out classicMod, DivideMode.Classic);
					BigInteger fast = BigInteger.DivideModulo(x, x2, out fastMod, DivideMode.AutoNewton);

					Assert.True(classic == fast);
					Assert.True(classicMod == fastMod);
				});
		}
	}
}
