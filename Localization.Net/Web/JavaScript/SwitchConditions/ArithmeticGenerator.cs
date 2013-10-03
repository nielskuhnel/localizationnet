using System;
using Localization.Net.Processing.SwitchConditions;
using Localization.Net.Support;

namespace Localization.Net.Web.JavaScript.SwitchConditions
{
    public class ArithmeticGenerator : PatternProcessorGenerator<ArithmeticCondition>
    {       

        public override void WriteEvaluator(ArithmeticCondition proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            foreach (var op in proc.Operations)
            {
                if (op.Operator == ArithmeticOperator.IntegerDivision)
                {
                    writer.Output.Write("Math.floor");
                }
                writer.Output.Write("(");
            }
            argumentWriters[0]();
            foreach (var op in proc.Operations)
            {
                writer.Output.Write(GetOperator(op.Operator));
                writer.Output.Write(op.Number.ToString(writer.ScriptCulture));
                writer.Output.Write(")");
            }

            writer.Output.Write(GetComparer(proc.CompareOperator));

            WriteScriptConstant(writer, proc.TargetValue);
        }


        string GetOperator(ArithmeticOperator op)
        {
            switch (op)
            {
                case ArithmeticOperator.Addition: return "+";
                case ArithmeticOperator.Subtraction: return "-";
                case ArithmeticOperator.Division:
                case ArithmeticOperator.IntegerDivision: return "/";
                case ArithmeticOperator.Modulo: return "%";
                case ArithmeticOperator.Multiplication: return "*";
            }

            throw new ArgumentOutOfRangeException("Unsupported operator");
        }

        string GetComparer(CompareOperator op)
        {
            switch (op)
            {
                case CompareOperator.Eq: return "===";
                case CompareOperator.Neq: return "!=";
                case CompareOperator.Lt: return "<";
                case CompareOperator.LtEq: return "<=";
                case CompareOperator.Gt: return ">";
                case CompareOperator.GtEq: return ">=";                    
            }

            throw new ArgumentOutOfRangeException("Unsupported operator");
        }
    }
}
