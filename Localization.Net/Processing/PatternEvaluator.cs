using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Localization.Net.Parsing;
using Localization.Net.Processing.ParameterValues;

namespace Localization.Net.Processing
{
    public class PatternEvaluator
    {

        public Expression Expression { get { return _evaluator.Expression; } }

        public IPatternTransformer PatternTransformer { get; set; }


        private PatternEvaluatingVisitor _evaluator;
        public PatternEvaluator(Expression expression)
        {
            _evaluator = new PatternEvaluatingVisitor(expression);
        }

        
        public string Evaluate(EvaluationContext context)
        {
            StringWriter temp = new StringWriter();
            Evaluate(context, temp);
            return temp.ToString();
        }

        public void Evaluate(EvaluationContext context, TextWriter target)
        {
            context = context.Clone();
            context.Parameters = new LayeredParameterSet(context.Parameters);

            if (PatternTransformer == null)
            {
                var state = new State { Writer = target, Context = context };
                _evaluator.Expression.Accept(_evaluator, state);
            }
            else
            {
                StringWriter temp = new StringWriter();
                var state = new State { Writer = temp, Context = context };
                _evaluator.Expression.Accept(_evaluator, state);
                string result = temp.ToString();
                target.Write(PatternTransformer.Decode(result));
            }
        }


        //Nested class to hide State
        class PatternEvaluatingVisitor : DescendingPatternVisitor<State>
        {
            public Expression Expression { get; private set; }            

            public PatternEvaluatingVisitor(Expression expression)
            {
                Expression = expression;                
            }


            public override void Visit(Text text, State state)
            {                
                state.Writer.Write(state.Context.StringEncoder(text.Spelling));
                base.Visit(text, state);
            }

            public override void Visit(ParameterSpec spec, State state)
            {                
                state.Writer.Write(GetParameterValue(spec.Evaluator.GetValue(state.Context), spec.Formatter, state));
                
                base.Visit(spec, state);
            }

            public override void Visit(Switch sw, State state)
            {                
                var val = sw.Evaluator.GetValue(state.Context);                       
                                
                if (val.Value == null)
                {                    
                    if (sw.NullExpression != null)
                    {
                        sw.NullExpression.Accept(this, state);
                    }
                }
                else
                {                
                    //Enumeration
                    var enumeration = val.Value as IEnumerable;
                    if (enumeration != null && !(enumeration is string)) //We don't want to enumerate the chars in a string
                    {                        
                        var vals = enumeration.Cast<object>().ToArray();
                        int iBackwards = -vals.Length;
                        for (int i = 0; i < vals.Length; i++)
                        {                            
                            state.Values.PushLayer();

                            //The parameter '#Index' provides the current index (1-indexed)
                            state.Values["#Index"] = ParameterValue.Wrap(i + 1);
                            //The parameter '#' here provides the enumerable's current value

                            var pval = vals[i] as ParameterValue;
                            if (pval == null)
                            {
                                //If the item of the enumerable is not a parameter value a clone is created to preserve wrapping, default format etc.                               
                                pval = val.Clone();
                                pval.Value = vals[i];
                            }
                            else
                            {
                                pval = ParameterValue.Wrap(vals[i]);
                            }

                            pval.DefaultFormat = pval.DefaultFormat ?? val.DefaultFormat;


                            if (sw.Formatter != null)
                            {
                                state.Values["#"] = new UnencodedParameterValue(GetParameterValue(pval, sw.Formatter, state));
                            }
                            else
                            {
                                state.Values["#"] = pval;
                            }
                            
                            foreach (var sc in sw.Cases)
                            {
                                if (sc.Evaluator.Evaluate(ParameterValue.Wrap(i), state.Context) || sc.Evaluator.Evaluate(ParameterValue.Wrap(iBackwards), state.Context))
                                {
                                    sc.Expression.Accept(this, state);
                                    break;
                                }
                            }

                            state.Values.PopLayer();
                            ++iBackwards;
                        }
                    }
                    else
                    {                        
                        //The parameter '#' is shorthand for 'evaluated value' in switches. If a format is specified for the switch parameter this is applied
                        state.Values.PushLayer();

                        state.Values["#"] = sw.Formatter != null ?
                            new UnencodedParameterValue(GetParameterValue(val, sw.Formatter, state)) :
                            val;

                        foreach (var sc in sw.Cases)
                        {
                            if (sc.Evaluator.Evaluate(val, state.Context))
                            {
                                sc.Expression.Accept(this, state);
                                break;
                            }
                        }
                        state.Values.PopLayer();
                    }
                }
                               

                //Dont' visit cases individually. They are all handled in this method: base.Visit(sw, state);
            }

