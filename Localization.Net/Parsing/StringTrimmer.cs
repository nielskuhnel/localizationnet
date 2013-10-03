using System.Linq;

namespace Localization.Net.Parsing
{
    /// <summary>
    /// Trims excessive white space in text, parameters etc.
    /// </summary>
    public class StringTrimmer : DescendingPatternVisitor<object>
    {
        public override void Visit(ParameterSpec spec, object state)
        {
            spec.ParameterFormat = spec.ParameterFormat.Trim();
            spec.ParameterName = spec.ParameterName.Trim();

            base.Visit(spec, null);
        }

        public override void Visit(Text text, object state)
        {            
            base.Visit(text, null);
        }
        
        public override void Visit(Switch sw, object state)
        {
            sw.ParameterName = sw.ParameterName.Trim();

            TrimQuotedExpression(sw.NullExpression);

            base.Visit(sw, null);
        }

        public override void Visit(SwitchCase sc, object state)
        {
            TrimQuotedExpression(sc.Expression);                 

            base.Visit(sc, null);
        }

        public override void Visit(FormatGroup group, object state)
        {
            if (group.Expression != null)
            {
                TrimQuotedExpression(group.Expression);
            }
            base.Visit(group, state);
        }

        void TrimQuotedExpression(Expression expr)
        {
            if (expr == null)
            {
                return;
            }
            //If the text of a case starts/with '"' whitespace is significant

            var ps = expr.Parts;
            if (ps.Any())
            {
                var firstText = ps.First() as Text;
                var lastText = ps.Last() as Text;

                //Trim first text's leading whitespace
                if (firstText != null)
                {
                    firstText.Spelling = firstText.Spelling.TrimStart();
                    if (firstText.Spelling.StartsWith("\""))
                    {
                        firstText.Spelling = firstText.Spelling.Substring(1);
                    }
                }

                //Trim last text's trailing whitespace
                if (lastText != null)
                {
                    lastText.Spelling = lastText.Spelling.TrimEnd();
                    if (lastText.Spelling.EndsWith("\""))
                    {
                        lastText.Spelling = lastText.Spelling.Substring(0, lastText.Spelling.LastIndexOf('"'));
                    }
                }
            }   
        }
    }
}
