using Localization.Net.Configuration;

namespace Localization.Net.Web.Mvc
{
    internal static class LocalizationHelper
    {
        /// <summary>
        /// Gets the current text manager.
        /// </summary>
        internal static TextManager TextManager
        {
            //TODO: This may be wrong in a DC context. This common utility method makes refactoring easy
            get {
                return LocalizationConfig.TextManager;
                //TODO:
                //return DependencyResolver.Current.GetService<TextManager>(); 
            }
        }
    }
}
