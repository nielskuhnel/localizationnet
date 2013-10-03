using System;

namespace Localization.Net
{
    public class PatternException : Exception {

        public ExceptionHelper Localization { get; private set; }
        
        public PatternException(string key, string defaultMessage, object parameters, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);            
        }

        public override string Message
        {
            get
            {
                return Localization.GetMessage(base.Message);
            }
        }
    }

    public class SyntaxErrorException : PatternException
    {
        public string Construct { get; private set; }
        public int Pos { get; private set; }

        public SyntaxErrorException(string message, string construct, int pos) :
            base("PatternExceptions.SyntaxError", "{0} while parsing {1} at {2}",
                new { Message = message, Construct = construct, Pos = pos })
        {            
        }
    }

    public class UnkownDialectException : PatternException
    {
        public string Dialect { get; private set; }
        public UnkownDialectException(string dialect) :
            base("PatternExceptions.UnkownDialect", "Unkown dialect {0}", new { Dialect = dialect })
        {            
        }
    }  
    
}
