using System;
using System.Collections.Generic;

namespace FSMForUnity.Editor
{
    internal class StringFormatCache<T> where T : IEquatable<T>
    {
        private readonly string format;

        private readonly Dictionary<T, string> generated = new Dictionary<T, string>(32);

        public StringFormatCache(string format)
        {
            this.format = format;
        }

        public string Get(T value)
        {
            if (generated.TryGetValue(value, out var str))
            {
                return str;
            }
            else
            {
                var s = string.Format(format, value);
                generated.Add(value, s);
                return s;
            }
        }
    }
}
