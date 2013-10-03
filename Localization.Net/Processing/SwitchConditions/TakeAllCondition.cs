using System;

namespace Localization.Net.Processing.SwitchConditions
{
    /// <summary>
    /// This evaluator always returns true
    /// </summary>
    public class TakeAllCondition : ISwitchConditionEvaluator
    {
        public bool Evaluate(ParameterValue val, EvaluationContext context)
        {
            return true;
        }
    }



    public class TakeAllConditionFactory : StringBasedSwitchConditionEvaluatorFactory
    {
        public override ISwitchConditionEvaluator GetFor(string spelling, PatternDialect dialect, TextManager manager)
        {
            if (string.IsNullOrEmpty(spelling) || spelling.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return new TakeAllCondition();
            }

            return null;
        }
    }

}
