using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NetworkAdapter.Tools
{
    public struct StringCursor
    {
        public string str;
        public int index;

        public bool IsEnded => index == str.Length;

        public StringCursor(string str)
        {
            this.str = str;
            index = 0;
        }

        public string Read(int count)
        {
            var s = str.Substring(index, count);
            index += count;
            return s;
        }

        public string ReadTo(char chr, bool includeEnd = true)
        {
            var end = str.IndexOf(chr, index) + 1;
            var idx = end - (includeEnd ? 0 : 1);
            var s = str.Substring(index, idx - index);
            index = end;
            return s;
        }

        public string ReadTo(string phrase, bool includeEnd = true)
        {
            var end = str.IndexOf(phrase, index, StringComparison.Ordinal) + phrase.Length;
            var idx = end - (includeEnd ? 0 : phrase.Length);
            var s = str.Substring(index, idx - index);
            index = end;
            return s;
        }

        public string ReadToEnd()
        {
            var s = str.Substring(index);
            index = str.Length;
            return s;
        }

        public void Skip(int count)
        {
            index += count;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}