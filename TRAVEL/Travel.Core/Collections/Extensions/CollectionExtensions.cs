using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
namespace Travel.Core.Collections.Extensions
{
    /// <summary>
    /// Extension methods for collections
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Checks whether the given collection object is null or has no item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
                //return false;
            }
            if (source.Contains(item))
            {
                return false;
            }
            source.Add(item);
            return true;

        }
        public static string ArrayToCommaSeparatedString(this string[] array)
        {
            var newArray = new string[array.Length];
            var i = 0;

            foreach (var s in array)
            {
                newArray.SetValue(s.SeperateWords(), i);
                i++;
            }
            return string.Join(",", newArray);
        }
        public static string SeperateWords(this string str)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            string output = "";
            char[] chars = str.ToCharArray();

            for(int i = 0; i < chars.Length; i++)
            {
                if(i == chars.Length - 1 || i == 0 || Char.IsWhiteSpace(chars[i]))
                {
                    output += chars[i];
                    continue;
                }

                if(char.IsUpper(chars[i]) && Char.IsLower(chars[i - 1]))
                {
                    output += " " + chars[i];
                }
                else
                {
                    output += chars[i];
                }
            }
            return output;
        }
        public static string UrlEncode(this string src)
        {
            if(src == null)
            {
                return null;
            }
            return HttpUtility.UrlEncode(src);
        }
    }
}
