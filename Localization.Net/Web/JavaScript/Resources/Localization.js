

(function () {

    var ns = this;
    var parts = ["Localization", "Net"];
    for (var i = 0; i < parts.length; i++) {
        var part = parts[i];
        if (!ns[part]) {
            ns[part] = {};
        }
        ns = ns[part];
    }


    ns.TextManager = function (defaultNamespace, cultureInfo, namespaceKeys, fallbackNamespaces, textFunctions) {


        var ParameterCollection = function (args) {
            if (!args) args = {};
            var sp = 0;
            var argsStack = [args];
            var r = {
                get: function (p) { return argsStack[sp][p]; },
                set: function (p, v) { argsStack[sp][p] = v; },
                pushLayer: function () {
                    var nl = {};
                    for (var p in argsStack[sp]) {
                        nl[p] = argsStack[sp][p];
                    }
                    argsStack[++sp] = nl;
                },
                popLayer: function () { --sp; }
            }
            r.g = r.get;

            return r;
        };

        function getInternal(key, args, ns) {
            ns = ns === undefined || ns === null ? defaultNamespace : ns;

            var textKey = namespaceKeys[ns] + key;
            var text = textFunctions[textKey];
            if (text) {
                var ctx = {
                    //parameters collections
                    parameters: new ParameterCollection(args),
                    //namespace
                    namespace: ns
                };
                //shorthand function for parameters.get
                ctx.g = ctx.parameters.get;
                ctx.n = ctx.namepsace;


                if (typeof (text) == "string") {
                    return text;
                } else {
                    var old = Sys.CultureInfo.CurrentCulture;
                    //Change the culture info (MicrosoftAjaxGlobalization.js thing)
                    if (cultureInfo != null)
                        Sys.CultureInfo.CurrentCulture = cultureInfo;
                    var t = text(ctx);
                    Sys.CultureInfo.CurrentCulture = old;
                    return t;
                }
            }
            //Not found
            return null;
        }


        var m = {
            unencoded: function (v) {
                return {
                    _type: "ParameterValue",
                    val: v,
                    format: function (e, formattedValue) { return formattedValue; }
                }
            },
            wrap: function (v, format) {
                return {
                    _type: "ParameterValue",
                    val: v,
                    format: function (e, formattedValue) {
                        return format.replace("{#}", e(formattedValue));
                    }
                };
            },
            get: function (key, args, ns) {
                var text = getInternal(key, args, ns);
                if (text == null) {
                    for (var i = fallbackNamespaces.length - 1; i >= 0; i--) {
                        if ((text = getInternal(key, args, fallbackNamespaces[i])) != null) {
                            break;
                        }
                    }
                }

                return text;
            }
        };

        textFunctions = textFunctions(m,
            ns.TextManager._applySwitch,
            ns.TextManager._defaultFormattedValue,
            ns.TextManager._htmlEncode,
            ns.TextManager._applyFormat,
            ns.TextManager._getValue,
            ns.TextManager._reflectionParameter,
            String.localeFormat /*client side String.Format function for StringFormatGenerator*/);


        return m;
    }

    ns.TextManager._htmlEncode = function (s) {

        return s ? ("" + s).replace(/&/gi, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;") : "";
    }

    ns.TextManager._applySwitch = function (ctx, v, nullExpression, formatter, cases) {

        var value = ns.TextManager._getValue(v);

        if (value === undefined || value === null) {
            return nullExpression;
        } else {
            ctx.parameters.pushLayer();
            var returnVal;
            if (value.constructor.toString().indexOf("Array") != -1) {
                //Enum                
                var sb = [];
                for (var i = 0; i < value.length; i++) {
                    v[i] = ns.TextManager._wrap(v[i]);
                    v[i].defaultFormat = v.defaultFormat;
                    ctx.parameters.set("#", formatter != null ? ns.TextManager._applyFormat(v[i], formatter(val(v[i]))) : v[i]);
                    ctx.parameters.set("#Index", i + 1);
                    sb.push(cases(i, i - v.length));
                }
                returnVal = sb.join("");
            } else {
                ctx.parameters.set("#", formatter != null ? ns.TextManager._applyFormat(v, formatter(value)) : v);
                returnVal = cases(value);
            }
            ctx.parameters.popLayer();

            return returnVal;
        }
    };

    ns.TextManager._wrap = function (val) {
        return val && val._type == "ParameterValue" ? val : {
            _type: "ParameterValue",
            val: val
        };
    };

    ns.TextManager._getValue = function (parameterValue) {
        return parameterValue && parameterValue._type == "ParameterValue" ? parameterValue.val : parameterValue;
    };

    ns.TextManager._applyFormat = function (v, formattedValue, encodeDefault) {
        var e = ns.TextManager._htmlEncode;
        return v._type == "ParameterValue" && v.format ? v.format(e, formattedValue) :
            encodeDefault ? e(formattedValue) : formattedValue;
    };

    ns.TextManager._defaultFormattedValue = function (v) {
        return v && v.defaultFormat ? v.defaultFormat(v) : v;
    };

    ns.TextManager._reflectionParameter = function (o, ps) {
        var v = o;
        if (ps != null) {
            for (var i = 0; i < ps.length && v; i++) {
                var _v = v;
                v = v[ps[i]];
                if (v == null) {
                    v = reflectionBridge(_v, ps[i]);
                }
            }
        }        
        return v;
    };

    //This function provides implicit .NET equivalents to, for now, TimeSpans
    function reflectionBridge(v, p) {
        if (!isNaN(v)) {
            if (p == "TotalDays") {
                return v / (1000 * 60 * 60 * 24);
            } else if (p == "TotalHours") {
                return v / (1000 * 60 * 60);
            } else if (p == "TotalMinutes") {
                return v / (1000 * 60);
            } else if (p == "TotalSeconds") {
                return v / (1000);
            } else if (p == "TotalMilliseconds") {
                return v;
            }

            if (p == "Days") {
                return reflectionBridge(v, "TotalDays");
            } else if (p == "Hours") {
                return v - 24 * reflectionBridge(v, "Days");
            } else if (p == "Minutes") {
                return v - 60 * reflectionBridge(v, "Hours");
            } else if (p == "Seconds") {
                return v - 60 * reflectionBridge(v, "Minutes");
            } else if (p == "Milliseconds") {
                return v - 1000 * reflectionBridge(v, "Seconds");
            }
        }

        return null;
    }
})();