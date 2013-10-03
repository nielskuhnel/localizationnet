using System;
using Localization.Net.Parsing;
using Localization.Net.Processing.SwitchConditions;

namespace Localization.Net.Web.JavaScript.SwitchConditions
{
    public class LookupGenerator : PatternProcessorGenerator<LookupCondition>
    {        

        public override void WriteEvaluator(LookupCondition proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            //NOTE: This will not work if parameters are involved.
            var pattern = (string)proc.Evaluator.GetValue(writer.BaseContext).Value;
            if (!string.IsNullOrEmpty(pattern))
            {
                var condition =  proc.Dialect.GetSwitchConditionEvaluator(Expression.Text(pattern), proc.Evaluator.Manager);
                var conditionWriter = writer.Writers[condition.GetType()];

                conditionWriter.WriteEvaluator(condition, writer, argumentWriters[0]);
            }
            else
            {
                //TODO: Inform the user that this key doesn't exist in a better way
                writer.Output.Write(writer.Json.Serialize(false));
            }

        }
    }
}
