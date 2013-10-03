using System;
using System.Collections.Generic;
using Localization.Net.Support;
using Localization.Net.Configuration;

namespace Localization.Net
{    

    /// <summary>
    /// Helper for localizing exception messages.
    /// </summary>    
    public class ExceptionHelper
    {
        /// <summary>
        /// Gets or sets the key used for finding the localized text.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the default message if no localization entry is found.
        /// </summary>
        /// <value>
        /// The default message.
        /// </value>
        public string DefaultMessage { get; set; }

        /// <summary>
        /// Gets or sets the parameters. These are used in the localization and added as {0}, {1}, {2} etc. for string.Format when the default message is returned
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        private ParameterSet _parameters { get; set; }


        /// <summary>
        /// Gets the exception the helper is for.
        /// </summary>
        public Exception For { get; private set; }


        public ExceptionHelper(Exception forException, string key, string defaultMessage, object parameters)
        {
            Key = key;
            DefaultMessage = defaultMessage;
            For = forException;
            _parameters = ObjectHelper.ParamsToParameterSet(parameters, addWithIndex: true);
        }
        
        public ExceptionHelper AddParameter(string name, object value)
        {
            int i = 0;
            while (_parameters.Contains("" + i))
            {
                ++i;
            }
            _parameters.SetObject("" + i, value);
            _parameters.SetObject(name, value);
            return this;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="baseMessage">The base exceptions message to allow framework texts if not default message is specified.</param>
        /// <returns></returns>
        public string GetMessage(string baseMessage)
        {            
            string text = null;
            var manager = LocalizationConfig.TextManagerResolver != null ?
                LocalizationConfig.TextManagerResolver() : null;

            if (manager != null)
            {
                text = manager.Get(Key, _parameters, 
                    callingAssembly: For.GetType().Assembly,
                    returnNullOnMissing: true,
                    encode: false);
            }

            if (text == null)
            {
                if (!string.IsNullOrEmpty(DefaultMessage))
                {
                    var args = new List<object>();
                    for (int i = 0; _parameters.Contains("" + i); i++)
                    {
                        args.Add(_parameters.GetObject("" + i));
                    }

                    text = string.Format(DefaultMessage, args.ToArray());
                }
                else
                {
                    text = baseMessage;
                }
            }

            return text;
        }
    }
}
