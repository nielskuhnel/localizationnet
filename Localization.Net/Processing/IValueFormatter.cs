namespace Localization.Net.Processing
{
   
    public interface IValueFormatter
    {
        string FormatValue(ParameterValue value, EvaluationContext context);
    }
      
}
