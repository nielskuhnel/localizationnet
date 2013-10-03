using System.Collections.Generic;
using Localization.Net.Parsing;
using Localization.Net.Processing.ParameterEvaluators;
using Localization.Net.Processing.SwitchConditions;
using Localization.Net.Processing.ValueFormatters;

namespace Localization.Net.Processing
{
    public class DefaultDialect : PatternDialect
    {
        public DefaultDialect()
        {
            Parser = new DefaultExpressionParser();

            ParameterEvaluators = new List<IParameterEvaluatorFactory> {
                new PatternLookupEvaluatorFactory(),
                new ReflectionParameterEvaluatorFactory(),
                new SimpleParameterEvaluatorFactory()
            };

            ValueFormatters = new List<IValueFormatterFactory> {
                new StringCaseFormatterFactory(),
                new RomanNumberFormatterFactory(),
                new StringFormatFormatterFactory(),
                new DefaultFormatterFactory()
            };

            SwitchConditionEvaluators = new List<ISwitchConditionEvaluatorFactory> {
                new TakeAllConditionFactory(), 
                new BooleanExpressionConditionFactory(), 
                new LookupConditionFactory(),
                new ArithmeticConditionFactory(),                
                new IntervalConditionFactory(),                 
                new ValueListConditionFactory()            
            };

            FormatGroupExpander = new HashTagFormatGroupExpander();            
        }


        private object _parseLock = new object();

        public override PatternEvaluator GetEvaluator(string pattern, TextManager manager)
        {
            lock (_parseLock)
            {
                if (PatternTransformer != null)
                {
                    pattern = PatternTransformer.Encode(pattern);
                }

                var expr = Parser.Parse(pattern, manager);

                //Bind parameter evaluators, value formatters etc.
                expr.Accept(new PatternDecorator(manager, this));

                //Convert switches with two or three condition less cases to default enum construction and apply switch templates
                expr.Accept(new SwitchDecorator(manager, this));

                return new PatternEvaluator(expr) {PatternTransformer = PatternTransformer};
            }
        }

    }    
}
