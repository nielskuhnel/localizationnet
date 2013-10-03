using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Localization.Net.Web.Mvc
{
    /// <summary>
    /// Localizes display names.
    /// If a name is specified it's considered a key
    /// 
    /// If a display name is not specified for a property the following keys are considered in this order:
    /// - Namespace . Class name . Property name
    /// - Namespace part after the assembly's default namespace . Class name . Property name (e.g. the property "Name" of the class "Acme.Web.Models.CoreEntityModel" becomes Models.CoreEntityModel.Name)
    /// - Class name . Property name
    /// - Property name (if TestSimplePropertyName is true)
    /// 
    /// </summary>
    public class LocalizingModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        /// <summary>
        /// Gets or sets a value indicating whether the TextManager's missing text format should be used or if the display name that could not be localized should just be used.
        /// You may consider this "strict mode" as it requires every property label to be localized        
        /// Default is <c>true</c>
        /// </summary>
        /// <value>
        ///   <c>false</c> if TextManager's missing text format is used
        /// </value>
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property name without any prefix should be tested as a key if no other localization is found. 
        /// As this may collide with other texts it can be disabled. It is not by default.       
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the property name without prefix is tested as key; otherwise, <c>false</c>.
        /// </value>
        public bool TestSimplePropertyName { get; set; }

        public LocalizingModelMetadataProvider()
        {
            IgnoreMissing = true;
            TestSimplePropertyName = true;
        }        


        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            #region Label

                var hasLabelMatch = false;

                if (!string.IsNullOrEmpty(metadata.DisplayName))
                {
                    var text = LocalizationHelper.TextManager.Get(metadata.DisplayName,
                        callingAssembly: containerType.Assembly,
                        returnNullOnMissing: IgnoreMissing,
                        encode: false);

                    if (text != null)
                    {
                        hasLabelMatch = true;
                        metadata.DisplayName = text;
                    }
                }

                if (containerType != null && !hasLabelMatch)
                {
                    //Se if one of the key conventions are present                

                    foreach (var key in GetConventionKeyNames(containerType, propertyName, TestSimplePropertyName))
                    {
                        var match = LocalizationHelper.TextManager.Get(key,
                            callingAssembly: containerType.Assembly,
                            returnNullOnMissing: true,
                            encode: false);
                        if (match != null)
                        {
                            metadata.DisplayName = match;
                            break;
                        }
                    }
                }

            #endregion

            #region Description

                var hasDescriptionMatch = false;

                if (!string.IsNullOrEmpty(metadata.Description))
                {
                    var text = LocalizationHelper.TextManager.Get(metadata.Description,
                        callingAssembly: containerType.Assembly,
                        returnNullOnMissing: IgnoreMissing,
                        encode: false);

                    if (text != null)
                    {
                        hasDescriptionMatch = true;
                        metadata.Description = text;
                    }
                }

                if (containerType != null && !hasDescriptionMatch)
                {
                    //Se if one of the key conventions are present                

                    foreach (var key in GetConventionKeyNames(containerType, propertyName, TestSimplePropertyName).Select(x => x + ".Description"))
                    {
                        var match = LocalizationHelper.TextManager.Get(key,
                            callingAssembly: containerType.Assembly,
                            returnNullOnMissing: true,
                            encode: false);
                        if (match != null)
                        {
                            metadata.Description = match;
                            break;
                        }
                    }
                }

            #endregion

            

            return metadata;
        }


        public static IEnumerable<string> GetConventionKeyNames(Type containerType, string propertyName, bool testSimplePropertyName)
        {
            //TODO: Move this somewhere else. It defines default naming conventions for keys

            var prefixes = new List<string>();
            var ns = containerType.Namespace;
            prefixes.Add(ns + "." + containerType.Name + ".");
            string defaultNs = containerType.Assembly.GetName().Name;
            if (ns != defaultNs && ns.StartsWith(defaultNs))
            {
                prefixes.Add(ns.Remove(0, defaultNs.Length + 1) + "." + containerType.Name + ".");
            }
            prefixes.Add(containerType.Name + ".");
            if (testSimplePropertyName)
            {
                prefixes.Add("");
            }
            return prefixes.Select(x=>x + propertyName);
        }
    }
}
