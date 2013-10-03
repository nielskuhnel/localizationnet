using System.Collections.Generic;

namespace Localization.Net.Maintenance
{
    public class DupplicateTextProcessor
    {
        IEnumerable<LocalizedText> _texts;

        public Dictionary<string, List<LocalizedText>> Dupplicates { get; private set; }

        public DupplicateTextProcessor(IEnumerable<LocalizedText> texts)
        {
            _texts = texts;
        }

        public IEnumerable<LocalizedText> Process()
        {
            Dupplicates = new Dictionary<string, List<LocalizedText>>();

            var processedTexts = new Dictionary<string, LocalizedText>();            
            foreach (var text in _texts)
            {
                var key = text.UniqueKey;
                LocalizedText existing;
                if (processedTexts.TryGetValue(key, out existing))
                {
                    List<LocalizedText> dupplicateList;
                    if (!Dupplicates.TryGetValue(text.Key, out dupplicateList))
                    {
                        Dupplicates.Add(text.Key, (dupplicateList = new List<LocalizedText>()));
                    }
                    dupplicateList.Add(text);

                    if (existing.Source != null)
                    {
                        existing.Source.Details += ", " + text.Source.Details;
                    }

                    //Prefer texts with better quality
                    if (text.Quality > existing.Quality)
                    {
                        text.Source = existing.Source;
                        processedTexts[key] = text;
                    }                    
                }
                else
                {
                    processedTexts.Add(key, text);
                }
            }

            return processedTexts.Values;        
        }
    }
}
