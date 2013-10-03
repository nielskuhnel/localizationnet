using System;
using System.Linq;
using Localization.Net.Processing.ParameterEvaluators;

namespace Localization.Net.Web.JavaScript.ParameterEvaluators
{
    public class ReflectionParameterGenerator : PatternProcessorGenerator<ReflectionParameterEvaluator>
    {        

        public override void WriteEvaluator(ReflectionParameterEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            writer.Output.Write("rp(");
            WriteGetParameter(writer, writer.Json.Serialize(proc.BaseParameterName));            
            if (proc.Properties.Any())
            {
                writer.Output.Write(",[");
                writer.Output.Write(string.Join(",", proc.Properties.Select(p=>writer.Json.Serialize(p))));                
                writer.Output.Write("]");
            }            
            writer.Output.Write(")");
        }
    }
}
