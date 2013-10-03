using System;
using Localization.Net.Processing;

namespace Localization.Net.Web.JavaScript.FormatGroupExpanders
{
    public class HashTagFormatGroupExpanderGenerator : PatternProcessorGenerator<HashTagFormatGroupExpander>    
    {        

        /// <summary>
        /// Writes the javascript to evalutate the format group
        /// </summary>
        /// <param name="expander">The expander (not used).</param>
        /// <param name="writer">The generator to write the output to.</param>
        /// <param name="argumentWriters">Argument 1: Format string, Argument 2: Value to put in format string.</param>
        public override void WriteEvaluator(HashTagFormatGroupExpander expander, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            argumentWriters[0]();
            writer.Output.Write(".replace(\"{#}\",");
            argumentWriters[1]();
            writer.Output.Write(")");            
        }
    }
}
