using System;
using System.Reflection;
using Localization.Net.Exceptions;
using Localization.Net.Configuration;

namespace Localization.Net
{
    /// <summary>
    /// Provides extension methods to localize strings
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Localizes the string.
        /// If a key is specified the string the method is called on is considered the default value. Otherwise; the key
        /// </summary>
        /// <param name="s">The string with the default value.</param>
        /// <param name="typeRef">An object reference to a class in the namespace to get texts from.</param>
        /// <param name="parameters">The parameters to the text.</param>
        /// <param name="key">The key for text. If this is specified the string the method is called on is considered the default value. Otherwise; the key</param>
        /// <param name="language">The language. Default is current language</param>
        /// <param name="ns">The namespace. Specify to override the namespace from the type</param>
        /// <param name="debug">Show debug output as configured in the current text manager.</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>
        /// A localized text using the current text manager
        /// </returns>
        public static string Localize(this string s, object typeRef = null, object parameters = null, string key = null, 
            LanguageInfo language = null, string ns = null, bool? debug = null, bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            Assembly asm = null;
            if (typeRef != null)
            {
                if (typeRef is string)
                {
                    throw new LocalizedArgumentException("StringHelpers.TypeRefIsString");
                }
                asm = typeRef.GetType().Assembly;
            }
            
            return Localize(s, asm, parameters, key, language, ns, debug,
                            returnNullOnMissing, encode, fallback);
        }

        /// <summary>
        /// Localizes the string.
        /// If a key is specified the string the method is called on is considered the default value. Otherwise; the key
        /// </summary>
        /// <typeparam name="TAssemblyRef">The type used to resolve the text namespace</typeparam>
        /// <param name="s">The string with the default value.</param>
        /// <param name="parameters">The parameters to the text.</param>
        /// <param name="key">The key for text. If this is specified the string the method is called on is considered the default value. Otherwise; the key</param>
        /// <param name="language">The language. Default is current language</param>
        /// <param name="ns">The namespace. Specify to override the namespace from the type</param>
        /// <param name="debug">Show debug output as configured in the current text manager.</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>
        /// A localized text using the current text manager
        /// </returns>
        public static string Localize<TAssemblyRef>(this string s, object parameters = null, string key = null, 
            LanguageInfo language = null, string ns = null, bool? debug = null, bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            return Localize(s, typeof(TAssemblyRef).Assembly, parameters, key, language, ns, debug,
                            returnNullOnMissing, encode, fallback);
        }

        /// <summary>
        /// Localizes the string.
        /// If a key is specified the string the method is called on is considered the default value. Otherwise; the key
        /// </summary>
        /// <param name="s">The string with the default value.</param>
        /// <param name="type">The type used to resolve the text namespace</param>
        /// <param name="parameters">The parameters to the text.</param>
        /// <param name="key">The key for text. If this is specified the string the method is called on is considered the default value. Otherwise; the key</param>
        /// <param name="language">The language. Default is current language</param>
        /// <param name="ns">The namespace. Specify to override the namespace from the type</param>
        /// <param name="debug">Show debug output as configured in the current text manager.</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>
        /// A localized text using the current text manager
        /// </returns>
        public static string Localize(this string s, Type type, object parameters = null, string key = null, 
            LanguageInfo language = null, string ns = null, bool? debug = null, bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            return Localize(s, type.Assembly, parameters, key, language, ns, debug,
                returnNullOnMissing, encode, fallback);
        }

        /// <summary>
        /// Localizes the string.
        /// If a key is specified the string the method is called on is considered the default value. Otherwise; the key
        /// </summary>
        /// <param name="s">The string with the default value.</param>
        /// <param name="nsAssembly">The assembly used to resolve the text namespace</param>
        /// <param name="parameters">The parameters to the text.</param>
        /// <param name="key">The key for text. If this is specified the string the method is called on is considered the default value. Otherwise; the key</param>
        /// <param name="language">The language. Default is current language</param>
        /// <param name="ns">The namespace. Specify to override the namespace from the type</param>
        /// <param name="debug">Show debug output as configured in the current text manager.</param>
        /// <param name="returnNullOnMissing">if set to <c>true</c> null is returned on missing texts. This has no effect if no key is specified as the string is the default text</param>
        /// <param name="encode">if set to <c>true</c> the text is encoded using the current text managers encoder.</param>
        /// <param name="fallback">A fallback value if no localized value can be found.</param>
        /// <returns>
        /// A localized text using the current text manager
        /// </returns>
        public static string Localize(this string s, Assembly nsAssembly, object parameters = null, string key = null, 
            LanguageInfo language = null, string ns = null, bool? debug = null, bool returnNullOnMissing = false, bool encode = true,
            string fallback = null)
        {
            bool stringIsDefault = false;
            if( key != null )
            {
                stringIsDefault = true;                
                returnNullOnMissing = true;
            } else
            {
                key = s;
            }
            
            var text = LocalizationConfig.TextManager.Get(key, parameters, language, ns, nsAssembly, debug,
                                                      returnNullOnMissing, encode, fallback);
            if( stringIsDefault && text == null )
            {
                return s;
            }

            return text;
        }

    }
}
