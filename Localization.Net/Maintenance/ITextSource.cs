using System;
using System.Collections.Generic;

namespace Localization.Net.Maintenance
{
    public interface ITextSource
    {    
        /// <summary>
        /// Gets the texts in the source
        /// </summary>
        /// <returns></returns>
        IEnumerable<LocalizedText> Get();


        /// <summary>
        /// Saves the specified texts with their state in the source with the specified merge options.
        /// </summary>
        /// <param name="texts">The texts.</param>
        /// <param name="options">The options.</param>
        void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options);


        /// <summary>
        /// Occurs when texts have changed
        /// </summary>
        event EventHandler TextsChanged;
    }

    public abstract  class TextSourceBase : ITextSource
    {        
        public abstract IEnumerable<LocalizedText> Get();

        public abstract void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options);

        public event EventHandler TextsChanged;

        protected virtual void OnTextsChanged()
        {
            if (TextsChanged != null)
            {
                TextsChanged(this, new EventArgs());
            }
        }
    }
    


    public enum TextMergeOptions
    {
        Replace,
        KeepUnused,
        DeleteUnused
    }

    public static class ITextSourceHelpers
    {        

        /// <summary>
        /// Folds the texts by namespace/key/language
        /// </summary>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, Dictionary<string, LocalizedText>>> Fold(this IEnumerable<LocalizedText> texts)
        {                       
            return texts.Fold(x => x, x=>x);
        }

        /// <summary>
        /// Folds the texts by namespace/key/language using the specified transformations
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="list"></param>
        /// <param name="mapper"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, Dictionary<string, TOut>>> Fold<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, LocalizedText> mapper, Func<TIn, TOut> converter)
        {
            var namespaces = new Dictionary<string, Dictionary<string, Dictionary<string, TOut>>>();
            foreach (var item in list)
            {
                var text = mapper(item);

                string ns = text.Namespace ?? "";

                Dictionary<string, Dictionary<string, TOut>> keys;
                if (!namespaces.TryGetValue(ns, out keys))
                {
                    namespaces.Add(ns, keys = new Dictionary<string, Dictionary<string, TOut>>());
                }

                Dictionary<string, TOut> translations;
                if (!keys.TryGetValue(text.Key, out translations))
                {
                    keys.Add(text.Key, translations = new Dictionary<string, TOut>());
                }

                translations.Add(text.Language, converter(item));
            }

            return namespaces;
        }
    }
}
