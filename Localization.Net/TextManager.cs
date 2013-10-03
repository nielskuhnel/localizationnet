using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Localization.Net.Maintenance;
using Localization.Net.Processing;
using Localization.Net.Support;

namespace Localization.Net
{
    public abstract class TextManager
    {

        public TextSourceAggregator Texts { get; private set; }

        /// <summary>
        /// Assemblies to exclude from the scan to find text
        /// </summary>
        internal static readonly string[] KnownAssemblyExclusionFilter = new[]
            {
                "System.",
                "Antlr3.",
                "Autofac.",
                "Autofac,",
                "Castle.",
                "ClientDependency.",
                "DataAnnotationsExtensions.",
                "DataAnnotationsExtensions,",
                "Dynamic,",
                "FluentNHibernate,",
                "HibernatingRhinos.Profiler.Appender.",
                "HtmlDiff,",
                "Iesi.Collections,",
                "log4net,",
                "Microsoft.",
                "Newtonsoft.",
                "NHibernate.",
                "NHibernate,",
                "NuGet.",
                "Remotion.",
                "RouteDebugger.",                
                "Lucene.",
                "Examine,",
                "Examine."
            };

        /// <summary>
        /// Gets or sets the handler that controls the response when texts are missing. (Default is null).
        /// </summary>
        /// <value>
        /// The missing text handler.
        /// </value>
        public MissingTextHandler MissingTextHandler { get; set; }

        public string DefaultNamespace { get; set; }

        /// <summary>
        /// If not matching texts are found in the specified namespace these namespaces will be searched in reverse order
        /// </summary>
        public IList<string> FallbackNamespaces { get; set; }


        /// <summary>
        /// Gets or sets the fallback languages. These are used for all language infos if no matching text is found in reverse order
        /// </summary>
        /// <value>
        /// The fallback languages.
        /// </value>
        public IEnumerable<LanguageInfo> FallbackLanguages { get; set; }

        /// <summary>
        /// Namespace/Key/Language
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, LocalizedTextCacheEntry>>> _textCache =
            new Dictionary<string, Dictionary<string, Dictionary<string, LocalizedTextCacheEntry>>>();

        /// <summary>
        /// Gets the current entries.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, LocalizedTextCacheEntry>>> CurrentEntries
        {
            get
            {
                ParseAllPatterns();
                return _textCache;
            }
        }



        public IDictionary<string, PatternDialect> Dialects { get; private set; }
        protected TextManager()
        {
            //TODO: Point of interest. Avoid defaults? Read from configuration? Neither?

            Dialects = new Dictionary<string, PatternDialect>();
            Dialects.Add("Default", new DefaultDialect());
            Dialects.Add("Text", new TextDialect());

            Texts = new TextSourceAggregator();
            Texts.TextsChanged += SourceTextsChanged;

            FallbackNamespaces = new List<string>();
        }



        /// <summary>
        /// Gets the text with the specified namespace and key for the specified language.
        /// If namespace is null the assembly asm is passed to the implementation of GetCurrentNamespace
        /// </summary>
        /// <typeparam name="TNamespace">The type's assembly will be used for namespacing if ns is omitted</typeparam>
        /// <param name="key">The key of the text to get</param>
        /// <param name="values">An IDictionary&lt;string,object&gt; or anonymous type</param>
        /// <param name="language">If null the current language</param>
        /// <param name="ns">If null the current namespace</param>
        /// <param name="debug">Show debug output instead of evaluating the text's pattern. If null (the default) the current implementation of IsInDebugMode is used</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>
        /// The translated string
        /// </returns>
        public string Get<TNamespace>(string key, object values = null,
            LanguageInfo language = null, string ns = null, bool? debug = null, 
            bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            return Get(key, values, language, ns, TrackCallingAssembly ? typeof(TNamespace).Assembly : null, debug, returnNullOnMissing, encode);
        }

        protected virtual void EnsureLoaded()
        {
            
        }

        /// <summary>
        /// Gets the text with the specified namespace and key for the specified language.
        /// If namespace is null the assembly asm is passed to the implementation of GetCurrentNamespace
        /// </summary>
        /// <param name="ns">If null the current namespace</param>
        /// <param name="key">The key of the text to get</param>
        /// <param name="language">If null the current language</param>        
        /// <param name="callingAssembly">The calling assembly. If helper methods invoke this method they should pass Assembly.GetCallingAssembly() to ensure that namespaces are handled correctly</param>        
        /// <param name="debug">Show debug output instead of evaluating the text's pattern</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>The translated string</returns>
        public string Get(string key, object values = null,
            LanguageInfo language = null, string ns = null, Assembly callingAssembly = null, bool? debug = null, 
            bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            EnsureLoaded();

            //Sad: Assembly.GetCallingAssembly isn't reliable. MVC views, compiled Linq expressions etc. return rubbish

            bool debugMode = debug ?? IsInDebugMode();

            ns = ns ?? GetCurrentNamespace(TrackCallingAssembly ? callingAssembly : null);

            ns = ns ?? (DefaultNamespace ?? "");

            language = language ?? GetCurrentLanguage();

            var dict = ObjectHelper.ParamsToParameterSet(values, addWithIndex: true);

            if (debugMode)
            {
                return DebugText(ns, key, language, dict);
            }


            var entry = GetTextEntry(ns, key, language, true);
            if (entry != null)
            {                
                return entry.Evaluator.Evaluate(new EvaluationContext
                {
                    Parameters = dict,
                    Language = language,
                    TimeZoneInfo = GetCurrentTimeZoneInfo(),
                    Namespace = ns,
                    StringEncoder = encode && StringEncoder != null && entry.PatternDialect.Encode ? StringEncoder : ((x) => x) //Use identity transform if no transformer is specified
                });
            }

            if (!returnNullOnMissing && MissingTextHandler != null)
            {
                return MissingTextHandler(ns, key, language, fallback);
            }
            return null;
        }

