using System;

namespace Localization.Net.Exceptions
{
    [Serializable]
    public class LocalizedArgumentNullException : ArgumentNullException
    {
        public ExceptionHelper Localization { get; private set; }

        public LocalizedArgumentNullException(string paramName, string key = "Exceptions.ArgumentNullException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(paramName, defaultMessage)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
            Localization.AddParameter("ParamName", paramName);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}