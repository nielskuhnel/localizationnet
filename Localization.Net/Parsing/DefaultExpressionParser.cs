using System;
using System.Linq;
using System.Text;
using System.IO;

namespace Localization.Net.Parsing
{
    public class DefaultExpressionParser : ExpressionParser
    {

        const char Eof = '\0';

        /// <summary>
        /// If these chars are escaped in texts they will not be misinterpreted
        /// </summary>
        internal static readonly string EscapeChars = "{}|#<>:";

        internal static readonly string StoppersText = "{}|#<>";

        internal static readonly string StoppersSwitchCase = "{}|:";
        internal static readonly string StoppersTemplatedSwitchName = "(:{";

        internal static readonly string StoppersParameterName = "{}|:<>";
        internal static readonly string StartersParameterNameArgs = "(:}";
        internal static readonly string StoppersParameterNameArgs = ")";
        internal static readonly string StoppersParameterNameTemplatedSwitch = "{}()";

        internal static readonly string StoppersParameterFormat = "{}";
        internal static readonly string StoppersParameterFormatTemplatedSwitch = "{}()";


        string _reader;
        char _current;
        int _pos;

        public DefaultExpressionParser()
        {
        }

        public static string Escape(string s)
        {
            var escaped = new StringBuilder();
            foreach (var p in s)
            {
                if (EscapeChars.Contains(p))
                {
                    escaped.Append("\\").Append(p);
                }
                else
                {
                    escaped.Append(p);
                }
            }
            return escaped.ToString();
        }

        StringTrimmer _trimmer = new StringTrimmer();


        /// <summary>
        /// Parses the specified pattern returned by the reader and localizes error messages with the text manager specified
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="textManager">The text manager.</param>
        /// <returns></returns>
        public override Expression Parse(TextReader reader, TextManager textManager)
        {
            try
            {
                _reader = reader.ReadToEnd();

                _pos = -1;

                MoveNext();

                var expr = ParseExpression();
                if (_current != Eof)
                {
                    UnexpectedToken("Expression", "" + _current);
                }

                expr.Accept(_trimmer);

                return expr;
            }
            catch (InnerParserException template)
            {
                throw new SyntaxErrorException(template.Message, template.Construct, template.Pos);
            }
        }

        Expression ParseExpression()
        {
            var expr = new Expression();
            while (_current != Eof)
            {
                var part = ParseExpressionPart();
                if (part == null) break; //No further expression parts, i.e. _current \in stoppers or EOF

                expr.Parts.Add(part);
            }

            if (!expr.Parts.Any())
            {
                //Empty expression is an empty string
                expr.Parts.Add(new Text { Spelling = "" });
            }

            return expr;
        }

        ExpressionPart ParseExpressionPart()
        {
            switch (_current)
            {
                case '{': return ParseParameterSpec();
                case '#': return ParseSwitch();
                case '<': return ParseFormatGroup();
                default:
                    {
                        if (StoppersText.Contains(_current))
                        {
                            //A stopper that doesn't have a meaning in the current context was met
                            return null;
                        }
                        return ParseText();
                    }
            }
        }

        string ReadText(string stopChars)
        {
            //PRE: !EOF, _current is not stopper
            var sb = new StringBuilder();

            for (; ; )
            {
                if (_current == '\\')
                {
                    MoveNext();
                }
                else if (stopChars.Contains(_current))
                {
                    break;
                }
                sb.Append(_current);
                if (!MoveNext())
                {
                    break;
                }
            }

            return sb.ToString();
        }

        Text ParseText()
        {
            return new Text { Spelling = ReadText(StoppersText) };
        }

        string ReadParameterName()
        {
            return ReadText(StoppersParameterName);
        }

        string ReadParameterFormat()
        {
            return ReadText(StoppersParameterFormat);
        }

        ParameterSpec ParseParameterSpec()
        {
            //PRE: !EOF && _current == '{'
            var spec = new ParameterSpec();

            MoveNext(); //Take '{'      

            var lparen = LookAheadFor(StartersParameterNameArgs);
            if (lparen == '(')
            {
                spec.ParameterName = ReadText("(");
                MoveNext(); //Take '('
                spec.Arguments = ReadText(StoppersParameterNameArgs);
                MoveNext(); //Take ')'
            }
            else
            {
                spec.ParameterName = ReadParameterName();
            }
            if (_current == ':')
            {
                MoveNext(); //Take it
                spec.ParameterFormat = ReadParameterFormat();
            }
            else
            {
                spec.ParameterFormat = "";
            }

            if (_current != '}') ExpectedToken("ParameterSpec", "}");

            MoveNext(); //Take '}';

            return spec;
        }

