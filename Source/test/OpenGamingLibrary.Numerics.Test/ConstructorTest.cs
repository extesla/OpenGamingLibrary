using System;
using Xunit;

namespace OpenGamingLibrary.Numerics.Test
{
	
	public class ConstructorTest
	{
		[Fact]
		public void DefaultCtor()
		{
			new BigInteger();
		}
		
		[Fact]
		public void IntCtor()
		{
			new BigInteger(7);
		}
		
		[Fact]
		public void UIntCtor()
		{
			new BigInteger(uint.MaxValue);
		}
		
		[Fact]
		public void IntArrayCtor()
		{
			new BigInteger(new uint[] { 1, 2, 3 }, true);
		}
		
		[Fact]
		public void IntArrayNullCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new BigInteger(null, false));
		}
	}
}