            public override void Visit(FormatGroup group, State state)
            {
                var val = (string)group.Evaluator.GetValue(state.Context).Value;
                
                if (!string.IsNullOrEmpty(val))
                {
                    string inner;
                    if (group.Expression != null)
                    {
                        var innerState = state.Clone();
                        innerState.Writer = new StringWriter();
                        group.Expression.Accept(this, innerState);
                        inner = innerState.Writer.ToString();
                        //StringEncoder is not used here. Inner is already encoded and the value for the format group is left unencoded                        
                    }
                    else
                    {
                        inner = "";
                    }
                    state.Writer.Write(group.Expander.Expand(val, inner));
                }
                else if( group.Expression != null )
                {
                    //Format pattern is missing. Just write inner expression
                    group.Expression.Accept(this, state);
                }
            }

            public override void Visit(CustomExpressionPart part, State state)
            {
                part.Evaluate(state.Context, state.Writer);

                base.Visit(part, state);
            }

            protected string GetParameterValue(ParameterValue value, IValueFormatter valueFormatter, State state)
            {
                //Adjust time zone
                if( value.Value is DateTime )
                {
                    value.Value = ((DateTime) value.Value).AdjustToTimeZone(state.Context.TimeZoneInfo);    
                }

                string formattedValue;
                try
                {
                    formattedValue = valueFormatter.FormatValue(value, state.Context);
                }
                catch
                {
                    //The formatter couldn't format the value. Just print the value.
                    formattedValue = "" + value.Value;
                }

                return value.Format(state.Context.StringEncoder, formattedValue);                
            }
        }

        
        class LayeredParameterSet : ParameterSet
        {
            List<ParameterSet> _layers;

            public LayeredParameterSet(ParameterSet level0)
            {
                _layers = new List<ParameterSet>();
                _layers.Add(level0);                
            }

            public void PushLayer()
            {
                _layers.Add(new DicitionaryParameterSet());   
            }

            public void PopLayer()
            {
                _layers.RemoveAt(_layers.Count - 1);
            }

            public override bool Contains(string key)
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    if (_layers[i].Contains(key))
                    {
                        return true;
                    }
                }
                return false;
            }

            protected override ParameterValue GetInternal(string key)
            {
                ParameterValue val = null;
                for (int i = _layers.Count - 1; (val == null || val.Value == null) && i >= 0; i--)
                {
                    val = _layers[i][key];                    
                }
                return val;
            }

            protected override void SetInternal(string key, ParameterValue value)
            {
                _layers[_layers.Count - 1][key] = value;
            }

            public override IEnumerable<string> Keys
            {
                get {
                    var keys = new HashSet<string>();
                    foreach (var layer in _layers)
                    {
                        foreach (var key in layer.Keys) keys.Add(key);
                    }
                    return keys;
                }
            }
        }

        //This struct represents the thread safe state of the visitor
        struct State
        {
            public LayeredParameterSet Values
            {
                get { return Context.Parameters as LayeredParameterSet; }
            }
            public TextWriter Writer;
            public EvaluationContext Context;

            public State Clone()
            {
                return new State
                {
                    Writer = Writer,
                    Context = Context.Clone()
                };
            }
        }        
    }
}
