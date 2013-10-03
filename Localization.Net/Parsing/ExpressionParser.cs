using System.IO;
using System.Globalization;

namespace Localization.Net.Parsing
{   
    public abstract class ExpressionParser
    {
        protected ExpressionParser() { }

        public CultureInfo PatternCulture = CultureInfo.GetCultureInfo("en-US");

        public virtual Expression Parse(string pattern, TextManager manager)
        {
            return Parse(new StringReader(pattern), manager);
        }

        public abstract Expression Parse(TextReader reader, TextManager manager);        

    }
}
