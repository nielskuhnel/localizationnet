using System;

namespace Localization.Net.Exceptions
{
    [Serializable]
    public class LocalizedArgumentException : ArgumentException
    {        
        public ExceptionHelper Localization { get; private set; }

        public LocalizedArgumentException(string key = "Exceptions.ArgumentException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}