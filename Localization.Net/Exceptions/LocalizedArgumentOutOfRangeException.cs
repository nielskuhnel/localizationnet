using System;

namespace Localization.Net.Exceptions
{
    [Serializable]
    public class LocalizedArgumentOutOfRangeException : ArgumentOutOfRangeException
    {        
        public ExceptionHelper Localization { get; private set; }

        public LocalizedArgumentOutOfRangeException(string argument, object min, object max, string key = "Exceptions.ArgumentOutOfRangeException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            if (defaultMessage == null)
            {
                defaultMessage = "Value must be in the range {0} to {1}";
            }
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
            Localization.AddParameter("Minimum", min).AddParameter("Maximum", max);            
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}