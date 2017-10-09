using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eyedrivomatic.Common.Extensions
{
    public static class StringExtensions
    {
        public static string IncrementPostfix(this string currentStr, int startNumber = 0, string seperator = null)
        {
            var regex = new Regex($@"(?<common>^.*)(?<seperator>{seperator})?(?<number>\d+)?$");
            var match = regex.Match(currentStr);
            if (!match.Success) return $"{currentStr}{seperator}{startNumber}";

            var number = match.Groups["number"].Success ? int.Parse(match.Groups["number"].Value) + 1 : startNumber;

            return $"{match.Groups["common"].Value}{seperator}{number}";
        }

        public static string NextPostfix(this string currentStr, IEnumerable<string> otherItems, int startNumber = 0, string seperator = null, bool includeNumberOnFirst = false)
        {
            var regex = string.IsNullOrEmpty(seperator) 
                ? new Regex(@"(?<common>^.*?)(?<number>\d+)?$")
                : new Regex($@"(?<common>^.*?)(?<seperator>{seperator})?(?<number>\d+)?$");

            var match = regex.Match(currentStr);
            var common = match.Success ? match.Groups["common"].Value : currentStr;

            regex = new Regex($@"(?<common>^{common})(?<seperator>{seperator})?(?<number>\d+)?$");

            var otherNumbers = otherItems.Where(item => regex.IsMatch(item))
                .Select(item =>
                {
                    var itemMatch = regex.Match(item);
                    return itemMatch.Groups["number"].Success 
                        ? int.Parse(itemMatch.Groups["number"].Value) 
                        : startNumber;
                }).ToList();

            if (!otherNumbers.Any()) return includeNumberOnFirst ? $"{common}{seperator}{startNumber}" : currentStr;

            var nextNumber = otherNumbers.Max() + 1;

            return $"{common}{seperator}{nextNumber}";
        }
    }
}
