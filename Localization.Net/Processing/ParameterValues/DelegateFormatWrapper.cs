using System;

namespace Localization.Net.Processing.ParameterValues
{
    /// <summary>
    /// Provides a format to the value in the <see cref="PatternEvaluator"/> after the actual value has passed its <see cref="IValueFormatter"/>.
    /// The delegate must encode the formatted value.
    /// </summary>
    public class DelegateFormatWrapper<TValue, TReference> : ParameterValue<TValue>
    {

        /// <summary>
        /// Gets or sets the object reference that will be passed to the formatting delegate.
        /// </summary>
        /// <value>
        /// The delegate reference.
        /// </value>
        public TReference DelegateReference { get; set; }

        public Func<DelegateValueFormatArgs<TValue, TReference>, object> FormatDelegate { get; set; }

        public DelegateFormatWrapper(TValue value, TReference reference, Func<DelegateValueFormatArgs<TValue, TReference>, object> formatter)
            :base(value)
        {            
            FormatDelegate = formatter;
            DelegateReference = reference;
        }
        
        public override string Format(Func<string, string> stringEncoder, string formattedValue)
        {
            return "" + FormatDelegate(new DelegateValueFormatArgs<TValue, TReference>
            {
                Value = TypedValue,
                Reference = DelegateReference,
                FormattedValue = formattedValue,
                Encoder=stringEncoder
            });
        }        
    }

    public class DelegateFormatWrapper : DelegateFormatWrapper<object, object>
    {
        public DelegateFormatWrapper(object value, Func<DelegateValueFormatArgs<object, object>, object> formatter)
            : base(value, value, formatter)
        {            
        }
    }

    public class DelegateValueFormatArgs<TValue, TReference>
    {
        public TValue Value { get; set; }
        public TReference Reference { get; set; }
        public string FormattedValue { get; set; }
        public Func<string,string> Encoder { get; set; }
    }
}
