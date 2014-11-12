using System.IO;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class PowTest : IUseFixture<BigInteger>
	{
		#region IUseFixture implementation
		public void SetFixture (BigInteger data)
		{
			BigInteger.GlobalSettings.MultiplyMode = MultiplyMode.Classic;
		}
		#endregion
		
		[Fact]
		public void Simple()
		{
			BigInteger BigInteger = new BigInteger(-1);
			Assert.True(BigInteger.Pow(BigInteger, 17) == -1);
			Assert.True(BigInteger.Pow(BigInteger, 18) == 1);
		}
		
		[Fact]
		public void Zero()
		{
			BigInteger BigInteger = new BigInteger(0);
			Assert.True(BigInteger.Pow(BigInteger, 77) == 0);
		}

		[Fact]
		public void PowZero()
		{
			Assert.True(BigInteger.Pow(0, 0) == 1);
		}

		[Fact]
		public void PowOne()
		{
			Assert.True(BigInteger.Pow(7, 1) == 7);
		}

		[Fact]
		public void Big()
		{
			Assert.Equal(BigInteger.Pow(2, 65).ToString(), "36893488147419103232");
		}
		
		// Simple output (2^65536). Uncomment to see
		//[Fact]
		//public void TwoNOut()
		//{
		//  using (StreamWriter file = File.CreateText(@"C:\2n65536.txt"))
		//  {
		//    file.WriteLine(BigInteger.Pow(2, 65536));
		//  }
		//}
	}
}
