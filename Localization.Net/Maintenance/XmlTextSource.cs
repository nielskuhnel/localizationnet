using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Localization.Net.Maintenance
{
    public class XmlTextSource : TextSourceBase
    {

        private bool _readOnly = false;

        private XDocument _document;
        public XDocument Document
        {
            get { return _document; }
            set
            {
                bool changed = _document != value;
                _document = value;
                if (changed)
                {
                    OnTextsChanged();
                }
            }
        }
        
        public string DefaultNamespace { get; set; }

        /// <summary>
        /// Gets or sets the reference assembly for the texts ReferenceAssembly property.
        /// </summary>
        /// <value>
        /// The reference assembly.
        /// </value>
        public Assembly ReferenceAssembly { get; set; }


        /// <summary>
        /// If this is the file contains a single language. This means that the Text elements contains the pattern as a string as opposed to a list of Translation elements for each language
        /// The language is specified on the root element
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string SingleLanguage { get; set; }
        

        public XmlTextSource(XDocument document = null)
        {
            Document = document;
        }
        
        public static XmlTextSource ForAssembly(Assembly assembly, string name)
        {            
            foreach (var resName in assembly.GetManifestResourceNames())
            {                
                if (resName.EndsWith(name))
                {
                    using (var s = assembly.GetManifestResourceStream(resName))
                    {
                        var doc = XDocument.Load(s);                        
                        
                        return new XmlTextSource { Document = doc, _readOnly = true, ReferenceAssembly = assembly };
                    }
                }
            }

            return null;
        }
       

        /// <summary>
        /// Creates a <see cref="XmlTextSource"/> that monitors the specified path for changes.
        /// This is not allowed in medium trust
        /// </summary>        
        /// <typeparam name="TReferenceAssembly">The default assembly used to resolve resources.</typeparam>
        /// <param name="manager">The manager.</param>
        /// <param name="path">The path.</param>        
        public static XmlTextSource Monitoring<TReferenceAssembly>(TextManager manager, string path)
        {
            return Monitoring(typeof(TReferenceAssembly).Assembly, manager, path);
        }
        
        /// <summary>
        /// Creates a <see cref="XmlTextSource"/> that monitors the specified path for changes.
        /// </summary>        
        /// <param name="referenceAssembly">The default assembly used to resolve resources</param>
        /// <param name="manager">The manager.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static XmlTextSource Monitoring(Assembly referenceAssembly, TextManager manager, string path, int pollInterval = 1000)
        {
            var source = new XmlTextSource(XDocument.Load(path)) { ReferenceAssembly = referenceAssembly };
            source.DefaultNamespace = manager.DefaultNamespace;

            //Note: FileSystemWatcher doesn't work in medium trust       

            var lastWriteTime = File.GetLastWriteTimeUtc(path);
            source._timer = new Timer((state) =>
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        source.Document = null;
                    }
                    else
                    {
                        var writeTime = File.GetLastWriteTimeUtc(path);
                        if (writeTime > lastWriteTime)
                        {
                            source.Document = XDocument.Load(path);
                            lastWriteTime = writeTime;
                        }
                    }
                }
                catch { /*We don't want to crash the server. */  }
            }, null, pollInterval, pollInterval);            

            return source;
        }

        //TODO: Actually, the monitoring text source should have it's own subclass. This is a little dirty, but the source needs a reference to the timer so that the timer's finalizer will eventually be called if the source is no longer used
        private Timer _timer;


        public override IEnumerable<LocalizedText> Get()
        {
            if (Document == null || Document.Root == null)
            {
                yield break;
            }

            var root = Document.Root;

            SingleLanguage = (string)root.Attribute("Language");
            DefaultNamespace = (string)root.Attribute("Namespace") ?? DefaultNamespace;

            string defaultDialect = (string)root.Attribute("PatternDialect");
            if (string.IsNullOrEmpty(defaultDialect)) defaultDialect = "Default";

            foreach (var text in GetInternal(root.Elements("Text"), DefaultNamespace, defaultDialect))
            {
                yield return text;
            }

            foreach (var ns in root.Elements("Namespace"))
            {
                foreach (var text in GetInternal(ns.Elements("Text"), (string) ns.Attribute("Name"), defaultDialect))
                {
                    yield return text;
                }
            }                       
        }

        IEnumerable<LocalizedText> GetInternal(IEnumerable<XElement> texts, string ns, string defaultDialect)
        {
            foreach (var el in texts)
            {
                IEnumerable<XElement> translations;
                if (!string.IsNullOrEmpty(SingleLanguage))
                {
                    //Read the pattern attributes from the Text element
                    translations = new[] { el };
                }
                else
                {
                    translations = el.Elements("Translation");
                }

                foreach (var trans in translations)
                {
                    var languages = ((string)trans.Attribute("Language") ?? SingleLanguage).Split(',')
                        .Select(x=>x.Trim());

                    foreach (var lang in languages)
                    {
                        if ((string)el.Attribute("Key") == "MultiLanguage")
                        {
                            bool b = false;
                        }
                        var text = new LocalizedText
                        {
                            Key = (string)el.Attribute("Key"),
                            Language = lang,
                            Pattern = trans.Value,
                            PatternDialect = (string)trans.Attribute("PatternDialect"),
                            Source = new TextSourceInfo
                            {
                                TextSource = this,
                                ReferenceAssembly = ReferenceAssembly,
                                Details = (string)trans.Attribute("Source")
                            },
                            Namespace = (string)el.Attribute("Namespace")
                        };


                        if (string.IsNullOrEmpty(text.Namespace)) text.Namespace = ns;
                        if (string.IsNullOrEmpty(text.PatternDialect)) text.PatternDialect = defaultDialect;

                        var quality = (string)trans.Attribute("Quality");
                        if (!string.IsNullOrEmpty(quality))
                        {
                            text.Quality = (TextQuality)Enum.Parse(typeof(TextQuality), quality);
                        }

                        yield return text;
                    }
                }
            }
        }

        public override void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options)
        {
            //TODO: Implement TextMergeOptions

            if (_readOnly)
            {
                throw new NotSupportedException("This text source does not support saving texts");
            }

            if (Document == null || Document.Root == null)
            {
                Document = new XDocument(new XElement("Localizations"));
            }

            var root = Document.Root;

            if (!string.IsNullOrEmpty(SingleLanguage))
            {
                texts = texts.Where(x => x.Text.Language == SingleLanguage);
                root.SetAttributeValue("Language", SingleLanguage);
            }

            string defaultDialect = (string)root.Attribute("PatternDialect");
            if (string.IsNullOrEmpty(defaultDialect)) defaultDialect = "Default";


            root.Elements().Remove();
            foreach (var ns in texts.Fold((x)=>x.Text, (x)=>x))
            {
                var parent = root;
                if (!string.IsNullOrEmpty(ns.Key) && ns.Key != DefaultNamespace)
                {
                    parent = new XElement("Namespace", new XAttribute("Name", ns.Key));
                    root.Add(parent);
                }

                foreach (var key in ns.Value)
                {

                    var textElement = new XElement("Text");
                    textElement.Add(new XAttribute("Key", key.Key));

                    foreach (var state in key.Value.Values)
                    {
                        if ((state.Status & LocalizedTextStatus.Unused) == 0 || options == TextMergeOptions.KeepUnused)
                        {
                            var trans = state.Text;
                           
                            var translationElement = string.IsNullOrEmpty(SingleLanguage) ? new XElement("Translation") : textElement;

                            translationElement.Add(new XAttribute("Language", trans.Language));
                            if (trans.PatternDialect != defaultDialect)
                            {
                                translationElement.Add(new XAttribute("PatternDialect", trans.PatternDialect));
                            }
                            
                            if( trans.Source != null && !string.IsNullOrEmpty(trans.Source.Details) ) {
                                translationElement.Add(new XAttribute("Source", trans.Source));
                            }

                            if (trans.Quality != TextQuality.Proper)
                            {
                                translationElement.Add(
                                    new XAttribute("Quality", Enum.GetName(typeof(TextQuality), trans.Quality)));
                            }

                            translationElement.Value = trans.Pattern;
                            
                            if (textElement != translationElement)
                            {
                                textElement.Add(translationElement);
                            }
                        }
                    }

                    parent.Add(textElement);
                }
            }
            OnTextsChanged();
        }
    }

    public class XmlTextSource<TReferenceAssembly> : XmlTextSource
    {
        public XmlTextSource(XDocument document = null)
            : base(document)
        {
            ReferenceAssembly = typeof(TReferenceAssembly).Assembly;
        }
    }
}
