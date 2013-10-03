using System;

namespace Localization.Net.Exceptions
{
    [Serializable]
    public class LocalizedNotSupportedException : NotSupportedException
    {        
        public ExceptionHelper Localization { get; private set; }

        public LocalizedNotSupportedException(string key = "Exception.NotSupportedException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}