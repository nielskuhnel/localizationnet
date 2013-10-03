using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace Localization.Net.Web.Mvc
{
    public static class ResourceHelper
    {

        public static string GetUrl<TNamespace>(string key, object values = null,
            LanguageInfo language = null)
        {
            return GetUrl(key, values, language, type: typeof(TNamespace));        
        }        

        public static string GetUrl(string key, object values = null,
            LanguageInfo language = null, string ns = null, Type type = null)
        {

            var manager = LocalizationHelper.TextManager;

            if (type != null)
            {
                ns = manager.GetNamespace(type.Assembly);
            }
            

            //Here we need the actual text entry to get Source.ReferenceAssembly
            var entry = manager.GetTextEntry(ns ?? manager.DefaultNamespace, key, language ?? manager.GetCurrentLanguage(), true);
            if (entry != null)
            {
                //The entry is non null. So will the text be.
                var pathSpecifier = LocalizationHelper.TextManager.Get(key, values, ns: ns, language: language, returnNullOnMissing: true);
                if (pathSpecifier.StartsWith("resource:"))
                {
                    pathSpecifier = pathSpecifier.Substring(9);

                    Assembly asm = null;
                    if (entry.Text.Source != null)
                    {
                        asm = entry.Text.Source.ReferenceAssembly;
                    }
                    var parts = pathSpecifier.Split(';');
                    string resourceName = parts[0];
                    if (parts.Length > 1)
                    {
                        asm = TypeFinder.GetFilteredLocalAssemblies(exclusionFilter: TextManager.KnownAssemblyExclusionFilter).FirstOrDefault(x => x.GetName().Name == parts[1]);
                    }

                    if (asm == null)
                    {
                        throw new AssemblyNotFoundException();
                    }
                    
                    //TODO: This ought to be done better                    
                             
                    var resourceUrl = new Page().ClientScript.GetWebResourceUrl(asm.GetTypes()[0], resourceName);

                    return VirtualPathUtility.ToAbsolute(resourceUrl);
                }
                else
                {
                    return VirtualPathUtility.ToAbsolute(pathSpecifier);
                }
            }
            return null;
        }

    }
}
