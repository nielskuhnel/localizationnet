using System;
using Localization.Net.Processing.SwitchConditions;

namespace Localization.Net.Web.JavaScript.SwitchConditions
{
    public class TakeAllGenerator : PatternProcessorGenerator<TakeAllCondition>
    {        

        public override void WriteEvaluator(TakeAllCondition proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            writer.Output.Write("true");
        }
    }
}
