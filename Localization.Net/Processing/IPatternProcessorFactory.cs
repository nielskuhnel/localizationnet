namespace Localization.Net.Processing
{
    /// <summary>
    /// Factory interface used by switch cases, parameter evaluators and formatters
    /// </summary>    
    public interface IPatternProcessorFactory<TInterface, TArg>
    {    
        /// <summary>
        /// If the pattern processor created by the factory supports the given representation it an instance is returned. Null otherwise
        /// </summary>
        /// <param name="rep"></param>
        /// <returns></returns>
        TInterface GetFor(TArg rep, PatternDialect dialect, TextManager manager);
    }
}