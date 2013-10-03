namespace Localization.Net.Processing.ValueFormatters
{
    public class DefaultFormatter : IValueFormatter
    {
        public string FormatValue(ParameterValue value, EvaluationContext context)
        {
            if (value.DefaultFormat != null)
            {
                return value.DefaultFormat.FormatValue(value, context);
            }
            else
            {
                return string.Format(context.Language.Culture, "{0}", value.Value);
            }
        }
    }


    public class DefaultFormatterFactory : IValueFormatterFactory
    {
        public IValueFormatter GetFor(string rep, PatternDialect dialect, TextManager manager)
        {
            return new DefaultFormatter();
        }
    }

}
