using System;
using Localization.Net.Configuration;

namespace Localization.Net
{
    public static class DateTimeHelpers
    {
        /// <summary>
        /// Adjusts the date to the context's current time zone as defined by the current <see cref="TextManager"/>
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime AdjustToTimeZone(this DateTime date)
        {                        
            return date.AdjustToTimeZone(LocalizationConfig.TextManager.GetCurrentTimeZoneInfo());
        }

        /// <summary>
        /// Adjusts the date to the <see cref="TimeZoneInfo"/> specified
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        public static DateTime AdjustToTimeZone(this DateTime date, TimeZoneInfo timeZoneInfo)
        {
            return
                TimeZoneInfo.ConvertTimeFromUtc(date.ToUniversalTime(), timeZoneInfo);
        }
        
    }
}