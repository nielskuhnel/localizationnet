using System.Collections.Generic;
using System.Linq;
using Localization.Net.Exceptions;
using Localization.Net.Parsing;

namespace Localization.Net.Processing
{
    /// <summary>
    /// A pattern dialect consist of a parser that understands a specific grammar and factories for value formaters, parameter evaluators and switch case evaluators when the parsed AST is decorated
    /// </summary>
    public abstract class PatternDialect
    {
        public bool Encode { get; protected set; }    


        protected PatternDialect()
        {
            Encode = true;
        }

        /// <summary>
        /// Gets or sets a pattern transformer to make simple transformations to the dialect's grammar. (See e.g. <see cref="HHtmlPatternTransformer"/>)
        /// </summary>
        /// <value>
        /// The pattern transformer.
        /// </value>
        public IPatternTransformer PatternTransformer { get; set; }

        public ExpressionParser Parser { get; protected set; }

        public List<IValueFormatterFactory> ValueFormatters { get; set; }
        public List<IParameterEvaluatorFactory> ParameterEvaluators { get; set; }
        public List<ISwitchConditionEvaluatorFactory> SwitchConditionEvaluators { get; set; }

        //TODO: This ought to follow the same structure as valueformatters, parameter evaluators etc.
        public IFormatGroupExpander FormatGroupExpander { get; set; }        


        public virtual IParameterEvaluator GetParameterEvaluator(ParameterSpec spec, TextManager manager)
        {
            IParameterEvaluator evaluator = null;
            if (!ParameterEvaluators.Any(x => (evaluator = x.GetFor(spec, this, manager)) != null))
            {
                throw new LocalizedKeyNotFoundException("Exceptions.ParameterEvaluatorNotFound", "No parameter evaluator found for {0}", new { Text = spec });
            }

            return evaluator;
        }

        public virtual IValueFormatter GetValueFormatter(string spelling, TextManager manager)
        {
            IValueFormatter formatter = null;
            if (!ValueFormatters.Any(x => (formatter = x.GetFor(spelling, this, manager)) != null))
            {
                throw new LocalizedKeyNotFoundException("Exceptions.ValueFormatterNotFound", "No parameter evaluator found for {0}", new { Text = spelling});                
            }

            return formatter;
        }

        public virtual ISwitchConditionEvaluator GetSwitchConditionEvaluator(Expression expr, TextManager manager)
        {
            ISwitchConditionEvaluator sc = null;
            if (!SwitchConditionEvaluators.Any(x => (sc = x.GetFor(expr, this, manager)) != null))
            {
                throw new LocalizedKeyNotFoundException("Exceptions.SwitchConditionNotFound", "No switch condition evaluator found for {0}", new { Spec = expr });                
            }
            return sc;
        }



        public abstract PatternEvaluator GetEvaluator(string pattern, TextManager manager);        
    }
}
