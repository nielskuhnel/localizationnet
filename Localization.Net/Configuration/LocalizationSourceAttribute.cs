using System;
using System.Reflection;
using Localization.Net;
using Localization.Net.Maintenance;

namespace Localization.Net.Configuration
{
    /// <summary>
    /// Represent a source for translations for an assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public abstract class LocalizationSourceAttribute : Attribute
    {
        public int Priority { get; set; }


        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <param name="asm">The assembly to load from.</param>
        /// <param name="textManager">The text manager.</param>
        /// <param name="targetNamespace">The default namespace expected for the assembly when adding texts</param>
        /// <returns></returns>
        public abstract ITextSource GetSource(Assembly asm, TextManager textManager, string targetNamespace);
    }
}
