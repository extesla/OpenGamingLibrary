// Copyright (c) 2014 Extesla, LLC.
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
using System.IO;
using System.Text;

namespace OpenGamingLibrary.IO
{
	public static class PathExtensions
	{
		internal static void CheckInvalidPathChars(string path, bool checkAdditional = false)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (HasIllegalCharacters(path, checkAdditional))
			{
				throw new ArgumentException("Illegal characters in path.");
			}
		}

		internal static bool HasIllegalCharacters(string path, bool checkAdditional)
		{
			for (int i = 0; i < path.Length; i++)
			{
				int num = (int)path[i];
				if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
				{
					return true;
				}
				if (checkAdditional && (num == 63 || num == 42))
				{
					return true;
				}
			}
			return false;
		}

		public static string Combine(params string[] paths)
		{
			if (paths == null)
			{
				throw new ArgumentNullException("paths");
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < paths.Length; i++)
			{
				if (paths[i] == null)
				{
					throw new ArgumentNullException("paths");
				}
				if (paths[i].Length != 0)
				{
					CheckInvalidPathChars(paths[i], false);
					if (Path.IsPathRooted(paths[i]))
					{
						num2 = i;
						num = paths[i].Length;
					}
					else
					{
						num += paths[i].Length;
					}
					char c = paths[i][paths[i].Length - 1];
					if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar)
					{
						num++;
					}
				}
			}

			var stringBuilder = new StringBuilder();
			for (int j = num2; j < paths.Length; j++)
			{
				if (paths[j].Length != 0)
				{
					if (stringBuilder.Length == 0)
					{
						stringBuilder.Append(paths[j]);
					}
					else
					{
						char c2 = stringBuilder[stringBuilder.Length - 1];
						if (c2 != Path.DirectorySeparatorChar && c2 != Path.AltDirectorySeparatorChar && c2 != Path.VolumeSeparatorChar)
						{
							stringBuilder.Append(Path.DirectorySeparatorChar);
						}
						stringBuilder.Append(paths[j]);
					}
				}
			}
			return stringBuilder.ToString();
		}


	}
}

