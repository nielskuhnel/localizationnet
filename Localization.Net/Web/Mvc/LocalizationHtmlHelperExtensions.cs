using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Localization.Net.Processing;
using Localization.Net.Processing.ParameterValues;

namespace Localization.Net.Web.Mvc
{
    public static class LocalizationHtmlHelperExtensions
    {

        public static MvcHtmlString GetText(this HtmlHelper htmlHelper, string key, object values = null,
            LanguageInfo language = null, string ns = null, Type type = null, string @default = null)
        {                        
            var manager = LocalizationHelper.TextManager;

            if (type != null) ns = manager.GetNamespace(type.Assembly);
            
            var text = manager.Get(key, values, ns: ns, language: language, returnNullOnMissing: @default != null);            
            if( text == null )
            {
                text = HttpUtility.HtmlEncode(@default);
            }
            return new MvcHtmlString(text);
        }


        public static FormatWrapper<T> Wrap<T>(this T value, string formatExpression)
        {
            return new FormatWrapper<T>(value, formatExpression);
        }

        public static DelegateFormatWrapper<TValue, TReference> Wrap<TValue, TReference>(this TReference reference, TValue value, Func<DelegateValueFormatArgs<TValue, TReference>, object> format)
        {
            return new DelegateFormatWrapper<TValue, TReference>(value, reference, format);
        }

        public static DelegateFormatWrapper<TValue, TValue> Wrap<TValue>(this TValue value, Func<DelegateValueFormatArgs<TValue, TValue>, object> format)
        {
            return value.Wrap(value, format);
        }        


        public static IEnumerable<FormatWrapper<T>> WrapList<T>(this IEnumerable<T> value, string formatExpression)
        {
            foreach (var val in value)
            {
                yield return new FormatWrapper<T>(val, formatExpression);
            }
        }

        public static IEnumerable<DelegateFormatWrapper<object, TValue>> WrapList<TValue>(this IEnumerable<TValue> value,                
                Func<DelegateValueFormatArgs<object, TValue>, object> format)
        {
            return value.WrapList(null, format);
        }
        
        /// <summary>
        /// Wraps the elements in the list with the specified format.
        /// Not that the value selector is a string expression as anonymous types doesn't support delegates and that how this method is supposed to be used
        /// </summary>
        /// <typeparam name="TReference">The type of the reference.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="valueSelectorExpression">The value selector expression. If null the list item is used</param>
        /// <returns>A list of <see cref="DelegateFormatWrapper" />s for each element in the list</returns>
        public static IEnumerable<DelegateFormatWrapper<object, TReference>> WrapList<TReference>(this IEnumerable<TReference> value,                             
                string valueSelectorExpression,
                Func<DelegateValueFormatArgs<object, TReference>, object> format)
        {
            foreach (var val in value)
            {
                yield return new DelegateFormatWrapper<object, TReference>(
                    valueSelectorExpression != null ? DataBinder.Eval(val, valueSelectorExpression) : (object)val,
                    val,
                    format);
            }
        }

        
        public static ParameterValue WithDefaultFormat<TValue>(this TValue value, string format)
        {
            var manager = LocalizationHelper.TextManager;
            IValueFormatter defaultFormatter = null;
            foreach (var dialect in manager.Dialects)
            {
                try
                {
                    defaultFormatter = dialect.Value.GetValueFormatter(format, manager);
                }
                catch { }
            }
            if (defaultFormatter == null)
            {
                throw new NullReferenceException("No format found for \"" + format);
            }

            var pv = ParameterValue.Wrap(value);
            pv.DefaultFormat = defaultFormatter;

            return pv;
        }


        public static UnencodedParameterValue Unencoded(this object value)
        {
            return new UnencodedParameterValue(value);
        }
    }
}
