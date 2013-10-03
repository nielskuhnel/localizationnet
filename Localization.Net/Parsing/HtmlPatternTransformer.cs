using System.Text.RegularExpressions;

namespace Localization.Net.Parsing
{
    /// <summary>
    /// Allows html tags to be used.    
    /// </summary>
    public class HtmlPatternTransformer : IPatternTransformer
    {        
        static Regex tagMatcher = new Regex(@"<[/a-z]+(?<Colon>:)?[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public string Encode(string pattern)
        {

            if (pattern != null)
            {
                return tagMatcher
                    .Replace(pattern, (m) => m.Groups["Colon"].Success ? m.Value : "%%lt%%" + m.Value.Substring(1, m.Value.Length - 2) + "%%gt%%");
            }
            return pattern;
        }
        
        static Regex decoder = new Regex(@"%%(?<Entity>[^%]+)%%", RegexOptions.Compiled);
        public string Decode(string encodedPattern)
        {
            if (encodedPattern != null)
            {
                return decoder.Replace(encodedPattern, (m) =>
                {
                    string name = m.Groups["Entity"].Value;
                    if (name == "lt") return "<";
                    else if( name == "gt") return ">";
                    return m.Value;
                });
            }
            return encodedPattern;
        }
    }
}
