using Localization.Net.Exceptions;
using Localization.Net.Parsing;
using Localization.Net.Processing.ParameterEvaluators;

namespace Localization.Net.Processing.SwitchConditions
{
    //IDEA: This could be generalized to a "late bound" condition that fills in parameters before evaluation

    /// <summary>    
    /// Plural forms can be rather complex (see http://www.gnu.org/software/hello/manual/gettext/Plural-forms.html)
    /// This condition evaluator allows to reuse rules by giving them a label. They should be stored in the manager with the "Text" dialect
    /// </summary>
    public class LookupCondition : ISwitchConditionEvaluator
    {

        public PatternDialect Dialect { get; set; }

        public PatternLookupEvaluator Evaluator { get; set; }

        public bool Evaluate(ParameterValue o, EvaluationContext context)
        {
            var pattern = (string) Evaluator.GetValue(context).Value;

            if (!string.IsNullOrEmpty(pattern))
            {
                return Dialect.GetSwitchConditionEvaluator(Expression.Text(pattern), Evaluator.Manager).Evaluate(o, context);
            }
            else
            {
                throw new LocalizedKeyNotFoundException(
                    "Exceptions.LookupConditionParameterNotResolved",
                    "The condition '{0}' could not be resolved",
                    new { Key = Evaluator.PatternKey });
            }            
        }
    }

    public class LookupConditionFactory : StringBasedSwitchConditionEvaluatorFactory
    {
        public override ISwitchConditionEvaluator GetFor(string spelling, PatternDialect dialect, TextManager manager)
        {
            if (spelling.StartsWith("@"))
            {
                var evaluator = dialect.GetParameterEvaluator(
                    new ParameterSpec { ParameterName = spelling }, manager) as PatternLookupEvaluator;
                if (evaluator != null)
                {
                    return new LookupCondition
                    {
                        Dialect = dialect,
                        Evaluator = evaluator
                    };
                }
            }

            return null;
        }        
    }

}