        FormatGroup ParseFormatGroup()
        {
            var group = new FormatGroup();

            MoveNext(); //Take '<'

            group.ParameterName = ReadParameterName();
            if (_current == ':')
            {
                MoveNext(); //Take ':'
                group.Expression = ParseExpression();
            }
            if (_current != '>') ExpectedToken("FormatGroup", ">");
            MoveNext(); //Take '>'

            return group;
        }

        Switch ParseSwitch()
        {
            //PRE: !EOF && _current == '#'

            var sw = new Switch();

            MoveNext(); //Take '#'

            var lparen = LookAheadFor(StoppersTemplatedSwitchName);
            if (lparen == '(')
            {
                //The first stopper was a '('. Read the switch's template name
                sw.SwitchTemplateName = ReadText(StoppersParameterNameTemplatedSwitch);
                MoveNext(); // Take '('
                sw.ParameterName = ReadText(StoppersParameterNameTemplatedSwitch);
                if (_current == ':')
                {
                    MoveNext(); //Take ':'
                    sw.ParameterFormat = ReadText(StoppersParameterFormatTemplatedSwitch);
                }
                if (_current != ')')
                {
                    ExpectedToken("TemplatedSwitch", ")");
                }
                MoveNext(); // Take ')'
            }
            else
            {
                sw.ParameterName = ReadParameterName();

                if (_current == ':')
                {
                    MoveNext(); //Take ':'
                    sw.ParameterFormat = ReadParameterFormat();
                }
            }

            if (_current != '{') ExpectedToken("Switch", "{");
            MoveNext(); // Take '{'

            while (_current != '}')
            {
                if (_current == '?')
                {
                    MoveNext(); // Take '?'
                    TakeWhitespace();
                    sw.NullExpression = ParseExpression();
                }
                else
                {
                    TakeWhitespace();
                    var switchCase = new SwitchCase();
                    var stopper = LookAheadFor(StoppersSwitchCase);
                    if (stopper == ':')
                    {
                        switchCase.Condition = Expression.Text(ReadText(":"));
                        MoveNext(); //Take ':'                 
                    }

                    switchCase.Expression = ParseExpression();

                    sw.Cases.Add(switchCase);
                }

                if (_current == '|')
                {
                    MoveNext(); //Take '|'
                }
                else if (_current != '}')
                {
                    UnexpectedSwitchToken(sw.ParameterName, "" + _current);
                }
            }

            MoveNext(); // Take '}'                        

            return sw;
        }

        SwitchCase ParseSwitchCase()
        {
            return new SwitchCase();
        }


        bool TakeWhitespace()
        {
            while (char.IsWhiteSpace(_current))
            {
                if (!MoveNext()) return false;
            }

            return true;
        }


        char LookAheadFor(string chars)
        {
            var pos = _pos;
            while (pos < _reader.Length)
            {
                var p = _reader[pos++];
                if (p == '\\') //Escaped char
                {
                    pos += 2;
                }
                else
                {
                    if (chars.Contains(p))
                    {
                        return p;
                    }
                }
            }

            return Eof;
        }


        bool MoveNext()
        {
            ++_pos;

            if (_pos < _reader.Length)
            {
                _current = _reader[_pos];
            }
            else
            {
                _current = Eof;
            }

            return _current != Eof;
        }

        void UnexpectedToken(string construct, string token)
        {
            SyntaxError(construct, "Unexpected '{0}'", "DefaultExpressionParser.SyntaxError.UnexpectedToken", new { Token = token });
        }

        void UnexpectedSwitchToken(string parameterName, string token)
        {
            SyntaxError("Switch",
                        "Unexpected token '{0}'. Expected '}' or '|' (switch on {1})",
                        "DefaultExpressionParser.SyntaxError.UnexpectedSwitchToken", new { Token = "" + _current, SwitchOn = parameterName });
        }

        void ExpectedToken(string construct, string token)
        {
            SyntaxError(construct, "'{0}' expected", "DefaultExpressionParser.SyntaxError.ExpectedToken", new { Token = "" + token });
        }

        void SyntaxError(string construct, string defaultMessage, string messageKey, object messageParams)
        {
            throw new InnerParserException(construct, _pos, messageKey, defaultMessage, messageParams);
        }

        class InnerParserException : Exception
        {
            public ExceptionHelper Localization { get; private set; }

            public int Pos { get; set; }
            public string Construct { get; set; }

            public InnerParserException(string construct, int pos, string key, string defaultMessage, object parameters)
            {
                Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
                Pos = pos;
                Construct = construct;
            }

            public override string Message { get { return Localization.GetMessage(base.Message); } }
        }
    }
}
