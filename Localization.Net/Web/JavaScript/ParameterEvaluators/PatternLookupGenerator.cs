using System;
using Localization.Net.Processing.ParameterEvaluators;

namespace Localization.Net.Web.JavaScript.ParameterEvaluators
{
    public class PatternLookupGenerator : PatternProcessorGenerator<PatternLookupEvaluator>
    {        

        public override void WriteEvaluator(PatternLookupEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            //NOTE: This one is hard to put in a general purpose function in Localization.js as it relies on formatters
            writer.Output.Write("(function(){");
            writer.Output.Write("k=");
            if (proc.PatternKey.StartsWith("@"))
            {
                var parts = proc.PatternKey.Substring(1).Split('+');

                WriteGetParameter(writer, writer.Json.Serialize(parts[0]));
                if (parts.Length > 1)
                {                                       
                    writer.Output.Write("+" + writer.Json.Serialize(parts[1]));
                }
            }
            else
            {
                writer.Output.Write(writer.Json.Serialize(proc.PatternKey));
            }
            writer.Output.Write(";");
            writer.Output.Write("var i=k.indexOf(" + writer.Json.Serialize(proc.NamespaceQualifier) + ");" +
                "var ns=c.n;if(i!=-1){ns=k.substr(0, k);k=k.substr(k+" + proc.NamespaceQualifier.Length + ");}");

            writer.Output.Write("var vs={};");
            int i = 0;
            foreach (var p in proc.Parameters)
            {    
                writer.Output.Write("var v=");
                if ((p.Key.StartsWith("\"") || p.Key.StartsWith("'")) && (p.Key.EndsWith("\"") || p.Key.EndsWith("'")))
                {
                    writer.Output.Write(writer.Json.Serialize(p.Key.Substring(1, p.Key.Length - 2)));                    
                }
                else
                {                    
                    WriteGetParameter(writer, writer.Json.Serialize(p.Key));
                    writer.Output.Write(";vs[" + writer.Json.Serialize(p.Key) + "]=v");
                    if (p.Value != null)
                    {
                        //TODO: Restore original format
                        writer.Output.Write(";v.defaultFormat=");
                        writer.Output.Write("function(v){return ");
                        var formatter = writer.Writers[p.Value.GetType()];
                        formatter.WriteEvaluator(p.Value, writer, () => writer.Output.Write("v"));
                        writer.Output.Write("}");
                    }
                    //TODO: Format
                }
                writer.Output.Write(";vs[''+" + writer.Json.Serialize(i++) + "]=v;");
            }

            writer.Output.Write("return m.unencoded(");
            WriteGetText(writer, "ns", "k", "vs");

            writer.Output.Write(");})()");

        }
    }
}
