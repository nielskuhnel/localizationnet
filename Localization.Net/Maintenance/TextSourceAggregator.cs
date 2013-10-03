using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Localization.Net.Maintenance
{
    public class TextSourceAggregator : BatchUpdatableTextSource
    {

        public BindingList<PrioritizedTextSource> Sources { get; private set; }

        public TextSourceAggregator()
        {
            Sources = new BindingList<PrioritizedTextSource>();
            Sources.ListChanged += (sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    Sources[args.NewIndex].Source.TextsChanged += SourceChanged;
                }
                else if (args.ListChangedType == ListChangedType.ItemDeleted)
                {
                    Sources[args.NewIndex].Source.TextsChanged -= SourceChanged;
                }
                OnTextsChanged();
            };
        }                        
        
        
        protected void SourceChanged(object sender, EventArgs args)
        {            
            OnTextsChanged();            
        }
        

        public override IEnumerable<LocalizedText> Get()
        {
            var texts = new Dictionary<string, PrioritizedText>();

            if (Sources != null)
            {
                foreach (var source in Sources)
                {
                    foreach (var text in source.Source.Get())
                    {
                        var key = text.UniqueKey;
                        PrioritizedText current;
                        if (!texts.TryGetValue(key, out current) 
                            || (source.Priority > current.Priority && text.Quality >= current.Text.Quality) ) //Prefer texts with better quality
                        {
                            //Use this text if it's new or has higher priority than the current
                            texts[key] = new PrioritizedText { Text = text, Priority = source.Priority };
                        }
                    }                    
                }                               
            }
            

            return texts.Values.Select(x=>x.Text);
        }

        public override void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options)
        {            
            throw new NotSupportedException("This text source does not support saving texts");
        }

        class PrioritizedText
        {
            public int Priority { get; set; }
            public LocalizedText Text { get; set; }
        }
    }

    public class PrioritizedTextSource
    {
        /// <summary>
        /// Bigger is better
        /// </summary>
        public int Priority { get; set; }
        public ITextSource Source { get; set; }

        public PrioritizedTextSource(ITextSource source, int priority = 0)
        {
            Source = source;
            Priority = priority;
        }
    }
}
