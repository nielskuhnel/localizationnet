using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Localization.Net.Exceptions;

namespace Localization.Net
{
    /// <summary>
    /// Represent a "language" that is used for translating texts
    /// </summary>
    public class LanguageInfo
    {
        /// <summary>
        /// The key used for the language in text sources
        /// </summary>
        public virtual string Key { get; set; }

        private CultureInfo _culture;

        /// <summary>
        /// The format provider used to format values 
        /// </summary>        
        public CultureInfo Culture
        {
            get
            {
                if (_culture == null)
                {
                    InferCultureFromKey();
                }
                return _culture;
            }
            protected set { _culture = value; }
        }




        /// <summary>
        /// If a text is not available in this language these fallbacks are considered
        /// </summary>
        public List<LanguageInfo> Fallbacks { get; set; }

        public void InferCultureFromKey()
        {
            Culture = GetCultureFromKey(Key);
        }

        public static CultureInfo GetCultureFromKey(string key)
        {
            var parts = key.Split('-');
            for (int i = parts.Length; i > 0; i--)           
            {
                string culture = string.Join("-", parts.Take(i).ToArray());
                try
                {
                    var foundCulture = CultureInfo.GetCultureInfo(culture);
                    return foundCulture;
                }
                catch { } //Eat it. We couldn't get a CultureInfo from the language code, but then, CurrentCulture.Current isn't that bad.
            }

            throw new LocalizedInvalidOperationException("Exception.InferCultureInfoException", 
                "Unable to infer CultureInfo from key {0}", new { Key = key});
        }


        public override int GetHashCode()
        {
            return Key != null ? Key.GetHashCode() : string.Empty.GetHashCode();
        }
        

        public override bool Equals(object obj)
        {            
            var other = obj as LanguageInfo;
            if (other != null && other.Key == this.Key)
            {
                return true;
            }

            return false;
        }

        public static implicit operator LanguageInfo(CultureInfo culture)
        {
            return new LanguageInfo
            {
                Key = culture.Name,
                Name = culture.DisplayName,
                Culture = culture
            };
        }

        #region Implementation of IReferenceByName

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByName.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public virtual string Alias
        {
            get { return Key; }
            set { Key = value; }
        }

        /// <summary>
        /// The description/name of the language (e.g. "Danish (not the pastry)")
        /// </summary>
        public virtual LocalizedString Name { get; set; }

        #endregion
    }
}
