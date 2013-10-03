using System;
using Localization.Net.Processing.SwitchConditions;

namespace Localization.Net.Web.JavaScript.SwitchConditions
{
    public class SingleValueGenerator<T> : PatternProcessorGenerator<SingleValueCondition<T>>
    {        

        public override void WriteEvaluator(SingleValueCondition<T> proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            argumentWriters[0]();            
            writer.Output.Write((proc.NotEquals ? "!" : "=") + "==");
            WriteScriptConstant(writer, proc.Value);
        }

    }

    public class ValueListGenerator<T> : PatternProcessorGenerator<ValueListCondition<T>>
    {        

        public override void WriteEvaluator(ValueListCondition<T> proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            int i = 0;
            foreach (var val in proc.Values)
            {
                if (i++ > 0) writer.Output.Write("||");
                argumentWriters[0]();                
                writer.Output.Write("===");
                WriteScriptConstant(writer, val);
            }
        }
    }
}
