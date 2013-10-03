using System;

namespace Localization.Net.Web.JavaScript
{
    public abstract class PatternProcessorGenerator<T> : IJavaScriptGenerator  
    {        

        public virtual void WritePrerequisites(System.IO.TextWriter writer)
        {
            
        }

        protected void WriteScriptConstant(JavaScriptExpressionWriter writer, object val)
        {
            writer.Output.Write(string.Format(writer.ScriptCulture, "{0}", val));
        }

        protected void WriteGetParameter(JavaScriptExpressionWriter writer, string nameExpr)
        {
            writer.Output.Write("c.g(");
            writer.Output.Write(nameExpr);
            writer.Output.Write(")");
        }

        protected void WriteGetText(JavaScriptExpressionWriter writer, string nsExpr, string keyExpr, string valuesExpr)
        {
            //"m" is the current manager from the initialization function
            writer.Output.Write("m.get(");            
            writer.Output.Write(keyExpr);
            writer.Output.Write(",");
            writer.Output.Write(valuesExpr);
            writer.Output.Write(",");
            writer.Output.Write(nsExpr);                        
            writer.Output.Write(")");
        }


        public void WriteEvaluator(object patternProcessor, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            WriteEvaluator((T)patternProcessor, writer, argumentWriters);
        }

        public abstract void WriteEvaluator(T proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters);
    }
}
