using System;

namespace Localization.Net.Parsing
{
    //This allows pattern AST to be saved as xml. This is probably easier to manage from a wysiwyg editor
    public class XmlExpressionParser : ExpressionParser
    {

        public override Expression Parse(System.IO.TextReader reader, TextManager manager)
        {
            throw new NotImplementedException();
        }
    }
}
