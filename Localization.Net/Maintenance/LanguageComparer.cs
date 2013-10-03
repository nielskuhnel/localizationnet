using System.Collections.Generic;
using System.Linq;

namespace Localization.Net.Maintenance
{
    /// <summary>
    /// Compares two languages in a text source to see if untranslated texts exist in the source language or text with no matching keys exist in the target (e.g. because of typos in keys)
    /// </summary>
    public class LanguageComparer
    {

        public ITextSource Texts { get; set; }

        public LanguageComparer(ITextSource texts)
        {
            Texts = texts;
        }


        public LanguageComparison Compare(string sourceLanguage, string targetLanguage)
        {

            var comparison = new LanguageComparison
            {
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage
            };


            var balance = new Dictionary<string, List<LocalizedText>>();
            foreach (var text in Texts.Get())
            {
                if (text.Language == sourceLanguage ||
                    text.Language == targetLanguage)
                {
                    List<LocalizedText> list;
                    if (!balance.TryGetValue(text.Key, out list))
                    {
                        balance.Add(text.Key, (list = new List<LocalizedText>()));
                    }
                    list.Add(text);
                }
            }

            var singleTexts = balance.Values.Where(x => x.Count == 1).Select(x => x.First());

            comparison.MissingTexts = singleTexts.Where(x => x.Language == sourceLanguage).ToList();
            comparison.UnmatchedTexts = singleTexts.Where(x => x.Language == targetLanguage).ToList();

            return comparison;
        }
    }

    public class LanguageComparison
    {
        /// <summary>
        /// Gets or sets a value indicating whether the two languages contains the same keys
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get { return !MissingTexts.Any() && !UnmatchedTexts.Any(); } }

        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }

        /// <summary>
        /// Source texts that doesn't exist in the target language
        /// </summary>
        public List<LocalizedText> MissingTexts { get; set; }

        /// <summary>
        /// Texts in the target language that doesn't match source texts
        /// </summary>
        public List<LocalizedText> UnmatchedTexts { get; set; }
    }
}
