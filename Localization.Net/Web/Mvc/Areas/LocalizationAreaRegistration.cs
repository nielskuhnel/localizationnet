using System.Web.Mvc;
using Localization.Net.Web.Mvc.Controllers;

namespace Localization.Net.Web.Mvc.Areas
{
    public class LocalizationAreaRegistration : AreaRegistration
    {
        readonly string _areaName;

        public LocalizationAreaRegistration(string areaName = null)
        {
            _areaName = areaName ?? "Localization";
        }


        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.Routes.MapRoute("Localization_Net_ClientLocalization",
                AreaName + "/script/{namespaces}/{keyFilters}",
                new {                        
                        action = "Index",
                        controller = "ClientSideScript", 
                        namespaces="", 
                        keyFilters=""
                    }, null, new [] { typeof(ClientSideScriptController).Namespace})
                
              .DataTokens.Add("area", AreaName);            
        }

        public override string AreaName
        {
            get { return _areaName; }
        }
    }
}