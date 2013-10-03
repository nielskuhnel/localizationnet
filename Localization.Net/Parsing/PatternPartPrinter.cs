using System.Linq;
using System.Text;

namespace Localization.Net.Parsing
{
    public class PatternPartPrinter : DescendingPatternVisitor<object>
    {

        StringBuilder _writer;

        public PatternPartPrinter()
        {
            _writer = new StringBuilder();
        }


        void WriteText(string text, string stoppers)
        {
            foreach (var p in text)
            {
                //Encode                                
                if (stoppers.Contains(p))
                {
                    _writer.Append('\\');
                }
                _writer.Append(p);
            }
        }

        public override void Visit(ParameterSpec spec, object state)
        {
            _writer.Append("{");
            WriteText(spec.ParameterName, DefaultExpressionParser.StoppersParameterName);
            _writer.Append(":");
            WriteText(spec.ParameterFormat, DefaultExpressionParser.StoppersParameterFormat);
            _writer.Append("}");            
        }

        public override void Visit(Switch sw, object state)
        {
            _writer.Append("#");

            if (!string.IsNullOrEmpty(sw.SwitchTemplateName))
            {
                WriteText(sw.SwitchTemplateName, DefaultExpressionParser.StoppersParameterNameTemplatedSwitch);
                _writer.Append("(");
                WriteText(sw.ParameterName, DefaultExpressionParser.StoppersParameterNameTemplatedSwitch);
                if (!string.IsNullOrEmpty(sw.ParameterFormat))
                {
                    WriteText(sw.ParameterFormat, DefaultExpressionParser.StoppersParameterFormatTemplatedSwitch);
                }
                _writer.Append(")");
            }
            else
            {
                WriteText(sw.ParameterName, DefaultExpressionParser.StoppersParameterName);
                if (!string.IsNullOrEmpty(sw.ParameterFormat))
                {
                    WriteText(sw.ParameterFormat, DefaultExpressionParser.StoppersParameterFormat);
                }
            }
            _writer.Append("{");
            bool first = true;
            foreach (var sc in sw.Cases)
            {
                if (first) first = false; else _writer.Append(" | ");
                sc.Accept(this);
            }

            if (sw.NullExpression != null)
            {
                _writer.Append(" |? ");
                sw.NullExpression.Accept(this);
            }

            _writer.Append("}");            
        }

        public override void Visit(SwitchCase sc, object state)
        {
            if (sc.Condition != null)
            {
                sc.Condition.Accept(this);
                _writer.Append(": ");
            }

            PrintQuotedExpression(sc.Expression);                                  
        }

        public override void Visit(FormatGroup group, object state)
        {
            _writer.Append("<");
            WriteText(group.ParameterName, DefaultExpressionParser.StoppersParameterName);
            _writer.Append(": ");
            if (group.Expression != null)
            {
                PrintQuotedExpression(group.Expression);
            }
            _writer.Append(">");
        }

        public override void Visit(Text text, object state)
        {
            WriteText(text.Spelling, DefaultExpressionParser.EscapeChars);            
        }


        void PrintQuotedExpression(Expression expr)
        {
            bool escape = false;

            var ps = expr.Parts;
            if (ps.Any() )
            {
                var firstText = ps.First() as Text;
                var lastText = ps.Last() as Text;

                //Escape if the expression starts/ends with whitespace or '"'
                escape = (firstText != null && !string.IsNullOrEmpty(firstText.Spelling) &&
                    (firstText.Spelling[0] == ' ' || firstText.Spelling[0] == '"')) ||

                    (lastText != null && !string.IsNullOrEmpty(lastText.Spelling) &&
                    (lastText.Spelling[lastText.Spelling.Length - 1] == ' ' || lastText.Spelling[lastText.Spelling.Length - 1] == '"'));                   
            }

            if (escape) _writer.Append("\"");
            expr.Accept(this);
            if (escape) _writer.Append("\"");            
        }

        public override void Visit(CustomExpressionPart part, object state)
        {
            WriteText(part.ToString(), "");

            base.Visit(part, state);
        }

        public override string ToString()
        {
            return _writer.ToString();
        }
    }
}
