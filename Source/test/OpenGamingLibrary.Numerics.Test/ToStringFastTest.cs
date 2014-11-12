using System;
using System.Text;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ToStringFastTest
	{
		const int StartLength     = 1024;
		const int LengthIncrement = 101;
		const int RepeatCount     = 10;
		
		const int RandomStartLength = 1024;
		const int RandomEndLength   = 4000;
		const int RandomRepeatCount = 10;
		
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
					BigInteger x = BigInteger.Parse(str, ParseMode.Fast);

					x.ToStringMode = ToStringMode.Fast;
					string strFast = x.ToString();
					x.ToStringMode = ToStringMode.Classic;
					string strClassic = x.ToString();

					Assert.Equal(str, strFast);
					Assert.Equal(strFast, strClassic);

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
					BigInteger x = BigInteger.Parse(str, ParseMode.Fast);

					x.ToStringMode = ToStringMode.Fast;
					string strFast = x.ToString();
					x.ToStringMode = ToStringMode.Classic;
					string strClassic = x.ToString();

					Assert.Equal(str, strFast);
					Assert.Equal(strFast, strClassic);
				});
		}
	}
}
