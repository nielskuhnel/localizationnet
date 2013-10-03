using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Localization.Net.Maintenance
{
    public class SimpleTextSource : BatchUpdatableTextSource
    {
        public BindingList<LocalizedText> Texts { get; private set; }

        public SimpleTextSource()
        {
            Texts = new BindingList<LocalizedText>();
            Texts.ListChanged += (sender, args) => OnTextsChanged();
        }        

        public override IEnumerable<LocalizedText> Get()
        {
            return Texts;
        }

        public override void Put(IEnumerable<LocalizedTextState> texts, TextMergeOptions options)
        {
            BeginUpdate();
            Texts.Clear();
            foreach (var text in texts.Select(x => x.Text))
            {
                Texts.Add(text);
            }            
            OnTextsChanged();
        }
    }

}
