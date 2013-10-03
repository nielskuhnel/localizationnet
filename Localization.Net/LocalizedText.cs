using System.Reflection;
using Localization.Net.Maintenance;

namespace Localization.Net
{
    /// <summary>
    /// Represent the pattern used to expand the translation for the text with the specified key in the specified language
    /// </summary>
    public class LocalizedText
    {
        /// <summary>
        /// The unique key for the localized text not including language
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The language in which the pattern generates a translation for the key
        /// </summary>
        public string Language { get; set; }


        private string _namespace;
        /// <summary>
        /// Used to distinguish texts with the same key in different contexts
        /// </summary>
        public string Namespace { get { return _namespace ?? ""; } set { _namespace = value; } }

        /// <summary>
        /// The pattern that generates the translation for the key
        /// </summary>
        public string Pattern { get; set; }
        
        /// <summary>
        /// The ID of the dialect of the parser for the pattern. Default is "Default"
        /// </summary>
        public string PatternDialect { get; set; }

        /// <summary>
        /// This may be used by the text manager to provide additional information. Not required
        /// </summary>
        public TextSourceInfo Source { get; set; }        
                

        /// <summary>
        /// If the text is a place holder or the translator was unsure about its meaning this can be stated here. Default is "Proper"
        /// </summary>        
        public TextQuality Quality { get; set; }


        /// <summary>
        /// This may contain hints for translators
        /// </summary>
        public string Description { get; set; }

        
        public string UniqueKey
        {
            get { return Namespace + "__" + Key + "_" + Language; }
        }

        public LocalizedText()
        {            
            Quality = TextQuality.Proper;
            PatternDialect = "Default";            
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Key.GetHashCode() ^
                PatternDialect.GetHashCode() ^
                Pattern.GetHashCode() ^
                Namespace.GetHashCode() ^
                Language.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// Two <see cref="LocalizedText"/>s are equal if namespace, key, language, pattern and pattern dialect are equal
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as LocalizedText;
            if (other != null)
            {
                return other.Key == this.Key &&
                    other.PatternDialect == this.PatternDialect &&
                    other.Pattern == this.Pattern &&
                    other.Namespace == this.Namespace &&
                    other.Language == this.Language;
            }
            return base.Equals(obj);
        }
    }

    public class TextSourceInfo
    {
        /// <summary>
        /// Gets or sets the text source that loaded the text.
        /// </summary>
        /// <value>
        /// The text source.
        /// </value>
        public ITextSource TextSource { get; set; }

        /// <summary>
        /// Gets or sets the assembly in which's context the text was loaded. 
        /// This is the default assembly when resolving resources and my be different than the one given by the namespace
        /// </summary>
        /// <value>
        /// The reference assembly.
        /// </value>
        public Assembly ReferenceAssembly { get; set; }


        /// <summary>
        /// Gets or sets the source specific details about the text source.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details { get; set; }
    }

    public enum TextQuality : int
    {
        /// <summary>
        /// The text is just defined "to be there" and not considered by the text manager
        /// </summary>
        PlaceHolder = 0,

        /// <summary>
        /// The text is based auto-generated based deterministic rules like machine translation
        /// </summary>
        Fuzzy = 1,

        /// <summary>
        /// The text is a verified translation of the text for the key in the language specified
        /// </summary>
        Proper = 2,                
    }
}