        protected virtual string DebugText(string ns, string key, LanguageInfo language, ParameterSet dict)
        {
            var text = GetTextEntry(ns, key, language, true);
            bool fallback = text != null && text.Text.Language != language.Key;

            var sb = new StringBuilder();
            sb.Append("Namespace: ").Append(ns).Append("\n")
                .Append("Key: ").Append(key).Append("\n")
                .Append("Language: ").Append(language.Key).Append("\n")
                .Append("Pattern: ");

            if (text != null)
            {
                sb.Append("" + text.Text.Pattern).Append("\n")
                .Append("Pattern Dialect: ").Append(text.Text.PatternDialect).Append("\n");

                if (fallback)
                {
                    sb.Append("Fallback to: ").Append(text.Text.Language).Append("\n");
                }
            }
            else
            {
                sb.Append("(undefined for language)\n");
            }

            sb.Append("Values = {\n");
            bool first = true;
            foreach (var dictKey in dict.Keys)
            {
                if (first) first = false; else sb.Append(", \n");
                sb.Append("    ").Append(dictKey).Append(": ").Append(dict[dictKey]);
            }
            sb.Append("\n}\n");

            return sb.ToString();
        }


        /// <summary>
        /// Parses all patterns. Regarding thread safety: An exception will occur if texts are changed while this method executes
        /// </summary>
        public void ParseAllPatterns()
        {
            //using (DisposableTimer.TraceDuration<TextManager>("Start ParseAllPatterns", "End ParseAllPatterns"))
            //{
                foreach (var keys in _textCache.Values)
                {
                    foreach (var translations in keys.Values)
                    {
                        foreach (var entry in translations.Values)
                        {
                            EnsureEvaluator(entry);
                        }
                    }
                }
            //}
        }

        protected void EnsureEvaluator(LocalizedTextCacheEntry entry)
        {
            EnsureLoaded();
            if (entry.Evaluator == null)
            {
                PatternDialect dialect;
                if (Dialects.TryGetValue(entry.Text.PatternDialect, out dialect))
                {
                    try
                    {
                        entry.PatternDialect = dialect;
                        entry.Evaluator = dialect.GetEvaluator(entry.Text.Pattern, this);
                    }
                    catch (PatternException pe)
                    {

                        var parameters = new
                        {
                            Message = pe.Message,
                            Namespace = entry.Text.Namespace,
                            Key = entry.Text.Key,
                            Language = entry.Text.Language
                        };

                        var ex = new PatternException("TextManager.PatternException",
                            "{0} while parsing {2} in namespace \"{1}\" for {2}", parameters, innerException: pe);


                        throw ex;
                    }
                }
                else
                {
                    throw new UnkownDialectException(entry.Text.PatternDialect);
                }
            }
        }


        public LocalizedTextCacheEntry GetTextEntry(string ns, string key, LanguageInfo language, bool considerLanguageFallbacks, bool considerNamespaceFallbacks = true)
        {
            EnsureLoaded();
            var entry = GetTextEntryInternal(ns, key, language, considerLanguageFallbacks);
            if (entry == null && FallbackNamespaces != null && considerNamespaceFallbacks)
            {
                foreach (var fallbackNs in FallbackNamespaces.Reverse())
                {
                    if ((entry = GetTextEntryInternal(fallbackNs, key, language, considerLanguageFallbacks)) != null)
                    {
                        break;
                    }
                }
            }
            return entry;
        }

