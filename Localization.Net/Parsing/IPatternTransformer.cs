namespace Localization.Net.Parsing
{
    /// <summary>
    /// Implement this interface to change the default grammar's syntax.
    /// </summary>
    public interface IPatternTransformer
    {
        //TODO: Make a mechanism to update positions in error messages based on unencoded pattern
        string Encode(string pattern);
        string Decode(string encodedPattern);
    }
}
