using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ImplicitConvertOpTest
	{
		[Fact]
		public void ConvertAllExceptLong()
		{
			BigInteger int1 = new int();
			Assert.True(int1 == 0);
			int1 = new uint();
			Assert.True(int1 == 0);
			int1 = new byte();
			Assert.True(int1 == 0);
			int1 = new sbyte();
			Assert.True(int1 == 0);
			int1 = new char();
			Assert.True(int1 == 0);
			int1 = new short();
			Assert.True(int1 == 0);
			int1 = new ushort();
			Assert.True(int1 == 0);
		}
		
		[Fact]
		public void ConvertLong()
		{
			BigInteger int1 = new long();
			Assert.True(int1 == 0);
			int1 = new ulong();
			Assert.True(int1 == 0);
			int1 = -123123123123;
			Assert.True(int1 == -123123123123);
		}
	}
}
