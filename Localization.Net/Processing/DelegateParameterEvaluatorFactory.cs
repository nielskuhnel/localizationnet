using System;
using Localization.Net.Parsing;

namespace Localization.Net.Processing
{
    public class DelegateParameterEvaluatorFactory : IParameterEvaluatorFactory
    {
        private Func<ParameterSpec, PatternDialect, TextManager, IParameterEvaluator> _factory;
        public DelegateParameterEvaluatorFactory(Func<ParameterSpec, PatternDialect, TextManager, IParameterEvaluator> factory)
        {
            _factory = factory;
        }

        public IParameterEvaluator GetFor(ParameterSpec spec, PatternDialect pattern, TextManager manager)
        {
            return _factory(spec, pattern, manager);
        }

        public static implicit operator DelegateParameterEvaluatorFactory(Func<ParameterSpec, PatternDialect, TextManager, IParameterEvaluator> factory)
        {
            return new DelegateParameterEvaluatorFactory(factory);
        }

    }
}