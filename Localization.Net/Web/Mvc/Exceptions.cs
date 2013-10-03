using System;

namespace Localization.Net.Web.Mvc
{
    [Serializable]
    public class AssemblyNotFoundException : Exception
    {
        public global::Localization.Net.ExceptionHelper Localization { get; private set; }

        public AssemblyNotFoundException(string key = "Exceptions.AssemblyNotFoundException", object parameters = null, Exception innerException = null)            
        {
            Localization = new global::Localization.Net.ExceptionHelper(
                this, key, "The assembly for the resource specifier could not be found.", parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}
