using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web;
using System.Globalization;
using Localization.Net.Parsing;
using Localization.Net.Processing;
using Localization.Net.Processing.ParameterEvaluators;
using Localization.Net.Processing.SwitchConditions;
using Localization.Net.Processing.ValueFormatters;
using Localization.Net.Web.JavaScript.FormatGroupExpanders;
using Localization.Net.Web.JavaScript.ParameterEvaluators;
using Localization.Net.Web.JavaScript.SwitchConditions;
using Localization.Net.Web.JavaScript.ValueFormatters;

[assembly: WebResource("Localization.Net.Web.JavaScript.Resources.Localization.js", "text/javascript")]
[assembly: WebResource("Localization.Net.Web.JavaScript.Resources.MicrosoftAjaxGlobalization.js", "text/javascript")]
[assembly: WebResource("Localization.Net.Web.JavaScript.Resources.MicrosoftAjaxCore.js", "text/javascript")]

namespace Localization.Net.Web.JavaScript
{

    public class JavaScriptGenerator
    {        
        public Dictionary<Type, IJavaScriptGenerator> Writers { get; set; }               

        public JavaScriptGenerator()
        {                                    
            Writers = new Dictionary<Type, IJavaScriptGenerator>();

            Writers[typeof(SimpleParameterEvaluator)] = new SimpleParameterGenerator();
            Writers[typeof(ReflectionParameterEvaluator)] = new ReflectionParameterGenerator();
            Writers[typeof(PatternLookupEvaluator)] = new PatternLookupGenerator();
            Writers[typeof(StringFormatFormatter)] = new StringFormatGenerator();
            Writers[typeof(DefaultFormatter)] = new DefaultGenerator();
            Writers[typeof(StringCaseFormatter)] = new StringCaseGenerator();

            Writers[typeof(TakeAllCondition)] = new TakeAllGenerator();
            Writers[typeof(SingleValueCondition<double>)] = new SingleValueGenerator<double>();
            Writers[typeof(SingleValueCondition<string>)] = new SingleValueGenerator<string>();
            Writers[typeof(ValueListCondition<double>)] = new ValueListGenerator<double>();
            Writers[typeof(ValueListCondition<string>)] = new ValueListGenerator<string>();
            Writers[typeof(IntervalCondition<double>)] = new IntervalGenerator<double>();
            Writers[typeof(IntervalCondition<TimeSpan>)] = new TimespanIntervalWriter();
            Writers[typeof(BooleanExpressionCondition)] = new BooleanExpressionGenerator();
            Writers[typeof(ArithmeticCondition)] = new ArithmeticGenerator();

            Writers[typeof(BooleanExpressionCondition)] = new BooleanExpressionGenerator();
            Writers[typeof(LookupCondition)] = new LookupGenerator();

            Writers[typeof(HashTagFormatGroupExpander)] = new HashTagFormatGroupExpanderGenerator();

        }

