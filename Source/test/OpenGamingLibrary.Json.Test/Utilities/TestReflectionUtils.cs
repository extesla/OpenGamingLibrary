// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenGamingLibrary.Json.Test.Utilities
{

	/// <summary>
	/// </summary>
	public class TestReflectionUtils
	{
		public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
		{
			return type.GetConstructors();
		}

		public static PropertyInfo GetProperty(Type type, string name)
		{
			return type.GetProperty(name);
		}

		public static FieldInfo GetField(Type type, string name)
		{
			return type.GetField(name);
		}

		public static MethodInfo GetMethod(Type type, string name)
		{
			return type.GetMethod(name);
		}
	}
}

