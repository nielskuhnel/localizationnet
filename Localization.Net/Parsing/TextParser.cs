namespace Localization.Net.Parsing
{
    /// <summary>
    /// A very simple parser that treats the entire pattern as text
    /// </summary>
    public class TextParser : ExpressionParser
    {
        public override Expression Parse(System.IO.TextReader reader, TextManager manager)
        {
            var expr = new Expression();
            expr.Parts.Add(new Text { Spelling = reader.ReadToEnd() });

            return expr;
        }
    }
}
