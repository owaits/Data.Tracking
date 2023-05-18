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

        public static string FormatTime(this DateTime value, string format = null)
        {
            try
            {
                return value.ToString(format ?? "HH:mm");
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
                return $"{value.Days} day{(value.Days > 1 ? "s": string.Empty)}";
            if (value.Hours > 0)
                return $"{value.Hours} hour{(value.Hours > 1 ? "s": string.Empty)}";
            if (value.Minutes > 0)
                return $"{value.Minutes} minute{(value.Minutes > 1 ? "s": string.Empty)}";
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
