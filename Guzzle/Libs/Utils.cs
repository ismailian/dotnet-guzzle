using System;
using System.Text.RegularExpressions;

namespace Cyberliberty.Guzzle
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utils
    {

        /// <summary>
        /// Check if the input string is a file.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static bool IsFile(string input)
        {
            if (!Regex.IsMatch(input, @"^([a-zA-Z]{1}\:\\).+"))
                return false;
            return (new Uri(input).IsFile);
        }

    }
}
