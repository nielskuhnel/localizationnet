using System;

namespace Localization.Net.Processing.ParameterValues
{
    /// <summary>
    /// Provides a format to the value in the <see cref="PatternEvaluator"/> after the actual value has passed its <see cref="IValueFormatter"/>.
    /// This can be used for wrapping format around values without using the format group construct.
    /// </summary>
    public class FormatWrapper<TValue> : ParameterValue<TValue>
    {
        public string FormatExpression { get; set; }

        /// <summary>
        /// Creates a new FormatWrapper for the value specified using the formatExpression
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="formatExpression">The format expression. Wraps format around the value. {#} will be replaced with the actual value</param>
        public FormatWrapper(TValue value, string formatExpression)
            :base(value)
        {
            FormatExpression = formatExpression;            
        }

        public override string Format(Func<string, string> stringEncoder, string formattedValue)
        {            
            return FormatExpression.Replace("{#}", stringEncoder(formattedValue));
        }

        public static implicit operator TValue(FormatWrapper<TValue> wrapper)
        {
            return (TValue)wrapper.Value;
        }
    }

    public class FormatWrapper : FormatWrapper<object> {

        public FormatWrapper(object value, string formatExpression)
            : base(value, formatExpression)
        {
        }
    }
}
