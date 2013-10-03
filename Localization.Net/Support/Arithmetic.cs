using System;
using Localization.Net.Exceptions;

namespace Localization.Net.Support
{
    public static class Arithmetic
    {

        public static bool CompareTo<T>(this T lhs, T rhs, CompareOperator op) where T : IComparable<T>
        {
            int c = lhs.CompareTo(rhs);
            if (c == 0)
            {
                return op == CompareOperator.GtEq || op == CompareOperator.LtEq || op == CompareOperator.Eq;
            }
            else
            {
                return (c == -1 && (op == CompareOperator.Lt || op == CompareOperator.LtEq)) 
                    || (c == 1 && (op == CompareOperator.Gt || op == CompareOperator.GtEq)) || op == CompareOperator.Neq;
            }            
        }

        public static double Evaluate(this double lhs, double rhs, ArithmeticOperator op)
        {
            switch (op)
            {
                case ArithmeticOperator.Addition: return lhs + rhs;
                case ArithmeticOperator.Subtraction: return lhs - rhs;
                case ArithmeticOperator.Multiplication: return lhs * rhs;
                case ArithmeticOperator.Division: return lhs / rhs;
                case ArithmeticOperator.IntegerDivision: return Math.Floor(lhs / rhs);
                case ArithmeticOperator.Modulo: return (int)lhs % (int)rhs;                    
            }

            throw new LocalizedInvalidOperationException();
        }

        public static string CompareOperatorRegex
        {
            get
            {
                return "(=|!=|<>|<=?|>=?)";
            }
        }

        public static string ArithemticOperatorRegex
        {
            get
            {
                return @"(\+|\-|\*|\/|\\|%)";
            }
        }

        public static CompareOperator GetCompareOperator(string spelling)
        {
            switch (spelling.ToLower())
            {
                case "=": return CompareOperator.Eq;
                case "!=": case "<>": return CompareOperator.Neq;
                case "<": return CompareOperator.Lt;
                case ">": return CompareOperator.Gt;
                case "<=" :return CompareOperator.LtEq;
                case ">=": return CompareOperator.GtEq;
            }

            throw new LocalizedInvalidOperationException();
        }

        public static ArithmeticOperator GetArithmeticOperator(string spelling)
        {
            switch (spelling)
            {
                case "+": return ArithmeticOperator.Addition;
                case "-": return ArithmeticOperator.Subtraction;
                case "*": return ArithmeticOperator.Multiplication;
                case "/": return ArithmeticOperator.Division;
                case "\\": return ArithmeticOperator.IntegerDivision;
                case "%": return ArithmeticOperator.Modulo;
            }

            throw new LocalizedInvalidOperationException();
        }
    }

    public enum CompareOperator { Eq, Lt, LtEq, Gt, GtEq, Neq }
    public enum ArithmeticOperator { Addition, Subtraction, Division, IntegerDivision, Multiplication, Modulo }
}
