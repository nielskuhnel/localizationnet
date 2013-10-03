using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Localization.Net
{
    public class LocalizedStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if( sourceType == null ) throw new ArgumentNullException("sourceType");            
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException("destinationType");            
            return (destinationType == typeof(LocalizedString)) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {            
            if (value == null) return null;
            if (culture == null) culture = Thread.CurrentThread.CurrentCulture;

            if (value.GetType() != typeof(string)) throw new ArgumentOutOfRangeException("value");


            return new LocalizedString((string) value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException("destinationType");            
            if (value == null) return null;
            if (culture == null) culture = Thread.CurrentThread.CurrentCulture;             

            return ((LocalizedString) value).GetValue(culture);
        }
    }
}