        public static HtmlString WriteScriptDependencies()
        {
            var sb = new StringBuilder();
            foreach( var dep in ScriptDependencies )
            {
                sb.Append(@"<script type=""text/javascript"" src=""").Append(dep).Append(@"""></script>");
            }
            return new HtmlString(sb.ToString());
        }

        public static IEnumerable<Tuple<Type, string>> ScriptResourceDependencies
        {
            get
            {
                yield return Tuple.Create(typeof (JavaScriptGenerator),
                    "Localization.Net.Web.JavaScript.Resources.Localization.js");
                yield return Tuple.Create(typeof (JavaScriptGenerator),
                    "Localization.Net.Web.JavaScript.Resources.MicrosoftAjaxCore.js");
                yield return Tuple.Create(typeof (JavaScriptGenerator),
                    "Localization.Net.Web.JavaScript.Resources.MicrosoftAjaxGlobalization.js");
            }
        }


        public static IEnumerable<string> ScriptDependencies
        {
            get
            {
                //TODO: This is ugly
                var page = new Page();
                foreach( var dep in ScriptResourceDependencies )
                {
                    yield return page.ClientScript.GetWebResourceUrl(dep.Item1, dep.Item2);
                }                
            }
        }




        private static  Regex _foundationTextKeyMatcher = new Regex(@"^(Enum|Plural|Ago)", RegexOptions.Compiled);

        /// <summary>
        /// Generates JavaScript functions to evaluate patterns client side
        /// </summary>
        /// <param name="manager">The text manager to extract texts from.</param>
        /// <param name="clientClassName">The client name of the generated object. (The script will be var clientClassName = ...)</param>
        /// <param name="output">The generated javascript will be written to this generator.</param>
        /// <param name="language">The language for the generated texts (if different from current language).</param>
        /// <param name="defaultNamespace">The default namespace for texts. (Set this to your assembly's namespace in plugins)</param>
        /// <param name="filter">Specify this to only include a subset of the TextManager's texts.</param>
        /// <param name="includeScriptTags">Wraps the generated script in &lt;script&gt; blocks if <c>true</c>.</param>
        public void WriteScript(TextManager manager, string clientClassName, TextWriter output, LanguageInfo language = null, string defaultNamespace = null, Func<string,string,bool> filter = null, bool includeScriptTags = true)
        {            
            language = language ?? manager.GetCurrentLanguage();
            defaultNamespace = defaultNamespace ?? manager.DefaultNamespace;            
            filter = filter ?? ((ns, key) => true);

            if (includeScriptTags)
            {
                output.Write("<script type='text/javascript'>/*<![CDATA[*/");
            }

            var foundationNamespace = manager.GetNamespace(typeof(TextManager).Assembly);
            Func<string, string, bool> foundationTextFilter = (ns, key) =>
                ns == foundationNamespace &&
                    _foundationTextKeyMatcher.IsMatch(key);                                           



            var texts = manager.CurrentEntries
                .SelectMany(ns => ns.Value
                    .Where(key => foundationTextFilter(ns.Key, key.Key) || filter(ns.Key, key.Key))                    
                    .Select(key =>
                            new
                            {
                                Namespace = ns.Key,
                                Key = key.Key,
                                CacheEntry = manager.GetTextEntry(ns.Key, key.Key, language, true)
                            })).Where(x=>x.CacheEntry != null).ToList();

            var json = new JavaScriptSerializer();

            output.Write("var ");
            output.Write(clientClassName);
            output.Write("=new Localization.Net.TextManager(");
            output.Write(json.Serialize(defaultNamespace));
            output.Write(",");
            output.Write(GetClientCultureInfoSpecification(json, (CultureInfo) language.Culture));            

            //Namespace keys
            output.Write(",{");
            var namespaceKeys = new Dictionary<string, string>();
            int i = 0;
            foreach (var ns in texts.Select(x => x.Namespace).Distinct())
            {
                var key = "" + i;
                namespaceKeys.Add(ns, key);
                if (i++ > 0) output.Write(",");
                output.Write(json.Serialize(ns));
                output.Write(":");
                output.Write(json.Serialize(key));                             
            }

            output.Write("}");

            output.Write(",");
            output.Write(json.Serialize(manager.FallbackNamespaces.ToArray()));

            //Texts (function takes: manager, applySwitch, defaultFormattedValue, htmlEncode, applyFormat, getValue, reflectionParameter
            output.Write(",function(m,sw,dv,e,af,val,rp,sf) {");            
                                            

            //Write prerequisite code                                   

            //Prerequisites for used writers
            var checker = new JavaScriptExpressionChecker(Writers);
            var usedWriters = new HashSet<IJavaScriptGenerator>();
            foreach (var text in texts)
            {
                var exprWriters = checker.CheckExpression(text.CacheEntry.Evaluator.Expression).UsedWriters;
                foreach (var writer in exprWriters)
                {
                    usedWriters.Add(writer);
                }
            }

            foreach (var writer in usedWriters)
            {
                writer.WritePrerequisites(output);
            }            

            bool first = true;        
            output.Write("return {");
            //Write texts
            foreach (var text in texts)
            {        
                if (first) first = false; else output.Write(",\n");                        
                output.Write(json.Serialize(namespaceKeys[text.Namespace] + text.Key));
                output.Write(":");
                //TODO: Maybe it should be put somewhere if the text is a fallback text
                //bool fallback = text.CacheEntry.Text.Language != language.Key;
                Write(text.Namespace, text.Key, language, text.CacheEntry.Evaluator.Expression, output, clientClassName);                
            }
            output.Write("};");

            output.Write("});");

            if (includeScriptTags)
            {
                output.Write("//]]></script>");
            }
        }

        public static string GetClientCultureInfoSpecification(JavaScriptSerializer json, CultureInfo cultureInfo)
        {
            //Reflected: System.Web.Globalization.ClientCultureInfo. Mostly harmless, but locked to the current version's JavaScript (embedded as resources anyway...)
            var clientCulture = new
                                    {
                                        name = cultureInfo.Name,
                                        numberFormat = cultureInfo.NumberFormat,
                                        dateTimeFormat = cultureInfo.DateTimeFormat

                                    };
            return json.Serialize(clientCulture);
        }


        protected void Write(string ns, string key, LanguageInfo language, Expression expr, TextWriter output, string clientClassName)
        {
            var context = new EvaluationContext
            {
                Namespace = ns,
                Language = language,
                StringEncoder = x => x
            };
            var writer = new JavaScriptExpressionWriter(Writers, output, context);
            writer.ClientClassName = clientClassName;
            expr.Accept(writer);            
        }
    }

    public static class JavaScriptHelpers
    {
        /// <summary>
        /// Creates a JavaScript class to use the TextManager's texts client-side
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="clientClassName">Name of the class.</param>
        /// <param name="language">The language for the generated texts (if different from current language).</param>
        /// <param name="defaultNamespace">The default namespace for texts. (Set this to your assembly's namespace in plugins)</param>
        /// <param name="filter">Specify this to only include a subset of the TextManager's texts.</param>
        /// <param name="includeScriptTags">Wraps the generated script in &lt;script&gt; blocks if <c>true</c>.</param>
        /// <returns></returns>
        public static HtmlString WriteScript(this TextManager manager, string clientClassName, LanguageInfo language = null,
            string defaultNamespace = null, Func<string, string, bool> filter = null, bool includeScriptTags = true)
        {
            var generator = new JavaScriptGenerator();
            using (var s = new StringWriter())
            {
                generator.WriteScript(manager, clientClassName, s, language, defaultNamespace, filter, includeScriptTags);
                return new HtmlString(s.ToString());
            }
        }

    }
}
