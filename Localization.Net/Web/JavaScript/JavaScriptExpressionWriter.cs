using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Web.Script.Serialization;
using Localization.Net.Parsing;
using Localization.Net.Processing;

namespace Localization.Net.Web.JavaScript
{
    /// <summary>
    /// Writes expressions as JavaScript functions so they can be called without roundtrips to the server    
    /// </summary>
    public class JavaScriptExpressionWriter : DescendingPatternVisitor<JavaScriptExpressionWriterState>
    {
        public JavaScriptSerializer Json { get; set; }

        CultureInfo _scriptCulture = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo ScriptCulture
        {
            get { return _scriptCulture; }
        }

        public TextWriter Output { get; set; }
        public Dictionary<Type, IJavaScriptGenerator> Writers { get; set; }


        /// <summary>
        /// Gets or sets the context that may be used by writers. It does not contain parameters.
        /// </summary>
        /// <value>
        /// The base context.
        /// </value>
        public EvaluationContext BaseContext { get; set; }

        public string ClientClassName { get; set; }

        JavaScriptExpressionChecker _checker;

        public override JavaScriptExpressionWriterState CreateInitialState()
        {
            return new JavaScriptExpressionWriterState
            {
                InSubExpression = false
            };
        }


        


        public JavaScriptExpressionWriter(Dictionary<Type, IJavaScriptGenerator> writers, TextWriter output, EvaluationContext baseContext)
        {
            Json = new JavaScriptSerializer();
            Output = output;
            Writers = writers;
            BaseContext = baseContext;
            _checker = new JavaScriptExpressionChecker(writers);
        }


        public override void Visit(Expression expression, JavaScriptExpressionWriterState state)
        {
            bool allText = expression.Parts.All(x => x is Text);

            if (!state.InSubExpression && !allText)
            {
                Output.Write(@"function(c) {");
                Output.Write("return ");
            }

            var missingTypes = _checker.CheckExpression(expression).MissingWriters;

            //Use "A" + "B" + ... or ["A", "B", ...].join('') ?
            var strategy = expression.Parts.Count > 1 || missingTypes.Any() ? (StringJoinStrategy)
                new StringArrayJoinStrategy() : new StringConcatJoinStrategy();

            Output.Write(strategy.Prologue);

            int index = 0;            
            if (missingTypes.Any())
            {
                foreach (var missingType in missingTypes)
                {
                    Output.Write(strategy.Separator(index++));
                    Output.Write(Json.Serialize("Missing " +
                        missingType.Interface.Name + ": " +
                        missingType.MissingType.Name + "\n"));
                }
            }
            else
            {
                var subState = state.Clone();
                subState.InSubExpression = true;
                foreach (var part in expression.Parts)
                {
                    Output.Write(strategy.Separator(index++));
                    part.Accept(this, subState);
                }
            }

            Output.Write(strategy.Epilogue);

            if (!state.InSubExpression && !allText)
            {
                Output.Write(";}");
            }
        }

        public override void Visit(ParameterSpec spec, JavaScriptExpressionWriterState state)
        {
            var evaulator = Writers[spec.Evaluator.GetType()];

            Output.Write("(function(){var v=");
            evaulator.WriteEvaluator(spec.Evaluator, this, null);
            Output.Write(";return af(v,"); //Apply format            
            if (spec.Formatter != null)
            {
                var formatter = Writers[spec.Formatter.GetType()];
                formatter.WriteEvaluator(spec.Formatter, this, () => {
                    Output.Write("val(");
                    evaulator.WriteEvaluator(spec.Evaluator, this, null);
                    Output.Write(")");
                });
            }
            else
            {
                Output.Write(Json.Serialize("") + "+val(v)");
            }
            Output.Write(",true);})()");
        }

        public override void Visit(FormatGroup group, JavaScriptExpressionWriterState state)
        {
            var evaluator = Writers[group.Evaluator.GetType()];
            var expander = Writers[group.Expander.GetType()];

            Output.Write("(function(){var p=val(");
            evaluator.WriteEvaluator(group.Evaluator, this, null);
            Output.Write("); return p!=null?");
            expander.WriteEvaluator(group.Expander, this, 
                () => Output.Write("p"),  //Format expression
                () => { //Inner expression                    
                    group.Expression.Accept(this, state);                    
                });
            Output.Write(":");
            Output.Write(Json.Serialize(""));
            Output.Write(";");
            Output.Write("})()");
        }


        public override void Visit(Text text, JavaScriptExpressionWriterState state)
        {
            Output.Write("e(");
            Output.Write(Json.Serialize(text.Spelling));
            Output.Write(")");
        }

        public override void Visit(Switch sw, JavaScriptExpressionWriterState state)
        {
            var evaluator = Writers[sw.Evaluator.GetType()];

            //TODO: Much of this can be put in a common function. Do that.

            Output.Write("sw(c,");
            evaluator.WriteEvaluator(sw.Evaluator, this, null);
            Output.Write(",");
            if (sw.NullExpression != null)
            {
                sw.NullExpression.Accept(this, state);
            }
            else
            {
                Output.Write(Json.Serialize(""));
            }
            Output.Write(",");
            
            if (sw.Formatter != null)
            {
                Output.Write("function(v){return ");
                var formatter = Writers[sw.Formatter.GetType()];
                formatter.WriteEvaluator(sw.Formatter, this, () => Output.Write("v"));
                Output.Write("}");
            }
            else
            {
                Output.Write("null");
            }
            Output.Write(",");
            Output.Write("function(v1,v2){return ");                        
            foreach (var sc in sw.Cases)
            {
                var cond = Writers[sc.Evaluator.GetType()];                                
                cond.WriteEvaluator(sc.Evaluator, this, () => Output.Write("v1"));
                Output.Write("||(v2!==undefined&&");
                cond.WriteEvaluator(sc.Evaluator, this, () => Output.Write("v2"));
                Output.Write(")");
                Output.Write("?");
                sc.Expression.Accept(this, state);
                Output.Write(":");
            }
            //If no cases match return ""
            Output.Write(Json.Serialize(""));
            Output.Write("})");
        }

        abstract class StringJoinStrategy
        {
            public abstract string Prologue { get; }
            public abstract string Separator(int index);
            public abstract string Epilogue { get; }
        }

        class StringConcatJoinStrategy : StringJoinStrategy
        {
            public override string Prologue { get { return ""; } }
            public override string Separator(int index) { return index == 0 ? "" : "+"; }
            public override string Epilogue { get { return ""; } }
        }

        class StringArrayJoinStrategy : StringJoinStrategy
        {
            public override string Prologue { get { return "["; } }
            public override string Separator(int index) { return index == 0 ? "" : ","; }
            public override string Epilogue { get { return "].join('')"; } }
        }
    }

    public struct JavaScriptExpressionWriterState
    {
        public bool InSubExpression;

        public JavaScriptExpressionWriterState Clone()
        {
            return (JavaScriptExpressionWriterState)MemberwiseClone();
        }
    }
}
