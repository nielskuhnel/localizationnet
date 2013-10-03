using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Localization.Net.Web.Mvc
{
    public class ExceptionHelper
    {
        /// <summary>
        /// Gets the name of the type like "integer", "number" "date" etc..
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static string GetCommonTypeName(Type t)
        {
            if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64) || t == typeof(UInt16) || t == typeof(UInt32) || t == typeof(UInt64))
            {
                return "Integer";
            }
            else if (t == typeof(Single) || t == typeof(Double) || t == typeof(Decimal))
            {
                return "Number";
            }
            else
            {
                return t.Name;
            }
        }


        public static string LocalizeValidationException(TextManager textManager, Exception ex, ModelMetadata metadata, 
                LocalizedValidationAttribute localizationInfo = null,
                ValueProviderResult value = null)
        {

            string key;

            var values = new Dictionary<string, object>();

            values["Property"] = metadata.PropertyName;
            if (!metadata.HideSurroundingHtml) //If this is true it is assumed that the model doesn't have a label
            {                
                values["DisplayName"] = metadata.GetDisplayName();
            }

            var type = (Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType);

            values["TypeKey"] = "Validation.Types." + (values["TypeName"] = type.Name);
            values["CommonTypeKey"] = "Validation.Types." + (values["CommonTypeName"] = GetCommonTypeName(type));

            var validationException = ex as ValidationException;
            if (validationException != null)
            {                
                values["Value"] = validationException.Value;
                var attr = validationException.ValidationAttribute;
                var attrType = attr.GetType();
                key = attrType.Name.Replace("Attribute", "");
                foreach (var prop in attrType.GetProperties())
                {
                    values[prop.Name] = prop.GetValue(attr, null);
                }
            }
            else
            {
                values["Value"] = value != null ? value.AttemptedValue : metadata.Model;
                key = ex.GetType().Name;
            }

            var asm = metadata.ContainerType.Assembly;
            string ns = asm != null ?
                textManager.GetNamespace(metadata.ContainerType.Assembly) : null;


            if (localizationInfo != null)
            {
                key = localizationInfo.Key;
                ns = localizationInfo.Namespace ?? ns;
                return textManager.Get(key, values, encode: false, ns: ns);
            }
            else
            {
                var keys = 
                    LocalizingModelMetadataProvider.GetConventionKeyNames(metadata.ContainerType, metadata.PropertyName, true)
                        .Select(x => Tuple.Create(ns, x + "." + key))
                        .Concat(new [] {Tuple.Create(textManager.DefaultNamespace, "Validation." + key)});  

                foreach (var conventionKey in keys)
                {
                    var text = textManager.Get(
                        ns: conventionKey.Item1,                        
                        key: conventionKey.Item2, 
                        values: values,                        
                        returnNullOnMissing: true,
                        encode: false);
                    if (text != null)
                    {
                        return text;
                    }
                }
            }

            return null;
        }
    }
}