        private LocalizedTextCacheEntry GetTextEntryInternal(string ns, string key, LanguageInfo language, bool considerLanguageFallbacks)
        {
            if (_textCache != null)
            {
                Dictionary<string, Dictionary<string, LocalizedTextCacheEntry>> keys;
                if (_textCache.TryGetValue(ns, out keys))
                {
                    Dictionary<string, LocalizedTextCacheEntry> translations;
                    if (keys.TryGetValue(key, out translations))
                    {
                        LocalizedTextCacheEntry text;
                        
                        if (translations.TryGetValue(language.Key, out text))
                        {
                            EnsureEvaluator(text);
                            return text;
                        }
                    }
                }
            }

            
            if (PrepareTextSources(ns))
            {
                return GetTextEntry(ns, key, language, considerLanguageFallbacks);
            }

            if (considerLanguageFallbacks)
            {
                var fallbacks = new List<LanguageInfo>();
                if (language.Fallbacks != null) fallbacks.AddRange(language.Fallbacks);
                if (FallbackLanguages != null) fallbacks.AddRange(FallbackLanguages.Reverse());

                LocalizedTextCacheEntry text = null;
                if (fallbacks.Any(x =>
                        (text = GetTextEntry(ns, key, x, false, false)) != null))
                {
                    return text;
                }
            }

            return null;
        }

        /// <summary>
        /// Implementing classes may override this method to add text sources on demand.        
        /// </summary>
        /// <param name="ns">The ns.</param>
        /// <returns>true if the namespace was loaded</returns>
        public virtual bool PrepareTextSources(string ns = null, string key = null, LanguageInfo language = null)
        {
            return false;
        }


        /// <summary>
        /// This method is called when the text source's texts have changed.        
        /// </summary>
        protected virtual void SourceTextsChanged(object sender, EventArgs args)
        {
            ReloadTexts();
        }


        public string TextHash { get; private set; }        

        protected virtual byte[] ComputeHash(IEnumerable<LocalizedText> texts)
        {
            using (var md5 = MD5.Create())
            {
                using (var s = new StreamWriter(new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write)))
                {
                    foreach( var text in texts )
                    {
                        s.Write(text.Key);
                        s.Write(text.Language);
                        s.Write(text.Pattern);
                        s.Write(text.PatternDialect);                        
                    }
                }

                return md5.Hash;
            }
        }

        public string HashToString(byte[] hash)
        {
            return Convert.ToBase64String(hash);
        }

        public virtual void ReloadTexts()
        {
            //using (DisposableTimer.TraceDuration<TextManager>("Start ReloadTexts", "End ReloadTexts"))
            //{
                var oldCache = _textCache;

                var newTexts = Texts.Get();
                TextHash = HashToString(ComputeHash(newTexts));

                var newCache = newTexts
                    .Where(text => text.Quality != TextQuality.PlaceHolder)
                    .Fold((text) => text,
                        (text) =>
                            new LocalizedTextCacheEntry { Text = text });

                if (oldCache != null)
                {
                    //Not too pretty... Find text entries in the old cache that equals those in the new and reuse them
                    foreach (var ns in oldCache)
                    {
                        Dictionary<string, Dictionary<string, LocalizedTextCacheEntry>> newNs;
                        if (newCache.TryGetValue(ns.Key, out newNs))
                        {
                            foreach (var key in ns.Value)
                            {
                                Dictionary<string, LocalizedTextCacheEntry> newKey;
                                if (newNs.TryGetValue(key.Key, out newKey))
                                {
                                    foreach (var lang in key.Value)
                                    {
                                        LocalizedTextCacheEntry newEntry;
                                        if (newKey.TryGetValue(lang.Key, out newEntry))
                                        {
                                            var oldEntry = lang.Value;
                                            if (oldEntry.Text.Equals(newEntry.Text))
                                            {
                                                //Reuse
                                                newKey[lang.Key] = lang.Value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                _textCache = newCache;
            //}
        }


        public bool TrackCallingAssembly { get; protected set; }


        public abstract LanguageInfo GetCurrentLanguage();

        public abstract TimeZoneInfo GetCurrentTimeZoneInfo();

        protected virtual bool IsInDebugMode()
        {
            return false;
        }



        /// <summary>
        /// Used to encode text generated from patterns. The content of formatting groups is not encoded
        /// </summary>
        public Func<string, string> StringEncoder { get; set; }




        /// <summary>
        /// Gets the current namespace given the calling assembly
        /// </summary>
        /// <param name="callingAssembly">If TrackCallingAssembly returns false this parameter is null</param>
        /// <returns></returns>
        public virtual string GetCurrentNamespace(Assembly callingAssembly)
        {
            return TrackCallingAssembly && callingAssembly != null ? GetNamespace(callingAssembly) : null;
        }

        /// <summary>
        /// Gets the namespace used for texts corresponding to the specified assembly
        /// </summary>   
        public virtual string GetNamespace(Assembly asm)
        {
            return null;
        }


        /// <summary>
        /// Gets the namespace used for texts corresponding to the assembly of the specified type
        /// </summary>        
        public virtual string GetNamespace<TInAssembly>()
        {
            return GetNamespace(typeof(TInAssembly).Assembly);
        }

        public class LocalizedTextCacheEntry
        {
            public LocalizedText Text;
            public PatternDialect PatternDialect { get; set; }
            public PatternEvaluator Evaluator;
        }
    }

    public delegate string MissingTextHandler(string ns, string key, LanguageInfo language, string fallback);
}