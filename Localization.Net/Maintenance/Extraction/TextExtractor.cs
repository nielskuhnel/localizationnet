using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Localization.Net.Exceptions;

namespace Localization.Net.Maintenance.Extraction
{
    /// <summary>
    /// Provides a base for classes that extract texts from source files
    /// </summary>
    /// <typeparam name="TDef"></typeparam>
    public abstract class TextExtractor<TDef> : TextSourceBase
    {

        public string Namespace { get; set; }

        public IEnumerable<SourceFile> SourceFiles { get; set; }
        
        //Used for line numbers
        static Regex lineBreakMatcher = new Regex(@"\r\n?|[^\r]\n", RegexOptions.Compiled);


        DupplicateTextProcessor _dupplicateProcessor;

        /// <summary>
        /// Returns the dupplicate text definitions in the source files
        /// </summary>
        public Dictionary<string, List<LocalizedText>> Dupplicates
        {
            get
            {
                if (_dupplicateProcessor == null)
                {
                    Get();
                }
                return _dupplicateProcessor.Dupplicates;
            }
        }

        public override IEnumerable<LocalizedText> Get()
        {
            var texts = new List<LocalizedText>();
            foreach (var sourceFile in SourceFiles)
            {                
                foreach (var text in ProcessText(sourceFile))
                {
                    texts.Add(text);
                }
            }

            return (_dupplicateProcessor = new DupplicateTextProcessor(texts)).Process();
        }

        protected virtual IEnumerable<LocalizedText> ProcessText(SourceFile file)
        {
            //Line offsets
            var lines = lineBreakMatcher.Matches(file.Contents).Cast<Match>().Select((m, i) => new { ArrayIndex = i, Match = m }).ToArray();

            foreach (var def in GetDefinitions(file.Contents))
            {
                //Find the first line with a greater offset than the match
                var line = lines.FirstOrDefault(x => x.Match.Index > def.Index);
                var lineIndex = line == null ? lines.Length : line.ArrayIndex;
                var lineNumber = lineIndex + 1;
                var lineOffset = 1 + (lineIndex > 0 ? def.Index - (lines[lineIndex - 1].Match.Index + lines[lineIndex - 1].Match.Length) : def.Index);

                var source = file.RelativePath + ": Ln " + lineNumber + " Col " + lineOffset;

                foreach (var text in GetTexts(def))
                {
                    text.Source = new TextSourceInfo { TextSource = this, Details = source };                    
                    if (string.IsNullOrEmpty(text.Namespace))
                    {
                        text.Namespace = Namespace;
                    }
                    yield return text;
                }
            }            
        }

        /// <summary>
        /// This method must return a list of "definitions", i.e some representation of a single key and it's translations
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        protected abstract IEnumerable<Definition> GetDefinitions(string sourceText);

        /// <summary>
        /// This method must return a list of texts based on the representation of a definition
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        protected abstract IEnumerable<LocalizedText> GetTexts(Definition definition);

        
        public override void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options)
        {
            throw new LocalizedNotSupportedException("Exceptions.PutUnsupported", "This text source does not support saving texts");
        }

        protected class Definition
        {            
            /// <summary>
            /// The representation, e.g. Regex Match
            /// </summary>
            public TDef Representation;

            /// <summary>
            /// Start index of the definition in the source text
            /// </summary>
            public int Index;

            /// <summary>
            /// Length of the definition in the source text
            /// </summary>
            public int Length;
        }        
    }
}
