using System;
using System.Reflection;
using Localization.Net;
using Localization.Net.Exceptions;
using Localization.Net.Maintenance;

namespace Localization.Net.Configuration
{
       
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public class LocalizationXmlSourceAttribute : LocalizationSourceAttribute
    {

        /// <summary>
        /// Gets or sets the name of the resource. The resource name is evaluated using EndsWith so the assembly's namespace is not required in the resource name
        /// </summary>
        /// <value>
        /// The name of the resource.
        /// </value>
        public string ResourceName { get; set; }

        public LocalizationXmlSourceAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        public override ITextSource GetSource(Assembly asm, TextManager textManager, string targetNamespace)
        {            
            var source = XmlTextSource.ForAssembly(asm, ResourceName);
            if (source != null)
            {
                source.DefaultNamespace = targetNamespace;
            }
            else
            {
                throw new LocalizedFileNotFoundException(ResourceName, "Exceptions.ResourceNotFoundException",
                    defaultMessage: "The resource {0} could not be opened");
            }
            return source;
        }
    }
}
