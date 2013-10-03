using System;
using System.Collections.Generic;
using System.Reflection;
using Localization.Net.Exceptions;

namespace Localization.Net.Support
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Converts an object to a parameter set.
        /// </summary>
        /// <param name="data">The object to convert.</param>
        /// <param name="addWithIndex">if set to <c>true</c> values are also added by index, e.g. new { Test = 10 } will be added both as "Test" and "0".</param>
        /// <returns></returns>
        public static ParameterSet ParamsToParameterSet(object data, bool addWithIndex = false)
        {
            if( data is string )
            {
                throw new LocalizedArgumentException("ObjectHelper.ParamsToParameterSet.DataCannotBeString");
            }

            object[] array = data as object[];
            if (array != null)
            {
                var numberedValues = new Dictionary<string, object>();
                if (data != null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        numberedValues.Add("" + i, array[i]);
                    }
                }

                return new DicitionaryParameterSet(numberedValues);
            }
            else
            {                
                var paramSet = data as ParameterSet;
                if (paramSet != null)
                {
                    return paramSet;
                }
                else
                {

                    var dictionary = data as IDictionary<string, object>;
                    if (dictionary != null)
                    {                        
                        return new DicitionaryParameterSet(dictionary);
                    }
                    else
                    {                        
                        var parameters = new DicitionaryParameterSet();                        
                        if (data != null)
                        {
                            int i = 0;

                            var attr = BindingFlags.Public | BindingFlags.Instance;
                            foreach (var property in data.GetType().GetProperties(attr))
                            {
                                if (property.CanRead)
                                {
                                    var val = property.GetValue(data, null);
                                    parameters.SetObject(property.Name, val);
                                    if (addWithIndex)
                                    {
                                        parameters.SetObject("" + i, val);
                                    }
                                    ++i;
                                }                                
                            }                            
                        }
                        return parameters;  
                    }
                }
            }
        }

        public static object Eval(object container, string propertyOrField)
        {
            if (container == null) throw new LocalizedArgumentNullException("container");

            var type = container.GetType();
            var prop = type.GetProperty(propertyOrField);
            if (prop == null)
            {
                //Field?
                var field = type.GetField(propertyOrField);
                if (field == null)
                {
                    throw new ArgumentException(string.Format("{0} is not a property or field of type {1}",
                        propertyOrField, type.FullName));
                }

                return field.GetValue(container);
            }


            return prop.GetValue(container, null);
        }
    }
}
