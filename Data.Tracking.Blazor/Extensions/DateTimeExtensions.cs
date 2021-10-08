using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the date time with the time set to the end of the current day specified by the date.
        /// </summary>
        /// <param name="date">The date to get the end of the day for.</param>
        /// <returns>The end of the current day.</returns>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Gets the date time with the time set to the start of the current day specified by the date.
        /// </summary>
        /// <param name="date">The date to get the start of the day for.</param>
        /// <returns>The start of the current day.</returns>
        public static DateTime StartOfDay(this DateTime date)
        {
            return date.Date;
        }
    }
}
