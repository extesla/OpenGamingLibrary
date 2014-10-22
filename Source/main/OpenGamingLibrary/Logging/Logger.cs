// Copyright (C) 2014 Extesla, LLC.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Text;
using Common.Logging;

namespace OpenGamingLibrary.Logging
{

	/// <summary>
	/// The Logger utility is a facade around the configured logging aparatus.
	/// </summary>
	public static class Logger
	{
		const int ExceptionBufferSize = 512;

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the debug
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="message">
		/// the message, possibly with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Debug (ILog log, object message, params object[] parameters)
		{
			if (log.IsDebugEnabled)
			{
				Log(log, LogLevel.Debug, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the debug
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="ex">
		/// the <code>Exception</code>
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Debug (ILog log, Exception ex, object message, params object[] parameters)
		{
			if (log.IsDebugEnabled)
			{
				Log(log, LogLevel.Debug, ex, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the error
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Error (ILog log, object message, params object[] parameters)
		{
			if (log.IsErrorEnabled)
			{
				Log(log, LogLevel.Error, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the error
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="ex">
		/// the <code>Exception</code>
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Error (ILog log, Exception ex, object message, params object[] parameters)
		{
			if (log.IsErrorEnabled)
			{
				Log(log, LogLevel.Error, ex, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the debug
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Info (ILog log, object message, params object[] parameters)
		{
			if (log.IsInfoEnabled)
			{
				Log(log, LogLevel.Info, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the debug
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="ex">
		/// the <code>Exception</code>
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Info (ILog log, Exception ex, object message, params object[] parameters)
		{
			if (log.IsInfoEnabled)
			{
				Log(log, LogLevel.Info, ex, message, parameters);
			}
		}

		/**
		 * Log the message, with parameter placeholders resolved, if trace is enabled.
		 * @param logger the logger
		 * @param message the message, possibly with parameter placeholders
		 * @param parameters optional parameters for placeholder replacement
		 */
		public static void Trace (ILog log, object message, params object[] parameters)
		{
			if (log.IsTraceEnabled)
			{
				Log(log, LogLevel.Trace, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the trace
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="ex">
		/// the <code>Exception</code>
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Trace (ILog log, Exception ex, object message, params object[] parameters)
		{
			if (log.IsTraceEnabled)
			{
				Log(log, LogLevel.Trace, ex, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the warn
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Warn (ILog log, object message, params object[] parameters)
		{
			if (log.IsWarnEnabled)
			{
				Log(log, LogLevel.Warn, message, parameters);
			}
		}

		/// <summary>
		/// Log the message, with parameter placeholders resolved, if the warn
		/// logging level is enabled.
		/// </summary>
		/// <param name="log">
		/// the logger
		/// </param>
		/// <param name="ex">
		/// the <code>Exception</code>
		/// </param>
		/// <param name="message">
		/// the message, with placeholder parameters.
		/// </param>
		/// <param name="parameters">
		/// optional parameters for placeholder replacement
		/// </param>
		public static void Warn (ILog log, Exception ex, object message, params object[] parameters)
		{
			if (log.IsWarnEnabled)
			{
				Log(log, LogLevel.Warn, ex, message, parameters);
			}
		}

		/**
		 * Access the Log4J logger directly so we can override the FQCN to make sure if we've turned on class and/or method
		 * logging that the calling class is logged, not this one.
		 * @param logger the logger
		 * @param level the log level
		 * @param message the message, possibly with parameter placeholders
		 * @param parameters optional parameters for placeholder replacement
		 */
		public static void Log (ILog log, LogLevel level, object message, params object[] parameters)
		{
			Log(log, level, null, message, parameters);
		}

		/**
		 * Format and log message text. The supplied message pattern should conform to the syntax used by
		 * {@link MessageFormat} and the resulting text will be rendered according to the rules of
		 * {@link MessageFormat} with the exception that integer and long parameters are rendered without
		 * thousands separators.
		 * @param logger the logger The logger.
		 * @param level the log level The log level.
		 * @param t a throwable object Optional throwable to be logged.
		 * @param message The message, possibly with parameter placeholders following the rules of {@link MessageFormat}.
		 * @param parameters Optional parameters for placeholder replacement.
		 */
		public static void Log (ILog log, LogLevel level, Exception ex, object message, params object[] parameters)
		{
			String formatted;
			if (parameters == null || parameters.Length == 0)
			{
				formatted = Convert.ToString(message);
			}
			else
			{
				formatted = String.Format(Convert.ToString(message), parameters);
			}
			Log(log, level, formatted, ex);
		}

		/**
		 * Logs the exception to the appropriate log level if the exception is annotated with
		 * <code>@RestrictedLevel</code>. Otherwise log normally.
		 * @param log the logger
		 * @param logLevel the desired log level
		 * @param message the error message
		 * @param t the exception
		 */
		private static void Log (ILog log, LogLevel logLevel, object message, Exception ex)
		{
			string theMessage = BuildExceptionMessage(Convert.ToString(message), ex);
			if (logLevel.Equals(LogLevel.Fatal))
			{
				log.Fatal(theMessage, ex != null ? ex : null);
			}
			else if (logLevel.Equals(LogLevel.Error))
			{
				log.Error(theMessage, ex != null ? ex : null);
			}
			else if (logLevel.Equals(LogLevel.Warn))
			{
				log.Warn(theMessage, ex != null ? ex : null);
			}
			else if (logLevel.Equals(LogLevel.Info))
			{
				log.Info(theMessage, ex != null ? ex : null);
			}
			else if (logLevel.Equals(LogLevel.Debug))
			{
				log.Debug(theMessage, ex != null ? ex : null);
			}
			else if (logLevel.Equals(LogLevel.Trace))
			{
				//log.Trace(theMessage, ex != null ? ex : null);
				log.Debug(theMessage, ex != null ? ex : null);
			}
		}

		/// <summary>
		/// Builds a log message without including the entire stack trace of
		/// the given exception.
		/// </summary>
		/// <returns>A string which includes the message object and the
		/// exception's class name and message.</returns>
		/// <param name="message">The message object.</param>
		/// <param name="ex">The exception.</param>
		static string BuildExceptionMessage (object message, Exception ex)
		{
			if (ex == null)
			{
				return Convert.ToString(message);
			}

			var sb = new StringBuilder(Logger.ExceptionBufferSize);
			sb.Append(message);
			sb.Append(ex.GetType());
			sb.Append(": ");
			sb.Append(ex.Message);
			return sb.ToString();
		}
	}
}


