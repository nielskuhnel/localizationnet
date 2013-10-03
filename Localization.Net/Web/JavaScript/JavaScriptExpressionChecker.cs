using System;
using System.Collections.Generic;
using Localization.Net.Parsing;
using Localization.Net.Processing;

namespace Localization.Net.Web.JavaScript
{
    class JavaScriptExpressionChecker : DescendingPatternVisitor<JavaScriptExpressionChecker.JavaScriptExpressionCheckerState>
    {
        public class JavaScriptExpressionCheckerState
        {
            public List<MissingWriter> MissingWriters { get; set; }
            public HashSet<IJavaScriptGenerator> UsedWriters { get; set; }
        }

        public class MissingWriter
        {
            public Type Interface;
            public Type MissingType;
        }

        public Dictionary<Type, IJavaScriptGenerator> Writers { get; set; }


        public JavaScriptExpressionCheckerState CheckExpression(Expression expr)
        {
            var state = CreateInitialState();
            expr.Accept(this, state);
            return state;
        }


        public override JavaScriptExpressionChecker.JavaScriptExpressionCheckerState CreateInitialState()
        {
            return new JavaScriptExpressionCheckerState
            {
                MissingWriters = new List<MissingWriter>(),
                UsedWriters = new HashSet<IJavaScriptGenerator>()
            };
        }


        public JavaScriptExpressionChecker(Dictionary<Type, IJavaScriptGenerator> writers)
        {
            Writers = writers;            
        }


        void RegisterWriter(Type forType, Type forInterface, JavaScriptExpressionCheckerState state)
        {
            IJavaScriptGenerator generator;
            if (Writers.TryGetValue(forType, out generator))
            {
                state.UsedWriters.Add(generator);
            }
            else
            {
                state.MissingWriters.Add(new MissingWriter { Interface = typeof(IParameterEvaluator), MissingType = forType });
            }
        }


        public override void Visit(ParameterSpec spec, JavaScriptExpressionCheckerState state)
        {

            RegisterWriter(spec.Evaluator.GetType(), typeof(IParameterEvaluator), state);
            
            if (spec.Formatter != null)
            {
                RegisterWriter(spec.Formatter.GetType(), typeof(IValueFormatter), state);                
            }

            base.Visit(spec, state);
        }

        public override void Visit(FormatGroup group, JavaScriptExpressionCheckerState state)
        {
            RegisterWriter(group.Evaluator.GetType(), typeof(IParameterEvaluator), state);
            RegisterWriter(group.Expander.GetType(), typeof(IFormatGroupExpander), state);

            base.Visit(group, state);
        }

        public override void Visit(Switch sw, JavaScriptExpressionCheckerState state)
        {
            Visit((ParameterSpec)sw, state);

            base.Visit(sw, state);
        }

        public override void Visit(SwitchCase sc, JavaScriptExpressionCheckerState state)
        {
            RegisterWriter(sc.Evaluator.GetType(), typeof(ISwitchConditionEvaluator), state);
            
            base.Visit(sc, state);
        }
    }
}
