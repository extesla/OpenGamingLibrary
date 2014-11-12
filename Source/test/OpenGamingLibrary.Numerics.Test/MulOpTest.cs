using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class MulOpTest : IUseFixture<BigInteger>
	{
		#region IUseFixture implementation
		public void SetFixture (BigInteger data)
		{
			BigInteger.GlobalSettings.MultiplyMode = MultiplyMode.Classic;
		}
		#endregion

		[Fact]
		public void PureBigInteger()
		{
			Assert.Equal(new BigInteger(3) * new BigInteger(5), new BigInteger(15));
		}
		
		[Fact]
		public void PureBigIntegerSign()
		{
			Assert.Equal(new BigInteger(-3) * new BigInteger(5), new BigInteger(-15));
		}
		
		[Fact]
		public void IntAndBigInteger()
		{
			Assert.True(new BigInteger(3) * 5 == 15);
		}
		
		[Fact]
		public void Zero()
		{
			Assert.True(0 * new BigInteger(3) == 0);
		}
		
		[Fact]
		public void Big()
		{
			BigInteger int1   = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int2   = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger intRes = new BigInteger(new uint[] { 1, 2, 1 }, false);
			Assert.Equal(int1 * int2, intRes);
		}
		
		[Fact]
		public void Big2()
		{
			BigInteger int1   = new BigInteger(new uint[] { 1, 1 }, false);
			BigInteger int2   = new BigInteger(new uint[] { 2 }, false);
			BigInteger intRes = new BigInteger(new uint[] { 2, 2 }, false);
			Assert.Equal(intRes, int1 * int2);
			Assert.Equal(intRes, int2 * int1);
		}
		
		[Fact]
		public void Big3()
		{
			BigInteger int1   = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			BigInteger int2   = new BigInteger(new uint[] { uint.MaxValue, uint.MaxValue }, false);
			BigInteger intRes = new BigInteger(new uint[] { 1, 0, uint.MaxValue - 1, uint.MaxValue }, false);
			Assert.True(int1 * int2 == intRes);
		}
		
		[Fact]
		public void Performance()
		{
			BigInteger BigInteger  = new BigInteger(new uint[] { 0, 1 }, false);
			BigInteger BigInteger2 = BigInteger;
			for (int i = 0; i < 1000; ++i) {
				BigInteger2 *= BigInteger;
			}
		}
	}
}
