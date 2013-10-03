using System;

namespace Localization.Net.Processing.ParameterValues
{
    /// <summary>
    /// The value is written without encoding.
    /// </summary>
    public class UnencodedParameterValue : ParameterValue
    {
        public object Value { get; set; }

        public UnencodedParameterValue(object value) 
            : base(value)
        {
            
        }        


        public override string Format(Func<string, string> stringEncoder, string formattedValue)
        {
            return formattedValue;
        }        
    }
}
