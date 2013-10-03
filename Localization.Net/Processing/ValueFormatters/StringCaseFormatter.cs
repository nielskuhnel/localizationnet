using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Localization.Net.Processing.ValueFormatters
{
    /// <summary>
    /// Transforms the casing of strings
    /// </summary>
    public class StringCaseFormatter : IValueFormatter
    {
        public StringCaseTransformationType TransformationType { get; set; }

        public string FormatValue(ParameterValue value, EvaluationContext context)
        {
            var s = "" + value.Value;
            if (!string.IsNullOrEmpty(s))
            {
                switch (TransformationType)
                {
                    case StringCaseTransformationType.Lowercase: return s.ToLower(context.Language.Culture);
                    case StringCaseTransformationType.Uppercase: return s.ToUpper(context.Language.Culture);
                    case StringCaseTransformationType.CapitalizeFirst: return Capitalize(s, context.Language.Culture);
                    case StringCaseTransformationType.CapitalizeAll: 
                        return string.Join(" ", 
                            s.Split(' ').Select(w => Capitalize(w, context.Language.Culture)));
                }
            }

            return s;
        }

        string Capitalize(string s, CultureInfo culture)
        {
            return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0], culture) + s.Substring(1).ToLower(culture);            
        }
    }

    /// <summary>
    /// The possible case transformations for <see cref="StringCaseFormatter"/>
    /// </summary>
    public enum StringCaseTransformationType
    {
        Lowercase,
        Uppercase,
        CapitalizeFirst,
        CapitalizeAll
    }

    /// <summary>
    /// Factory class for <see cref="StringCaseFormatter"/>
    /// </summary>
    public class StringCaseFormatterFactory : IValueFormatterFactory
    {
        static Dictionary<string, StringCaseTransformationType> _transformationTypes = new Dictionary<string, StringCaseTransformationType>
        {
            {"lc", StringCaseTransformationType.Lowercase},
            {"lowercase", StringCaseTransformationType.Lowercase},
            {"uc", StringCaseTransformationType.Uppercase},
            {"uppercase", StringCaseTransformationType.Uppercase},
            {"cf", StringCaseTransformationType.CapitalizeFirst},
            {"capitalize-first", StringCaseTransformationType.CapitalizeFirst},
            {"ca", StringCaseTransformationType.CapitalizeAll},
            {"capitalize-all", StringCaseTransformationType.CapitalizeAll}
        };


        public IValueFormatter GetFor(string formatExpression, PatternDialect dialect, TextManager manager)
        {
            StringCaseTransformationType type;
            if (!string.IsNullOrEmpty(formatExpression) 
                && _transformationTypes.TryGetValue(formatExpression.ToLowerInvariant(), out type))
            {
                return new StringCaseFormatter { TransformationType = type };
            }

            return null;
        }
    }

}
