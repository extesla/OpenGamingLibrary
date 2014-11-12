// Copyright 2014 Extesla, LLC. All rights reserved.
//
// It is illegal to use, reproduce or distribute any part of this
// Intellectual Property without prior written authorization from
// Extesla, LLC.
using System;
using Xunit;

namespace OpenGamingLibrary.Xunit.Extensions
{
	public static class AssertException
	{

		public static void Throws<TException>(Assert.ThrowsDelegate action, params string[] possibleMessages)
			where TException : Exception
		{
			try
			{
				action();
			}
			catch (TException ex)
			{
				if (possibleMessages != null && possibleMessages.Length > 0)
				{
					bool match = false;
					foreach (string possibleMessage in possibleMessages)
					{
						if (possibleMessage.Equals(ex.Message))
						{
							match = true;
							break;
						}
					}

					if (!match) {
						throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
			}
		}
	}
}

