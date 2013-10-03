using System;
using Localization.Net.Processing.ValueFormatters;

namespace Localization.Net.Web.JavaScript.ValueFormatters
{
    public class StringCaseGenerator : PatternProcessorGenerator<StringCaseFormatter>
    {
        public override void WritePrerequisites(System.IO.TextWriter writer)
        {
            writer.Write(@"function _lc(s){return s.toLowerCase();}");
            writer.Write(@"function _uc(s){return s.toUpperCase();}");
            writer.Write(@"function _ca(s){var c=s.split(' ');for(var i=0;i<c.length;i++)c[i]=_cf(c[i]);return c.join(' ');}");
            writer.Write(@"function _cf(s){return s.length ? s.substring(0, 1).toUpperCase() + s.substring(1).toLowerCase() : s;}");
        }
        public override void WriteEvaluator(StringCaseFormatter proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            switch (proc.TransformationType)
            {
                case StringCaseTransformationType.Lowercase:
                    writer.Output.Write("_lc(");
                    break;
                case StringCaseTransformationType.Uppercase:
                    writer.Output.Write("_uc(");
                    break;
                case StringCaseTransformationType.CapitalizeFirst:
                    writer.Output.Write("_cf(");
                    break;
                case StringCaseTransformationType.CapitalizeAll:
                    writer.Output.Write("_ca(");
                    break;
            }
            writer.Output.Write(writer.Json.Serialize("") + "+");
            argumentWriters[0]();
            writer.Output.Write(")");
        }
    }
}
