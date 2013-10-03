using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization.Net.Processing.SwitchConditions
{
    /// <summary>
    /// Matches the value against a fixed value using the type's Equals method
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SingleValueCondition<TValue> : ISwitchConditionEvaluator
    {
        public bool NotEquals { get; set; }

        public TValue Value { get; private set; }

        public SingleValueCondition(TValue value, bool notEquals)
        {
            Value = value;
            NotEquals = notEquals;
        }

        public bool Evaluate(ParameterValue val, EvaluationContext context)
        {
            bool success = Value.Equals(Convert.ChangeType(val.Value, typeof(TValue)));
            return NotEquals ? !success : success;
        }
    }

    /// <summary>
    /// Matches the value against a list of values
    /// </summary>
    /// <typeparam name="TValues">The type of the list values</typeparam>
    public class ValueListCondition<TValues> : ISwitchConditionEvaluator
    {
        public bool NotEquals { get; set; }

        public HashSet<TValues> Values { get; private set; }

        public ValueListCondition(IEnumerable<TValues> values, bool notEquals)
        {
            Values = new HashSet<TValues>(values);
            NotEquals = notEquals;
        }

        public bool Evaluate(ParameterValue val, EvaluationContext context)
        {
            try
            {
                var success = Values.Contains((TValues)Convert.ChangeType(val.Value, typeof(TValues)));
                if( NotEquals ) success = !success;
                return success;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ValueListConditionFactory : StringBasedSwitchConditionEvaluatorFactory
    {
        public override ISwitchConditionEvaluator GetFor(string spelling, PatternDialect dialect, TextManager manager)
        {
            if (!string.IsNullOrEmpty(spelling))
            {
                bool notEquals;
                if (spelling.StartsWith("!="))
                {
                    notEquals = true;
                    spelling = spelling.Substring(2).TrimStart();
                }
                else
                {
                    notEquals = false;
                    if (spelling.StartsWith("="))
                    {
                        spelling = spelling.Substring(1).TrimStart();
                    }
                }

                var labels = spelling.Split(',').Select(x => x.Trim()).Where(x => x != "").ToArray();

                //If all the labels can be converted to a number (i.e. a double) it's better to represent them as that instead of doing string conversions
                bool allDouble = true;
                double d;
                double[] dvals = labels.Select(x => (allDouble = double.TryParse(x, out d)) ? d : 0).ToArray();
                if (allDouble)
                {
                    return dvals.Length == 1 ? (ISwitchConditionEvaluator)new SingleValueCondition<double>(dvals[0], notEquals)
                        : new ValueListCondition<double>(dvals, notEquals);
                }
                else
                {
                    return dvals.Length == 1 ? (ISwitchConditionEvaluator)new SingleValueCondition<string>(labels[0], notEquals)
                        : new ValueListCondition<string>(labels, notEquals);
                }
            }

            return null;
        }
    }

}
