using System;
using Localization.Net.Processing.SwitchConditions;

namespace Localization.Net.Web.JavaScript.SwitchConditions
{
    public class BooleanExpressionGenerator : PatternProcessorGenerator<BooleanExpressionCondition>
    {        

        public override void WriteEvaluator(BooleanExpressionCondition proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            var left = writer.Writers[proc.Left.GetType()];
            var right = writer.Writers[proc.Left.GetType()];

            left.WriteEvaluator(proc.Left, writer, argumentWriters[0]);

            writer.Output.Write(proc.Disjunction ? "||" : "&&");

            right.WriteEvaluator(proc.Right, writer, argumentWriters[0]);            

        }
    }
}
