using System.Collections.Generic;
using System.Linq;
using Localization.Net.Parsing;
using Localization.Net.Processing.ParameterValues;

namespace Localization.Net.Processing.ParameterEvaluators
{
    //TODO: Refactor? Maybe pattern references should have their own grammatical construct instead of this
    //TODO: Prevent cyclic lookups? Can be difficult as switches may be involved

    /// <summary>
    /// Executes another pattern. Takes the format @OtherPatternKey or @OtherPatternKey(Parameter1, Parameter2, ... ParameterN)
    /// The specified parameters are passed on from the current parameters both with names and index. 
    /// The latter provides a way to reuse the same pattern even though parameter names may be different where it's called.
    /// In that case the pattern should only use parameter numbers as in: Special pattern says "Hello {0}"
    /// 
    /// If the key of the pattern to lookup is prefixed with '@' the name of the pattern is given by a parameter. For example @@PatternName(Parameter)
    /// If a parameter name is quoted ("My value" or 'My value') it is considered a string literal that is passed to the referenced pattern
    /// Parameters can be omitted to maintain ordinal positions, e.g. @Key(P1,,P3,,,"foo")
    /// </summary>
    public class PatternLookupEvaluator : IParameterEvaluator
    {
        public string PatternKey { get; set; }
        
        public TextManager Manager { get; set; }

        public KeyValuePair<string, IValueFormatter>[] Parameters { get; set; }

        public string NamespaceQualifier { get; set; }
        

        public PatternLookupEvaluator()
        {
            NamespaceQualifier = "__";
        }

        //If another namespace is needed for the pattern lookup it can be written as Namespace__Key, i.e. namespace and key seperated by two underscores '_'
        public ParameterValue GetValue(EvaluationContext context)
        {            
            var values = context.Parameters;


            string actualPatternKey;
            if (PatternKey.StartsWith("@"))
            {
                var parts = PatternKey.Substring(1).Split('+');
                actualPatternKey = (string)values.GetObject(parts[0]);
                if (parts.Length > 1)
                {
                    actualPatternKey += parts[1];
                }                
            }
            else
            {
                actualPatternKey = PatternKey;
            }
            
            string ns = context.Namespace;

            int ix = actualPatternKey.IndexOf(NamespaceQualifier);
            if (ix != -1)
            {
                ns = actualPatternKey.Substring(0, ix);
                actualPatternKey = actualPatternKey.Substring(ix + NamespaceQualifier.Length);
            }            

            if (actualPatternKey != null)
            {
                int i = 0;
                var callValues = new DicitionaryParameterSet();                
                foreach (var p in Parameters)
                {                    
                    
                    if ((p.Key.StartsWith("\"") || p.Key.StartsWith("'")) && (p.Key.EndsWith("\"") || p.Key.EndsWith("'")))
                    {
                        callValues[""+i] = ParameterValue.Wrap(p.Key.Substring(1, p.Key.Length - 2));                        
                    }
                    else
                    {
                        ParameterValue v = null;
                        if (p.Value != null)
                        {
                            v = values[p.Key].Clone();
                            v.DefaultFormat = p.Value;
                        }
                        else
                        {
                            v = values[p.Key];
                        }
                        
                        callValues[p.Key] = v;
                        callValues["" + i] = v;
                    }
                    ++i;
                }                

                return new UnencodedParameterValue(Manager.Get(actualPatternKey, callValues,
                    ns: ns, language: context.Language, returnNullOnMissing: true));
            }
            else
            {
                //The pattern key was to be looked up and wasn't in the provided values. Return null
                return null;
            }
        }
    }

    public class PatternLookupEvaluatorFactory : IParameterEvaluatorFactory
    {
        

        public IParameterEvaluator GetFor(ParameterSpec spec, PatternDialect dialect, TextManager manager)
        {                        
            
            if( spec.ParameterName.StartsWith("@") )
            {
                return new PatternLookupEvaluator
                {
                    Manager = manager,
                    PatternKey = spec.ParameterName.Substring(1),
                    Parameters = spec.Arguments != null ? spec.Arguments.Split(',')
                        .Select(x =>
                        {
                            var parts = x.Trim().Split(':');
                            if (parts.Length > 1)
                            {
                                return new KeyValuePair<string, IValueFormatter>(parts[0], dialect.GetValueFormatter(parts[1], manager));
                            }
                            else
                            {
                                return new KeyValuePair<string, IValueFormatter>(parts[0], null);
                            }
                        }).ToArray() : new KeyValuePair<string, IValueFormatter>[0]
                };
            }

            return null;
        }
    }

}
