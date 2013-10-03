using System;

namespace Localization.Net.Processing
{
    public class DelegateValueFormatterFactory: IValueFormatterFactory
    {
        private Func<string, PatternDialect, TextManager, IValueFormatter> _factory;
        public DelegateValueFormatterFactory(Func<string, PatternDialect, TextManager, IValueFormatter> factory)
        {
            _factory = factory;
        }
        
        public IValueFormatter GetFor(string rep, PatternDialect pattern, TextManager manager)
        {
            return _factory(rep, pattern, manager);
        }

        public static implicit operator DelegateValueFormatterFactory(Func<string, PatternDialect, TextManager, IValueFormatter> factory)
        {
            return new DelegateValueFormatterFactory(factory);
        }

    }
}