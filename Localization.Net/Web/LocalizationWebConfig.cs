using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Reflection;
using Localization.Net.Maintenance;
using Localization.Net.Web.Mvc;
using Localization.Net.Web.Mvc.Areas;
using Localization.Net.Configuration;

namespace Localization.Net.Web
{
    public static class LocalizationWebConfig
    {
        public static TextManager ApplyDefaults<TWebApp>(TextManager manager, string overridesPath = null)
        {
            return ApplyDefaults(typeof(TWebApp).Assembly, manager, overridesPath);
        }

        public static TextManager ApplyDefaults(Assembly appAssembly, TextManager manager, string overridesPath = null)
        {
            overridesPath = overridesPath ?? "~/App_Config/LocalizationEntries.xml";

            manager.StringEncoder = (s) =>
                HttpUtility.HtmlEncode(s ?? "")
                .Replace("\r\n", "\n") // Windows to UNIX
                .Replace("\r", "<br />") //Mac
                .Replace("\n", "<br />") //Other
            ;
                        
            manager.MissingTextHandler = (ns, key, lang, fallback) => !string.IsNullOrWhiteSpace(fallback)
                ? fallback
                : "(" + (string.IsNullOrEmpty(ns) || ns == manager.DefaultNamespace ? "" : ns + ".") + key + ")";                        

            string defaultNamespace = manager.GetNamespace(appAssembly);
            manager.DefaultNamespace = defaultNamespace;
            
            var overridePath = HostingEnvironment.MapPath(overridesPath);
            if (File.Exists(overridePath))
            {
                manager.Texts.Sources.Add(
                    new PrioritizedTextSource(XmlTextSource.Monitoring(appAssembly, manager, overridePath), 1));
            }

            manager.PrepareTextSources(defaultNamespace);

            return manager;
        }

        public static DefaultTextManager SetupDefaultManager<TApp>()
        {
            return SetupDefaultManager(typeof(TApp).Assembly);
        }

        public static DefaultTextManager SetupDefaultManager(Assembly appAssembly, string overridesPath = null)
        {
            return (DefaultTextManager)ApplyDefaults(appAssembly, LocalizationConfig.SetupDefault(), overridesPath);
        }

        public static void SetupMvcDefaults(bool setupValidation = true, bool setupMetadata = true)
        {            
            var manager = LocalizationHelper.TextManager;
            if (setupValidation)
            {
                ModelBinders.Binders.DefaultBinder = new LocalizingDefaultModelBinder(manager);

                DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory((meta, ctx, attr) =>
                    new LocalizingDataAnnotationsModelValidator(meta, ctx, attr));

                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(RangeAttribute), (meta, ctx, attr) =>
                    new LocalizingRangeAttributeAdapater(meta, ctx, attr));

                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(RegularExpressionAttribute), (meta, ctx, attr) =>
                    new LocalizingRegularExpressionAttributeAdapater(meta, ctx, attr));

                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(RequiredAttribute), (meta, ctx, attr) =>
                    new LocalizingRequiredAttributeAdapater(meta, ctx, attr));

                DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(typeof(StringLengthAttribute), (meta, ctx, attr) =>
                    new LocalizingStringLengthAttributeAdapater(meta, ctx, attr));
            }

            if (setupMetadata)
            {
                ModelMetadataProviders.Current = new LocalizingModelMetadataProvider();
            }
        }

        public static void RegisterRoutes(RouteCollection routes, string areaName = "Localization")
        {
            var csArea = new LocalizationAreaRegistration(areaName);
            csArea.RegisterArea(
                new AreaRegistrationContext(csArea.AreaName, routes));
        }
    }
}
