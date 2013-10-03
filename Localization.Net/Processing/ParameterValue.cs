using System;

namespace Localization.Net.Processing
{       


    /// <summary>
    /// A value for <see cref="ParameterSet"/>s. Logic can be applied after the string has been formatted in the <see cref="PatternEvaluator"/>
    /// </summary>    
    public class ParameterValue
    {        
        /// <summary>
        /// The actual value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Default format if nothing is specified
        /// </summary>
        public IValueFormatter DefaultFormat { get; set; }

        public ParameterValue(object value)
        {
            var other = value as ParameterValue;
            if (other != null)
            {
                Value = other.Value;
                DefaultFormat = other.DefaultFormat;
            }
            else
            {
                Value = value;
            }
        }

        
        public virtual string Format(Func<string, string> stringEncoder, string formattedValue)
        {
            return stringEncoder(formattedValue);
        }            

        public virtual ParameterValue Clone()
        {
            return (ParameterValue) this.MemberwiseClone();
        }


        public override string ToString()
        {
            return Format(x => x, "" + Value);
        }

        /// <summary>
        /// Wraps the specified value if its not a <see cref="ParameterValue"/> already.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>        
        public static ParameterValue Wrap(object value)
        {
            var pv = value as ParameterValue;
            return pv ?? new ParameterValue(value);
        }    
    }  


    /// <summary>
    /// A strongly typed value for <see cref="ParameterSet"/>s. Logic can be applied after the string has been formatted in the <see cref="PatternEvaluator"/>
    /// </summary>
    /// <typeparam name="TValue">The value type</typeparam>
    public class ParameterValue<TValue> : ParameterValue
    {

        public TValue TypedValue
        {
            get { return (TValue)Value; }
            set { Value = value; }
        }

        

        public ParameterValue(TValue value)
            : base(value)
        {}

        public static implicit operator TValue(ParameterValue<TValue> pv)
        {
            return pv.TypedValue;
        }
    }   
}
