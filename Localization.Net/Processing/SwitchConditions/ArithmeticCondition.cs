using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Localization.Net.Support;

namespace Localization.Net.Processing.SwitchConditions
{
    public class ArithmeticCondition : ISwitchConditionEvaluator
    {
        public class Operation
        {
            public double Number { get; set; }
            public ArithmeticOperator Operator { get; set; }
        }


        public List<Operation> Operations { get; set; }        

        public double TargetValue { get; set; }

        public CompareOperator CompareOperator { get; set; }

        public ArithmeticOperator ArithmeticOperator { get; set; }


        public bool Evaluate(ParameterValue o, EvaluationContext context)
        {
            try
            {
                double n = Convert.ToDouble(o.Value);
                foreach (var rhs in Operations)
                {
                    n = n.Evaluate(rhs.Number, rhs.Operator);
                }

                return  n.CompareTo(TargetValue, CompareOperator);
            }
            catch
            {
                return false;
            }
        }

        public enum Comparer { Lt, Gt, Lte, Gte, Eq, Neq };
    }

    public class ArithmeticConditionFactory : StringBasedSwitchConditionEvaluatorFactory
    {

        static Regex operation = new Regex(string.Format(@"\s* (?<Op>{0}) \s* (?<Number>\d+)", Arithmetic.ArithemticOperatorRegex),
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        static Regex matcher = new Regex(string.Format(
                @"(?<Ops> (\s* {0} \s* \d+)+) \s* (?<Op>{1}) \s* (?<TargetValue>\d+)", 
                    Arithmetic.ArithemticOperatorRegex,
                    Arithmetic.CompareOperatorRegex), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public override ISwitchConditionEvaluator GetFor(string spelling, PatternDialect dialect, TextManager manager)
        {
            Match m;
            if ((m = matcher.Match(spelling)).Success)
            {
                var ops = new List<ArithmeticCondition.Operation>();
                foreach (Match op in operation.Matches(m.Groups["Ops"].Value))
                {
                    ops.Add(new ArithmeticCondition.Operation
                    {
                       Number = double.Parse(op.Groups["Number"].Value, dialect.Parser.PatternCulture),
                       Operator = Arithmetic.GetArithmeticOperator(op.Groups["Op"].Value)
                    });
                }

                return new ArithmeticCondition
                {
                    Operations = ops,
                    CompareOperator = Arithmetic.GetCompareOperator(m.Groups["Op"].Value),
                    TargetValue = int.Parse(m.Groups["TargetValue"].Value)
                };
            }

            return null;
        }
    }
}
