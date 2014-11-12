// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenGamingLibrary.Json.Serialization;
using System.Text;
using System.Threading;
using Xunit;
using OpenGamingLibrary.Json.Utilities;
using System.Collections;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace OpenGamingLibrary.Json.Test
{

  
    public abstract class TestFixtureBase
    {
        protected string GetDataContractJsonSerializeResult(object o)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer s = new DataContractJsonSerializer(o.GetType());
            s.WriteObject(ms, o);

            var data = ms.ToArray();
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        protected string GetOffset(DateTime d, DateFormatHandling dateFormatHandling)
        {
            char[] chars = new char[8];
            int pos = DateTimeUtils.WriteDateTimeOffset(chars, 0, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset(), dateFormatHandling);

            return new string(chars, 0, pos);
        }

        protected string BytesToHex(byte[] bytes)
        {
            return BytesToHex(bytes, false);
        }

        protected string BytesToHex(byte[] bytes, bool removeDashes)
        {
            string hex = BitConverter.ToString(bytes);
            if (removeDashes)
                hex = hex.Replace("-", "");

            return hex;
        }

        protected byte[] HexToBytes(string hex)
        {
            string fixedHex = hex.Replace("-", string.Empty);

            // array to put the result in
            byte[] bytes = new byte[fixedHex.Length / 2];
            // variable to determine shift of high/low nibble
            int shift = 4;
            // offset of the current byte in the array
            int offset = 0;
            // loop the characters in the string
            foreach (char c in fixedHex)
            {
                // get character code in range 0-9, 17-22
                // the % 32 handles lower case characters
                int b = (c - '0') % 32;
                // correction for a-f
                if (b > 9) b -= 7;
                // store nibble (4 bits) in byte array
                bytes[offset] |= (byte)(b << shift);
                // toggle the shift variable between 0 and 4
                shift ^= 4;
                // move to next byte
                if (shift != 0) offset++;
            }
            return bytes;
        }

        protected void TestSetup()
        {
            JsonConvert.DefaultSettings = null;
        }

        protected void WriteEscapedJson(string json)
        {
            Console.WriteLine(EscapeJson(json));
        }

        protected string EscapeJson(string json)
        {
            return @"@""" + json.Replace(@"""", @"""""") + @"""";
        }
    }

    public static class CustomAssert
    {
        public static void Contains(IList collection, object value)
        {
            Contains(collection, value, null);
        }

        public static void Contains(IList collection, object value, string message)
        {
            if (!collection.Cast<object>().Any(i => i.Equals(value)))
                throw new Exception(message ?? "Value not found in collection.");
        }
    }

    public static class StringAssert
    {
        private readonly static Regex Regex = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.CultureInvariant);

        public static void Equal(string expected, string actual)
        {
            expected = Normalize(expected);
            actual = Normalize(actual);

            Assert.Equal(expected, actual);
        }

        public static bool Equals(string s1, string s2)
        {
            s1 = Normalize(s1);
            s2 = Normalize(s2);

            return string.Equals(s1, s2);
        }

        public static string Normalize(string s)
        {
            if (s != null)
                s = Regex.Replace(s, "\r\n");

            return s;
        }
    }
}