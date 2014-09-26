// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;
using System.IO;
using System.Text;
using OpenGamingLibrary.Utils;
using Xunit;

namespace OpenGamingLibrary.Test.Utils
{
	public class FileUtilsTest
	{

		[Fact]
		public void TestGetPath()
		{
			string path = @"..\_assets\foo.txt";
			StringBuilder expected = new StringBuilder(Directory.GetCurrentDirectory())
				.Append(Path.DirectorySeparatorChar)
				.Append(path);

			string actual = FileUtils.GetPath(path);

			Assert.NotNull(actual);
			Assert.Equal(expected.ToString(), actual);
		}

	}
}

