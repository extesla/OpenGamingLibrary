// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;
using OpenGamingLibrary.Ioc;
using Xunit;

namespace OpenGamingLibrary.Ioc.Test
{

	public class RegisteredServiceContainerTest
	{

		[Fact]
		public void TestAssign()
		{
			RegisteredServiceContainer.Assign(new NoOpServiceContainer());
			Assert.Null(RegisteredServiceContainer.GetInstance<IFilter>());

			RegisteredServiceContainer.Reset();
		}

		[Fact]
		public void TestNoAssignment()
		{
			var ex = Assert.Throws<NullReferenceException>(() => RegisteredServiceContainer.GetInstance<IFilter>());

			Assert.NotNull(ex);
			Assert.IsType<NullReferenceException>(ex);

			RegisteredServiceContainer.Reset();
		}

	}
}

