using System;
using Localization.Net.Processing.ParameterEvaluators;

namespace Localization.Net.Web.JavaScript.ParameterEvaluators
{
    public class SimpleParameterGenerator : PatternProcessorGenerator<SimpleParameterEvaluator>
    {        

        public override void WriteEvaluator(SimpleParameterEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            WriteGetParameter(writer, writer.Json.Serialize(proc.ParameterName));            
        }
    }
}
