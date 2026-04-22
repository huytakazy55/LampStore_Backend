using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LampStoreProjects.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return string.Empty;
                
            string str = RemoveDiacritics(phrase).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 100 ? str.Length : 100).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            
            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D'); // Special handle for Vietnamese 'Đ'
        }
    }
}
