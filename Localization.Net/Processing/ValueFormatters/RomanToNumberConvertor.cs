using System;
using System.Text;
using Localization.Net.Exceptions;

namespace Localization.Net.Processing.ValueFormatters
{

    /// <summary>
    /// Factory class for <see cref="RomanNumberFormatter"/>
    /// </summary>
    public class RomanNumberFormatterFactory : IValueFormatterFactory
    {

        public IValueFormatter GetFor(string formatExpression, PatternDialect dialect, TextManager manager)
        {
            if (!string.IsNullOrEmpty(formatExpression) && formatExpression.Equals("roman", StringComparison.InvariantCulture))
            {
                return new RomanNumberFormatter();
            }

            return null;
        }
    }

    /// <summary>
    /// Formats numbers as roman numbers. Numbers must be between 1 and 3999
    /// </summary>
    public class RomanNumberFormatter : IValueFormatter
    {
        private NumberToRomanConvertor _converter;
        public RomanNumberFormatter()
        {
            _converter = new NumberToRomanConvertor();
        }

        public string FormatValue(ParameterValue value, EvaluationContext context)
        {            
            return _converter.NumberToRoman((int) Convert.ChangeType(value.Value, typeof(int)));
        }
    }




    /// <summary>
    /// Thanks http://www.blackwasp.co.uk/RomanToNumber.aspx
    /// </summary>
    class NumberToRomanConvertor
    {
        // Converts an integer value into Roman numerals
        public string NumberToRoman(int number)
        {
            // Validate
            if (number < 0 || number > 3999)
                throw new LocalizedArgumentOutOfRangeException("number", 0, 3999);

            if (number == 0) return "N";

            // Set up key numerals and numeral pairs
            int[] values = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            // Initialise the string builder
            StringBuilder result = new StringBuilder();

            // Loop through each of the values to diminish the number
            for (int i = 0; i < 13; i++)
            {
                // If the number being converted is less than the test value, append
                // the corresponding numeral or numeral pair to the resultant string
                while (number >= values[i])
                {
                    number -= values[i];
                    result.Append(numerals[i]);
                }
            }

            // Done
            return result.ToString();
        }
    }
}
