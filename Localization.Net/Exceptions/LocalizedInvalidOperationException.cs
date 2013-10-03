using System;

namespace Localization.Net.Exceptions
{
    [Serializable]
    public class LocalizedInvalidOperationException : InvalidOperationException
    {        
        public ExceptionHelper Localization { get; private set; }

        public LocalizedInvalidOperationException(string key = "Exceptions.InvalidOperationException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}