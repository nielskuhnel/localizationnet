using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace Localization.Net
{
    [TypeConverter(typeof(LocalizedStringConverter))]
    [DebuggerDisplay("{Value}")]
    public class LocalizedString : IEquatable<LocalizedString>, IEquatable<string>
    {
        private readonly Dictionary<CultureInfo, string> _innerList = new Dictionary<CultureInfo, string>();

        public LocalizedString()
            : this(string.Empty)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedString"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LocalizedString(string value)
        {
            DefaultCulture = Thread.CurrentThread.CurrentCulture;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public CultureInfo DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get { return _innerList[DefaultCulture]; }
            set { _innerList[DefaultCulture] = value; }
        }

        #region IEquatable<LocalizedString> Members

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(LocalizedString other)
        {
            if (other == null)
                return false;

            return Equals(other.Value);
        }

        #endregion

        #region IEquatable<string> Members

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(string other)
        {
            return string.Compare(Value, other, DefaultCulture, CompareOptions.None) == 0;
        }

        #endregion

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public string GetValue(CultureInfo culture)
        {
            if (!_innerList.ContainsKey(culture))
                return null;

            return _innerList[culture];
        }

        /// <summary>
        /// Adds the specified culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        public void Add(CultureInfo culture, string value)
        {
            _innerList[culture] = value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is string) return Equals((string) obj);
            if (obj is LocalizedString) return Equals((LocalizedString) obj);
            return Equals(obj.ToString());
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator LocalizedString(string value)
        {
            return new LocalizedString(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="LocalizedString"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(LocalizedString value)
        {
            return value == null ? null : value.Value;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(LocalizedString left, LocalizedString right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(LocalizedString left, LocalizedString right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static string operator +(LocalizedString left, LocalizedString right)
        {
            return string.Concat(left.Value, right.Value);
        }

        #endregion
    }
}