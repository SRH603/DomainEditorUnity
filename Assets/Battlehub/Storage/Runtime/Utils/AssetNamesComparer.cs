using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Battlehub.Storage
{
    public class AssetNamesComparer : IComparer<string>
    {
        private static readonly Regex s_numberRegex = new Regex(@"\^d+", RegexOptions.Compiled);

        public int Compare(string x, string y)
        {
            int xNumber = ExtractNumber(x);
            int yNumber = ExtractNumber(y);

            string xName = RemoveNumber(GetFileNameWithoutExtension(x));
            string yName = RemoveNumber(GetFileNameWithoutExtension(y));

            int nameComparison = StringComparer.OrdinalIgnoreCase.Compare(xName, yName);
            if (nameComparison != 0)
            {
                return nameComparison;
            }

            return xNumber.CompareTo(yNumber);
        }

        private int ExtractNumber(string input)
        {
            Match match = s_numberRegex.Match(input);
            if (match.Success)
            {
                int number;
                if (int.TryParse(match.Value, out number))
                {
                    return number;
                }
            }
            return int.MinValue;
        }

        private string RemoveNumber(string input)
        {
            return s_numberRegex.Replace(input, "").Trim();
        }

        private string GetFileNameWithoutExtension(string input)
        {
            return Path.GetFileNameWithoutExtension(input);
        }
    }
}
