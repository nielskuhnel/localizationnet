using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Localization.Net.Configuration;

namespace Localization.Net.Maintenance
{
    public class AssemblySourceLoader
    {
        /// <summary>
        /// Gets the text sources defined in the assembly specified
        /// </summary>
        /// <param name="asm">The assembly.</param>
        /// <param name="textManager">The text manager.</param>
        /// <param name="targetNamespace">The default namespace expected for the assembly when adding texts</param>
        /// <returns></returns>
        public static ITextSource GetTextSource(Assembly asm, TextManager textManager, string targetNamespace)
        {
            var sources = new List<LocalizationSourceAttribute>();


            sources.AddRange(asm.GetCustomAttributes(typeof(LocalizationSourceAttribute), true)
                .Cast<LocalizationSourceAttribute>());

            var source = new TextSourceAggregator();

            using (source.BeginUpdate())
            {
                //TODO: Is a default source needed?
                //The default source  
                try
                {                    
                    source.Sources.Add(
                        new PrioritizedTextSource(
                            new LocalizationXmlSourceAttribute(LocalizationConfig.DefaultXmlFileName).GetSource(asm, textManager, targetNamespace)));
                }
                catch
                {                                        
                }

                foreach (var attr in sources)
                {
                    var s = attr.GetSource(asm, textManager, targetNamespace);
                    if (s != null)
                    {
                        source.Sources.Add(
                            new PrioritizedTextSource(s, attr.Priority));
                    }
                }
            }

            return source.Sources.Any() ? source : null;
        }
    }
}
