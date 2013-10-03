using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Localization.Net.Maintenance.Extraction
{
    public class CStyleLanguageTextExtractor : TextExtractor<Match>
    {


        /// <summary>
        /// Gets or sets the language for empty /* @L10n */ markers.
        /// </summary>
        /// <value>
        /// The marker language.
        /// </value>
        public string MarkerLanguage { get; set; }

        /// <summary>
        /// Gets or sets the marker/default text for empty /* @L10n */ markers.
        /// </summary>
        /// <value>
        /// The marker patern.
        /// </value>
        public string MarkerText { get; set; }
        


        public CStyleLanguageTextExtractor()
        {
            MarkerLanguage = "";
            MarkerText = "(Todo)";
        }

        /// <summary>
        /// A string literal. C#'s @"Bla, bla, ""bla""" is also supported, and JavaScript's 'This is a string'
        /// </summary>
        static string _stringLiteral = @"
              (
                    (?<Ad>) \@""(?<Key> (""""|[^""])* )"" 
                  | ""(?<Key> (\\[^\n]|[^\\""\n])* )""
                  | '(?<Key> (\\[^\n]|[^\\'\n])* )'
              )";

        /// <summary>
        /// A comment that starts with @L10n
        /// </summary>
        static string _l10nComment = @"([ \t]* \/\/ [ \t]* @L10n\:? [ \t]* (?<Defaults> [^\r\n]* )
              | \s* \/\* \s* @L10n\:? \s* (?<Defaults> .*? ) \*\/)";


        /// <summary>
        /// This one recognizes L10n definitions with keys (in strings) and translations (in comments).
        /// Currently only this simple pattern is supported: "Key" (whitespace) /* @L10n @da-DK Hej @en-UK Hello
        /// </summary>
        static Regex _definitionMatcher = new Regex(string.Format(@"{0} (\s|;)* {1}", _stringLiteral, _l10nComment), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // This extracts the individual language defaults in the @L10n comment
        static Regex _translationMatcher = new Regex(@"\s*@(?<Lang>[^:\s]+):?(?<Pattern>(\{[^\}]+\}|\\@|[^@])*)", RegexOptions.Compiled | RegexOptions.Singleline);

        
        /// <summary>
        /// Used to remove VS's leading asterisks from multi line comments.
        /// </summary>
        /* Multi line
         * comment 
         * example*/
        static Regex _bolMatcher = new Regex(@"(?<=\n)[ \t]+(\*[ \t]*)?", RegexOptions.Compiled | RegexOptions.Singleline);

        //Used to find escaped characters
        static Regex _escapedCharMatcher = new Regex(@"\\(?<Char>.)", RegexOptions.Compiled);


        protected override IEnumerable<TextExtractor<Match>.Definition> GetDefinitions(string sourceText)
        {
            foreach (Match def in _definitionMatcher.Matches(sourceText))
            {
                yield return new Definition
                {
                    Representation = def,
                    Index = def.Index,
                    Length = def.Length
                };
            }
        }

        protected override IEnumerable<LocalizedText> GetTexts(TextExtractor<Match>.Definition definition)
        {
            var def = definition.Representation;
            
            var key = Unescape(def.Groups["Key"].Value, def.Groups["Ad"].Success);

            var defaults = _bolMatcher.Replace(def.Groups["Defaults"].Value.Trim(), "");

            var hasLanguages = false;
            foreach (Match trans in _translationMatcher.Matches(defaults))
            {
                var lang = trans.Groups["Lang"].Value;
                string pattern = trans.Groups["Pattern"].Value.Replace("\\@", "@").Trim();
                //Whitespace is trimmed. If leading or trailing whitespace is needed the pattern can be quoted. If a pattern must start with '"' use ""Pattern"
                if (pattern.StartsWith("\"") && pattern.EndsWith("\""))
                {
                    pattern = Unescape(pattern.Substring(1, pattern.Length - 2), false);
                }

                var text = new LocalizedText
                {
                    Key = key,
                    Pattern = pattern,
                    Language = lang
                };

                hasLanguages = true;

                yield return text;
            }

            if (!hasLanguages)
            {
                //Handle the L10n marker
                yield return new LocalizedText
                {
                    Key = key,
                    Language = MarkerLanguage,
                    Pattern = MarkerText.Replace("{#}", key),
                    Quality = TextQuality.PlaceHolder
                };
            }
        }
             


        string Unescape(string literal, bool multiline)
        {
            if (multiline)
            {
                return literal.Replace("\"\"", "\"");
            }
            else
            {
                return _escapedCharMatcher.Replace(literal, (m) =>
                {
                    switch (m.Groups["Char"].Value)
                    {
                        case "a": return "\a";
                        case "b": return "\b";
                        case "n": return "\n";
                        case "r": return "\r";
                        case "f": return "\f";
                        case "t": return "\t";
                        case "v": return "\v";
                        default: return m.Groups["Char"].Value;
                    }
                });
            }
        }        
        
    }    
}
