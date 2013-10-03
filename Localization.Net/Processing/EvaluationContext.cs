using System;

namespace Localization.Net.Processing
{
    /// <summary>
    /// This class contains the language and namespace that pattern parameter evaluators etc. should evaluate against
    /// </summary>
    public class EvaluationContext
    {
        public LanguageInfo Language { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; }

        public string Namespace { get; set; }
        
        public ParameterSet Parameters { get; set; }

        public Func<string,string> StringEncoder { get; set; }

        public EvaluationContext Clone()
        {
            return (EvaluationContext)this.MemberwiseClone();
        }
    }
}
