using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Localization.Net.Web.Mvc.Controllers
{
    public class ClientSideScriptController : Controller
    {

        public ActionResult Index(string namespaces, string keyFilters, string var, string handler, string defaultNamespace, bool gzip = true)
        {
            

            var nsList =
                string.IsNullOrEmpty(namespaces) || namespaces == "default" ? new[] {LocalizationHelper.TextManager.DefaultNamespace}
                    : namespaces == "all" ? new string[0]
                        : namespaces.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();

            bool jsonp = handler != null;
            if( string.IsNullOrWhiteSpace(handler) && string.IsNullOrWhiteSpace(var) )
            {
                throw new ArgumentNullException("Either a var name or jsonp handler must be specified. (e.g. ?var=L10n or ?handler=loadTexts)");
            }

            Regex nsFilter = null, keyFilter = null;

            if( nsList.Any() )
            {
                if (defaultNamespace == null)
                {
                    defaultNamespace = nsList[0];
                }

                nsFilter = new Regex("^(" + string.Join("|", nsList.Select(Regex.Escape)) + ")$", RegexOptions.IgnoreCase);
            } 
                        

            var keyFilterList = keyFilters.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
            if( keyFilterList.Any() )
            {
                keyFilter = new Regex("^(" + string.Join("|",                     
                    keyFilterList.Select(kf=>Regex.Escape(kf).Replace("_", ".*"))) 
                        + ")", RegexOptions.IgnoreCase);
                
            }

            Func<string, string, bool> filter = (ns, key) =>
                (nsFilter == null || nsFilter.IsMatch(ns)) && (keyFilter == null || keyFilter.IsMatch(key));

            return new LocalizedTextJavascriptResult(handler ?? var, null, defaultNamespace, filter, gzip, jsonp);
        }
    }
}
