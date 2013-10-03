namespace Localization.Net.Processing.ValueFormatters
{
    public class StringFormatFormatter : IValueFormatter
    {
        public string FormatExpression { get; private set; }

        public StringFormatFormatter(string formatExpression)
        {
            FormatExpression = formatExpression;
        }

        public string FormatValue(ParameterValue value, EvaluationContext context)
        {
            return string.Format(context.Language.Culture, "{0:" + FormatExpression + "}", value.Value);
        }
    }

    public class StringFormatFormatterFactory : IValueFormatterFactory
    {

        public IValueFormatter GetFor(string formatExpression, PatternDialect dialect, TextManager manager)
        {
            if (!string.IsNullOrEmpty(formatExpression))
            {
                return new StringFormatFormatter(formatExpression);
            }
            return null;
        }
    }

}
