using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Reflection;
using System.Globalization;
using Localization.Net.Maintenance;
using Localization.Net.Parsing;

namespace Localization.Net
{
    /// <summary>
    /// A default implementation of TextManager where the current language is given by the evaluation of the delegate
    /// </summary>
    public class DefaultTextManager : TextManager
    {
        /// <summary>
        /// This delegate is used to resolve the current language;
        /// </summary>
        public Func<LanguageInfo> CurrentLanguage;

        public Func<TimeZoneInfo> CurrentTimeZone { get; set; }
        

        public Func<Assembly, string, ITextSource> NamespaceTextResolver { get; set; }

        /// <summary>
        /// The function used to write debug information about texts. If null default behavior is used.
        /// Arguments: Namespace, Key, Language, Mathing text (if any), the parameters for the text
        /// </summary>
        public Func<string, string, LanguageInfo, LocalizedTextCacheEntry, ParameterSet, string> DebugFormatter { get; set; }

        /// <summary>
        /// If this is specified and returns true debug information about the texts will be returned. (See DebugFunction).
        /// </summary>
        public Func<bool> DebugMode;

        private Dictionary<CultureInfo, LanguageInfo> _languageInfoCache = new Dictionary<CultureInfo,LanguageInfo>();

        public DefaultTextManager()
            : base()
        {
            CurrentLanguage = () =>
            {                
                LanguageInfo languageInfo;
                var culture = CultureInfo.CurrentUICulture;
                if (!_languageInfoCache.TryGetValue(culture, out languageInfo))
                {                    
                    _languageInfoCache.Add(culture, languageInfo = (LanguageInfo)culture);
                }
                return languageInfo;
            };

            CurrentTimeZone = () => TimeZoneInfo.Local;
            


            TrackCallingAssembly = true;

            //Build-in texts            
            FallbackNamespaces.Add(GetNamespace<DefaultTextManager>());

            //Empty namespace
            FallbackNamespaces.Add("");


            // This is used to match texts that all languages should fall back to
            FallbackLanguages = new List<LanguageInfo> { new LanguageInfo { Key = "*" } };

            NamespaceTextResolver = (asm, ns) =>
            {
                return AssemblySourceLoader.GetTextSource(asm, this, ns);                
            };
        }     
                     
        public override string GetNamespace(Assembly asm)
        {
            try
            {
                return asm.GetName().Name;
            }
            catch (SecurityException mediumTrustException)
            {
                return asm.FullName;
            }
        }


        /// <summary>
        /// If the calling assemblies' name are used as namespace for keys
        /// </summary>
        public bool UseNamespaces { get; private set; }
        
        
        public Dictionary<string, LanguageInfo> Languages { get; set; }


        /// <summary>
        /// Enables the Html tags in patterns using the default dialect. 
        /// It will change the syntax for format groups from "&lt;Group: ... &gt;" to "&lt;!Group: ... &gt;"
        /// </summary>
        /// <returns></returns>
        public DefaultTextManager EnableHtmlPatterns()
        {
            Dialects["Default"].PatternTransformer = new HtmlPatternTransformer();
            return this;
        }


        protected override bool IsInDebugMode()
        {
            return DebugMode != null ? DebugMode() : base.IsInDebugMode();
        }

        protected override string DebugText(string ns, string key, LanguageInfo language, ParameterSet dict)
        {
            if (DebugFormatter != null)
            {
                var text = GetTextEntry(ns, key, language, true);
                return DebugFormatter(ns, key, language, text, dict);
            }

            return base.DebugText(ns, key, language, dict);
        }


        /// <summary>
        /// Only search for assemblies once.
        /// </summary>
        private HashSet<string> probedNamespaces = new HashSet<string>();

        public override bool PrepareTextSources(string ns = null, string key = null, LanguageInfo language = null)
        {                        
            if (ns!= null && NamespaceTextResolver != null)
            {
                lock (this)
                {
                    if (probedNamespaces.Contains(ns))
                    {
                        return false;
                    }                    

                    var asm = TypeFinder.GetFilteredLocalAssemblies(exclusionFilter:KnownAssemblyExclusionFilter).FirstOrDefault(x => {
                        try
                        {
                            //This will fail for certain assemblies in medium trust (e.g. mscorlib)
                            return GetNamespace(x) == ns;
                        }
                        catch
                        {                           
                            return false;
                        }
                    });

                    if (asm != null)
                    {
                        return PrepareTextSources(asm);
                    }
                    else
                    {
                        probedNamespaces.Add(ns);
                    }
                }
            }

            return false;
        }

        public void PrepareAssemblyTextSources()
        {
            foreach (var asm in TypeFinder.GetFilteredLocalAssemblies(exclusionFilter: KnownAssemblyExclusionFilter))
            {
                PrepareTextSources(asm);
            }
        }

        protected override void EnsureLoaded()
        {
            if (!_assemblyTextSourcesInitialized)
            {
                _assemblyTextSourcesInitialized = true;
                PrepareAssemblyTextSources();
            }
        }

        private bool _assemblyTextSourcesInitialized = false;

        /// <summary>
        /// Prepares the text sources from the specified assembly.
        /// </summary>
        /// <param name="asm">The assembly.</param>
        /// <returns></returns>
        public bool PrepareTextSources(Assembly asm )
        {
            var ns = GetNamespace(asm);
            if (probedNamespaces.Contains(ns))
            {
                return false;
            }

            try
            {

                var source = NamespaceTextResolver(asm, ns);
                if (source != null)
                {

                    Texts.Sources.Add(new PrioritizedTextSource(source));
                }

                return true;

            }
            catch
            {
                //Medium trust exception for the assembly. Eat it
                return false;
            }
            finally
            {

                probedNamespaces.Add(ns);
            }
        }

        protected override void SourceTextsChanged(object sender, EventArgs args)
        {         
            base.SourceTextsChanged(sender, args);            
        }


        public override LanguageInfo GetCurrentLanguage()
        {
            return CurrentLanguage();
        }

        public override TimeZoneInfo GetCurrentTimeZoneInfo()
        {
            return CurrentTimeZone();
        }

    }    
}