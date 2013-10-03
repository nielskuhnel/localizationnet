using System.Collections.Generic;
using Localization.Net.Processing;

namespace Localization.Net
{
    /// <summary>
    /// A set of parameters used for localizing texts. Keys are case insensitive
    /// </summary>
    public abstract class ParameterSet
    {        
        public ParameterValue this[string key]
        {
            get
            {
                key = key.ToLowerInvariant();
                var val =GetInternal(key);
                return val ?? ParameterValue.Wrap(null);
            }
            set
            {
                key = key.ToLowerInvariant();
                SetInternal(key, value);
            }
        }        

        public object GetObject(string key)
        {
            var val = this[key];
            return val != null ? val.Value : null;
        }

        public void SetObject(string key, object value)
        {            
            this[key] = ParameterValue.Wrap(value);
        }

        public abstract IEnumerable<string> Keys { get; }

        public abstract bool Contains(string key);
        protected abstract ParameterValue GetInternal(string key);
        protected abstract void SetInternal(string key, ParameterValue value);
    }

    public class DicitionaryParameterSet : ParameterSet
    {
        private IDictionary<string, ParameterValue> _values;

        public DicitionaryParameterSet()
        {
            _values = new Dictionary<string, ParameterValue>();
        }

        public DicitionaryParameterSet(IDictionary<string, object> values)
            : this()
        {
            foreach (var val in values)
            {
                SetObject(val.Key, val.Value);
            }
        }

        public override IEnumerable<string> Keys
        {
            get { return _values.Keys; }
        }

        public override bool Contains(string key)
        {
            return _values.ContainsKey(key);
        }

        protected override ParameterValue GetInternal(string key)
        {
            ParameterValue val;
            return _values.TryGetValue(key, out val) ? val : null;
        }

        protected override void SetInternal(string key, ParameterValue value)
        {
            _values[key] = value;
        }

        public static implicit operator DicitionaryParameterSet(Dictionary<string, object> dict)
        {
            return new DicitionaryParameterSet(dict);
        }
    }
}
