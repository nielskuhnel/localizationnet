using System;
using System.IO;

namespace Localization.Net.Web.JavaScript
{
    public interface IJavaScriptGenerator
    {
        void WritePrerequisites(TextWriter writer);

        void WriteEvaluator(object patternProcessor, JavaScriptExpressionWriter writer, params Action[] argumentWriters);
    }
}
