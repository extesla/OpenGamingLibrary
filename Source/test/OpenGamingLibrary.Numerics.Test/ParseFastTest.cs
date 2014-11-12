using System;
using System.Text;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ParseFastTest
	{
		const int StartLength     = 1024;
		const int LengthIncrement = 101;
		const int RepeatCount     = 50;
		
		const int RandomStartLength = 1024;
		const int RandomEndLength   = 4000;
		const int RandomRepeatCount = 50;
		
		int _length = StartLength;
		
		public string GetAllNineChars(int length)
		{
			return new string('9', length);
		}
		
		public string GetRandomChars()
		{
			Random random = new Random();
			int length = random.Next(RandomStartLength, RandomEndLength);
			StringBuilder builder = new StringBuilder(length);
			
			builder.Append((char)random.Next('1', '9'));
			--length;
			
			while (length-- != 0) {
				builder.Append((char)random.Next('0', '9'));
			}
			return builder.ToString();
		}
		
		
		[Fact]
		public void CompareWithClassic()
		{
			TestHelper.Repeat(
				RepeatCount,
				delegate
				{
					string str = GetAllNineChars(_length);
					BigInteger classic = BigInteger.Parse(str, ParseMode.Classic);
					BigInteger fast = BigInteger.Parse(str, ParseMode.Fast);

					Assert.True(classic == fast);

					_length += LengthIncrement;
				});
		}
		
		[Fact]
		public void CompareWithClassicRandom()
		{
			TestHelper.Repeat(
				RandomRepeatCount,
				delegate
				{
					string str = GetRandomChars();
					BigInteger classic = BigInteger.Parse(str, ParseMode.Classic);
					BigInteger fast = BigInteger.Parse(str, ParseMode.Fast);

					Assert.True(classic == fast);
				});
		}
	}
}
