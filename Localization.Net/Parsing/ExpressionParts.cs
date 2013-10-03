using System.Collections.Generic;
using System.IO;
using Localization.Net.Processing;

namespace Localization.Net.Parsing
{    

    public abstract class PatternPart
    {
        public abstract void Accept<T>(IPatternVisitor<T> visitor, T state);

        public virtual void Accept<T>(IPatternVisitor<T> visitor)
        {
            Accept<T>(visitor, visitor.CreateInitialState());
        }


        
        public override string ToString()
        {
            var printer = new PatternPartPrinter();
            this.Accept(printer);

            return printer.ToString();
        }
    }

    public class Expression : PatternPart
    {
        public List<ExpressionPart> Parts = new List<ExpressionPart>();

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {            
            visitor.Visit(this, state);            
        }

        public static Expression Text(string spelling)
        {
            return new Expression { Parts = new List<ExpressionPart> { new Text { Spelling = spelling } } };
        }
    }

    public abstract class ExpressionPart : PatternPart { }

    public class Text : ExpressionPart
    {
        public string Spelling;

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);
        }        
    }

    public class ParameterSpec : ExpressionPart
    {
        public string ParameterName;
        public string ParameterFormat;

        public string Arguments;

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);            
        }

        public IValueFormatter Formatter;
        public IParameterEvaluator Evaluator;        
    }

    public class FormatGroup : ExpressionPart
    {
        public string ParameterName;
        public Expression Expression;

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);
        }

        public IParameterEvaluator Evaluator;

        public IFormatGroupExpander Expander;
    }

    public class Switch : ParameterSpec
    {
        public string SwitchTemplateName { get; set; }        

        public List<SwitchCase> Cases = new List<SwitchCase>();
        public Expression NullExpression;

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);                       
        }        
    }

    public class SwitchCase : PatternPart
    {
        public Expression Condition;
        public Expression Expression;

        public ISwitchConditionEvaluator Evaluator;

        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);
        }        
    }

    /// <summary>
    /// Extension point for external parsers
    /// </summary>
    public abstract class CustomExpressionPart : ExpressionPart
    {
        public override void Accept<T>(IPatternVisitor<T> visitor, T state)
        {
            visitor.Visit(this, state);
        }

        /// <summary>
        /// Override this method to use the build in pattern decorator
        /// </summary>        
        public virtual void Decorate(PatternDialect dialect, TextManager manager){}

        /// <summary>
        /// Override this method to use the build in pattern evaluator
        /// </summary>        
        public virtual void Evaluate(EvaluationContext context, TextWriter writer) { }

        public override string ToString()
        {
            return "CustomExpressionPart";
        }
    }
}
