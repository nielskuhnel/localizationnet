namespace Localization.Net.Processing
{    
    /// <summary>
    /// Classes implementing this interface are used to evaluate if a value matches the condition on switch cases
    /// </summary>
    public interface ISwitchConditionEvaluator
    {
        /// <summary>
        /// Evaluates the value of the object against the condition and returns true if it matches
        /// </summary>
        /// <param name="o">The value to evaluate</param>
        /// <returns>true if the value matches the condition</returns>
        bool Evaluate(ParameterValue o, EvaluationContext context);
    }    

    
    
}
