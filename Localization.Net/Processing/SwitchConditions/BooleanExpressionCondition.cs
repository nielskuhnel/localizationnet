using System;
using System.Text.RegularExpressions;
using Localization.Net.Parsing;

namespace Localization.Net.Processing.SwitchConditions
{
    public class BooleanExpressionCondition : ISwitchConditionEvaluator
    {
        public ISwitchConditionEvaluator Left { get; set; }
        public ISwitchConditionEvaluator Right { get; set; }

        public bool Disjunction = false;

        public bool Evaluate(ParameterValue o, EvaluationContext context)
        {
            if (Disjunction)
            {
                return Left.Evaluate(o, context) || Right.Evaluate(o, context);
            }
            else
            {
                return Left.Evaluate(o, context) && Right.Evaluate(o, context);
            }
        }
    }

    public class BooleanExpressionConditionFactory : StringBasedSwitchConditionEvaluatorFactory
    {
        //Balancing groups!!! http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpcongroupingconstructs.asp?frame=true
        //TODO: Make a more clear way to express this. Maybe not use Regex      

        static string _s = @"([^\(](.(?!and|or))*) | ((((?'Open'\()[^\(\)]*)+((?'Close-Open'\))[^\(\)]*)+)*(?(Open)(?!)))";

        static string _op = @"\s+(?<Operator>and|or)\s+";

        static Regex _matcher = new Regex(string.Format(
            @"\s* (?<Left>{0}) {1} (?<Right>.+)", _s, _op), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public override ISwitchConditionEvaluator GetFor(string spelling, PatternDialect dialect, TextManager manager)
        {
            if (spelling.StartsWith("(") && spelling.EndsWith(")"))
            {
                spelling = spelling.Substring(1, spelling.Length - 1);
            }            

            Match m;
            if ((m = _matcher.Match(spelling)).Success)
            {
                var left = RemoveParentheses(m.Groups["Left"].Value);
                var right = RemoveParentheses(m.Groups["Right"].Value);

                return new BooleanExpressionCondition
                {
                    Left = dialect.GetSwitchConditionEvaluator(Expression.Text(left), manager),                    
                    Disjunction = m.Groups["Operator"].Value.Equals("or", StringComparison.CurrentCultureIgnoreCase),
                    Right = dialect.GetSwitchConditionEvaluator(Expression.Text(right), manager)
                };
            }

            return null;
        }

        //Remove leading and trailing parentheses
        string RemoveParentheses(string s)
        {
            if (s.StartsWith("(")) s = s.Substring(1);
            if (s.EndsWith(")")) s = s.Substring(0, s.Length - 1);

            return s;
        }
    }
}
