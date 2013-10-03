using Localization.Net.Parsing;

namespace Localization.Net.Processing.ParameterEvaluators
{
    public class SimpleParameterEvaluator : IParameterEvaluator
    {
        public string ParameterName { get; private set; }

        public SimpleParameterEvaluator(string parameterName)
        {
            ParameterName = parameterName;
        }

        public ParameterValue GetValue(EvaluationContext context)
        {
            return context.Parameters[ParameterName];
        }
    }

    public class SimpleParameterEvaluatorFactory : IParameterEvaluatorFactory
    {

        public IParameterEvaluator GetFor(ParameterSpec spelling, PatternDialect dialect, TextManager manager)
        {
            return new SimpleParameterEvaluator(spelling.ParameterName);
        }
    }
}
