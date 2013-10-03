namespace Localization.Net.Processing
{
    public interface IParameterEvaluator
    {
        ParameterValue GetValue(EvaluationContext context);
    }
      
}
