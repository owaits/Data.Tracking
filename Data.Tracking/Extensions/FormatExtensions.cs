using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Oarw.Data.Tracking.Extensions
{
    public static class FormatExtensions
    {
        public static string FormatMoney(this decimal? value)
        {
            return (value != null ? ((decimal)value).FormatMoney() : "-");
        }


        public static string FormatMoney(this decimal value)
        {
            try
            {
                return "£" + value.ToString("n2");
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatMoney(this double value)
        {
            return ((decimal)value).FormatMoney();
        }

        public static string FormatPercentage(this decimal? value)
        {
            return (value != null ? ((decimal)value).FormatPercentage() : "-");
        }



        public static string FormatPercentage(this decimal value)
        {
            try
            {
                return Math.Round((value * 100), 0).ToString() + "%";
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatFrequency(this double? value)
        {
            return (value != null ? ((double)value).FormatFrequency() : "-");
        }

        public static string FormatFrequency(this double value)
        {
            try
            {
                return value.ToString("0.000") + "MHz";
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatWeight(this decimal? value)
        {
            return (value != null ? ((decimal)value).FormatWeight() : "-");
        }

        public static string FormatWeight(this decimal value)
        {
            try
            {
                return value.ToString() + "kg";
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatVolume(this decimal? value)
        {
            return (value != null ? ((decimal)value).FormatVolume() : "-");
        }

        public static string FormatVolume(this decimal value)
        {
            try
            {
                return (value / 1000000000).ToString("0.00") + "m³";
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatDimension(this decimal? value)
        {
            try
            {
                return (value != null ? ((decimal)value).ToString("0") + "mm" : "-");
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Formats the date for display in UI. Null datyes will be formated with dash.
        /// </summary>
        /// <param name="value">The date to be formatted.</param>
        /// <returns>The formated date for display in UI.</returns>
        public static string FormatDate(this DateTime? value, string format = null)
        {
            if (value == null)
                return "-";
            return ((DateTime)value).FormatDate(format);
        }

        public static string FormatDate(this DateTime value, string format = null)
        {
            try
            {
                return value.ToString(format ?? "dd/MM/yy");
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatTime(this DateTime? value, string format = null)
        {
            if (value == null)
                return "-";
            return ((DateTime)value).FormatTime(format);
        }

        /// <summary>
        /// Formats a date and time as a time using HH:mm format.
        /// </summary>
        /// <param name="value">The date and time to be formatted.</param>
        /// <param name="format">An optional alternative format to use..</param>
        /// <param name="showTimeZone">Whether the abbreviated time zon name is displayed after the time.</param>
        /// <returns>The time formatted as HH:mm.</returns>
        public static string FormatTime(this DateTime value, string format = null, bool showTimeZone = false)
        {
            try
            {
                var result = value.ToString(format ?? "HH:mm");
                if (showTimeZone)
                {
                    result += " " + value.FormatTimeZone();
                }
                return result;
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Gets the time zone name in its abbreviated form, adjusting for daylight saving hours.
        /// </summary>
        /// <param name="value">The date and time to return the time zone name for.</param>
        /// <returns>The abbreviated time zone name.</returns>
        public static string FormatTimeZone(this DateTime value)
        {
            try
            {
                return value.IsDaylightSavingTime() ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Uses the standard function ToShortTimeString() to convert a date and time but appends the time zone name.
        /// </summary>
        /// <param name="value">The date and time to convert.</param>
        /// <returns>The short time with the time zone appended.</returns>
        public static string ToShortTimeStringWithZone(this DateTime value)
        {
            try
            {
                return value.ToShortTimeString() + " " + value.FormatTimeZone();
            }
            catch (Exception ex)
            {
                return $"[error: {ex.Message}]";
            }
        }

        public static string FormatTimeSpan(this TimeSpan? value)
        {
            if (value == null)
                return "-";
            return ((TimeSpan)value).FormatTimeSpan();
        }

        public static string FormatTimeSpan(this TimeSpan value)
        {
            if (value.Days > 0)
                return $"{value.TotalDays} day{(value.Days > 1 ? "s": string.Empty)}";
            if (value.Hours > 0)
                return $"{value.TotalHours} hour{(value.Hours > 1 ? "s": string.Empty)}";
            if (value.Minutes > 0)
                return $"{value.TotalMinutes} minute{(value.Minutes > 1 ? "s": string.Empty)}";
            return value.ToString();
        }

        /// <summary>
        /// Formats the time span as a human readable string using the interval to determine the units of days, hours or minutes.
        /// </summary>
        /// <remarks>
        /// To specify that the time span is always shown as a faction of days pass in an interval of TimeSpan.FromDays(1).
        /// </remarks>
        /// <param name="value">The value.</param>
        /// <param name="interval">The unit of time to use when displaying the time, an interval of 1 days will ensure the time is displayed as a faction of days.</param>
        /// <returns></returns>
        public static string FormatTimeSpan(this TimeSpan value, TimeSpan interval)
        {
            if (interval.Days > 0)
                return $"{value.TotalDays} day{(value.Days > 1 ? "s" : string.Empty)}";
            if (interval.Hours > 0)
                return $"{value.TotalHours} hour{(value.TotalHours > 1 ? "s" : string.Empty)}";
            if (interval.Minutes > 0)
                return $"{value.TotalMinutes} minute{(value.TotalMinutes > 1 ? "s" : string.Empty)}";
            return value.ToString();
        }

        public static string FormatForCSV(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"-\"";
            }
            else
            {
                //Remove any new line characters
                value = value.Replace("\n", "\r");

                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

        }


        public static int GetOrder<T>(this T enumValue)
where T : struct, IConvertible
        {
            return ((T?)enumValue).GetOrder();
        }

        public static int GetOrder<T>(this T? enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return 0;

            if (enumValue == null)
                return 0;

            int order = 0;
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    order = ((DisplayAttribute)attrs[0]).Order;
                }
            }

            return order;
        }

        public static string FormatName<T>(this T enumValue)
where T : struct, IConvertible
        {
            return ((T?)enumValue).FormatName();
        }

        public static string FormatName<T>(this T? enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            if (enumValue == null)
                return null;

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    var name = ((DisplayAttribute)attrs[0]).Name;
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
            }

            return enumValue.ToString();
        }


        public static string FormatDescription<T>(this T enumValue)
    where T : struct, IConvertible
        {
            return ((T?) enumValue).FormatDescription();
        }

        public static string FormatDescription<T>(this T? enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            if (enumValue == null)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }
    }
}
