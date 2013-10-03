using System;
using System.IO;
using System.IO.Compression;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Localization.Net.Web.JavaScript;

namespace Localization.Net.Web.Mvc
{
    /// <summary>
    /// Generates JavaScript to use localization patterns client-side.
    /// </summary>
    public class LocalizedTextJavascriptResult : ViewResult
    {
        public string ClientClassName { get; set; }
        public LanguageInfo Language { get; set; }
        public string DefaultNamespace { get; set; }
        public Func<string, string, bool> Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to gzip compress the script.
        /// </summary>        
        public bool Gzip { get; set; }//TODO: Should be true in any case but <=IE6

        public bool JsonP { get; set; }

        
        public LocalizedTextJavascriptResult(
            string clientClassName, 
            LanguageInfo language = null,             
            string defaultNamespace = null, 
            Func<string,string,bool> filter = null,             
            bool gzip = true,
            bool jsonp = false)
        {
            ClientClassName = clientClassName;
            Language = language;
            DefaultNamespace = defaultNamespace;
            Filter = filter;
            Gzip = gzip;
            JsonP = jsonp;
        }

        /// <summary>
        /// Creates an LocalizedTextJavascriptResult filtered to the texts from the specified type's assembly and default namespaces.
        /// </summary>        
        public static LocalizedTextJavascriptResult Create<TAssemblyRef>(
            string clientClassName,
            LanguageInfo language = null, 
            Func<string, bool> keyFilter = null,
            Func<string, string, bool> filter = null,
            bool gzip = true)
        {
            var tm = LocalizationHelper.TextManager;
            var defaultNamespace = tm.GetNamespace(typeof (TAssemblyRef).Assembly);

            keyFilter = keyFilter ?? ((key) => true);
            filter = filter ?? ((ns, key) => ns == defaultNamespace && keyFilter(key));           

            return new LocalizedTextJavascriptResult(clientClassName, language, defaultNamespace, filter, gzip);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            //TODO: Cache header!

            //NOTE: compression could be removed and this could be registered with ClientDependency

            var response = context.HttpContext.Response;
            response.ContentType = "text/javascript; charset=utf8";
            if( Gzip )
            {
                response.AppendHeader("Content-Encoding", "gzip");
            }

            var output = response.OutputStream;
            if( Gzip )
            {
                output = new GZipStream(output, CompressionMode.Compress);
            }

            using (var tw = new StreamWriter(output))
            {
                if( JsonP ) tw.Write(ClientClassName + "((function() {");
                tw.Write(
                    LocalizationHelper.TextManager.WriteScript(JsonP ? "_" : ClientClassName , Language, DefaultNamespace, Filter, false).ToString());

                if( JsonP ) tw.Write("return _;})(), " + new JavaScriptSerializer().Serialize(DefaultNamespace) + ");");
            }
        }
    }
}
