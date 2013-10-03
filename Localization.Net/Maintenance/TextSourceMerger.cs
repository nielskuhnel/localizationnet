using System.Collections.Generic;

namespace Localization.Net.Maintenance
{
    public class TextSourceMerger
    {
        public ITextSource Source { get; set; }
        public ITextSource Destination { get; set; }

        public TextSourceMerger(ITextSource source, ITextSource destination)
        {
            Source = source;
            Destination = Destination;
        }

        
        public virtual IEnumerable<LocalizedTextState> Diff()
        {
            var destTexts = new Dictionary<string, LocalizedText>();
            foreach (var dest in Destination.Get())
            {
                destTexts[dest.UniqueKey] = dest;
            }

            var srcTexts = new Dictionary<string, LocalizedText>();
            foreach (var source in Source.Get())
            {
                string key = source.UniqueKey;
                srcTexts[key] = source;

                var state = new LocalizedTextState();
                LocalizedText current;
                if (destTexts.TryGetValue(key, out current))
                {
                    if (source.Pattern != current.Pattern)
                    {
                        state.Text = source;
                        state.Status = LocalizedTextStatus.Changed;
                    }
                    else
                    {
                        state.Text = current;
                        state.Status = LocalizedTextStatus.Unchanged;
                    }
                }
                else
                {
                    state.Text = current;
                    state.Status = LocalizedTextStatus.New;
                }

                yield return state;
            }

            foreach (var dest in destTexts)
            {
                if (!srcTexts.ContainsKey(dest.Key))
                {
                    yield return new LocalizedTextState { Text = dest.Value, Status = LocalizedTextStatus.Unused };
                }
            }
        }

        public virtual void Merge(TextMergeOptions options)
        {
            Destination.Put(Diff(), options);
        }
    }
}
